using System.Collections.Generic;
using System.Linq;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace LoomNetwork.CZB
{
    public class PlayerController : IController
    {
        private IGameplayManager _gameplayManager;

        private IDataManager _dataManager;

        private IPlayerManager _playerManager;

        private ITutorialManager _tutorialManager;

        private ITimerManager _timerManager;

        private AbilitiesController _abilitiesController;

        private CardsController _cardsController;

        private BattlegroundController _battlegroundController;

        private SkillsController _skillsController;

        private BoardArrowController _boardArrowController;

        private bool _startedOnClickDelay;

        private bool _isPreviewHandCard;

        private float _delayTimerOfClick;

        private bool _cardsZooming;

        private BoardCard _topmostBoardCard;

        private BoardUnit _selectedBoardUnit;

        private PointerEventSolver _pointerEventSolver;

        // private HeroController _heroController;
        public bool IsPlayerStunned { get; set; }

        public bool IsCardSelected { get; set; }

        public bool IsActive { get; set; }

        public void Init()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _timerManager = GameClient.Get<ITimerManager>();

            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _cardsController = _gameplayManager.GetController<CardsController>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _skillsController = _gameplayManager.GetController<SkillsController>();
            _boardArrowController = _gameplayManager.GetController<BoardArrowController>();

            // _heroController = _gameplayManager.GetController<HeroController>();
            _gameplayManager.OnGameStartedEvent += OnGameStartedEventHandler;
            _gameplayManager.OnGameEndedEvent += OnGameEndedEventHandler;

            _pointerEventSolver = new PointerEventSolver();
            _pointerEventSolver.OnDragStartedEvent += PointerEventSolver_OnDragStartedEventHandler;
            _pointerEventSolver.OnClickEvent += PointerEventSolver_OnClickEventHandler;
            _pointerEventSolver.OnEndEvent += PointerEventSolver_OnEndEventHandler;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            if (!_gameplayManager.GameStarted || _gameplayManager.GameEnded)
                return;

            if (_tutorialManager.IsTutorial && _tutorialManager.CurrentStep != 8 && _tutorialManager.CurrentStep != 17 && _tutorialManager.CurrentStep != 19 && _tutorialManager.CurrentStep != 27)
                return;

            _pointerEventSolver.Update();

            HandleInput();
        }

        public void ResetAll()
        {
            StopHandTimer();
            _timerManager.StopTimer(SetStatusZoomingFalse);
        }

        public void InitializePlayer()
        {
            _gameplayManager.CurrentPlayer = new Player(GameObject.Find("Player"), false);

            List<string> playerDeck = new List<string>();

            if (_gameplayManager.IsTutorial)
            {
                playerDeck.Add("GooZilla");
                playerDeck.Add("Burrrnn");
                playerDeck.Add("Burrrnn");
                playerDeck.Add("Burrrnn");
                playerDeck.Add("Azuraz");
            }
            else
            {
                int deckId = _gameplayManager.PlayerDeckId;
                foreach (DeckCardData card in _dataManager.CachedDecksData.Decks.First(d => d.Id == deckId).Cards)
                {
                    for (int i = 0; i < card.Amount; i++)
                    {
#if DEV_MODE

// playerDeck.Add("Whizpar");

// playerDeck.Add("Nail Bomb");
#endif

                        playerDeck.Add(card.CardName);
                    }
                }
            }

            _gameplayManager.CurrentPlayer.SetDeck(playerDeck);

            _gameplayManager.CurrentPlayer.OnStartTurnEvent += OnTurnStartedEventHandler;
            _gameplayManager.CurrentPlayer.OnEndTurnEvent += OnTurnEndedEventHandler;
        }

        public void SetHand()
        {
            _gameplayManager.CurrentPlayer.SetFirstHand(_gameplayManager.IsTutorial);

            GameClient.Get<ITimerManager>().AddTimer(
                x =>
                {
                    _cardsController.UpdatePositionOfCardsForDistribution(_gameplayManager.CurrentPlayer);
                },
                null,
                1f);

            _battlegroundController.UpdatePositionOfCardsInPlayerHand();
        }

        public virtual void OnGameStartedEventHandler()
        {
        }

        public virtual void OnGameEndedEventHandler(Enumerators.EndGameType endGameType)
        {
            IsActive = false;
            IsPlayerStunned = false;
            IsCardSelected = false;
        }

        public void HideCardPreview()
        {
            StopHandTimer();
            _battlegroundController.DestroyCardPreview();

            _delayTimerOfClick = 0f;
            _startedOnClickDelay = false;
            _topmostBoardCard = null;
            _selectedBoardUnit = null;
        }

        public void HandCardPreview(object[] param)
        {
            Vector3 cardPosition = Vector3.zero;

            if (!InternalTools.IsTabletScreen())
            {
                cardPosition = new Vector3(-9f, -3f, 0f);
            }
            else
            {
                cardPosition = new Vector3(-6f, -2.5f, 0f);
            }

            _battlegroundController.CreateCardPreview(param[0], cardPosition, false);
        }

        public void OnTurnEndedEventHandler()
        {
        }

        public void OnTurnStartedEventHandler()
        {
        }

        public void UpdateHandCardsHighlight()
        {
            if (_gameplayManager.CurrentTurnPlayer.Equals(_gameplayManager.CurrentPlayer))
            {
                foreach (BoardCard card in _battlegroundController.PlayerHandCards)
                {
                    if (card.CanBeBuyed(_gameplayManager.CurrentPlayer))
                    {
                        card.SetHighlightingEnabled(true);
                    }
                    else
                    {
                        card.SetHighlightingEnabled(false);
                    }
                }
            }
        }

        private void HandleInput()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                List<GameObject> hitCards = new List<GameObject>();
                bool hitHandCard = false;
                bool hitBoardCard = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.gameObject != null && _battlegroundController.GetBoardCardFromHisObject(hit.collider.gameObject) != null)
                    {
                        hitCards.Add(hit.collider.gameObject);
                        hitHandCard = true;
                    }
                }

                if (!hitHandCard)
                {
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider != null && hit.collider.name.Contains("BoardCreature"))
                        {
                            hitCards.Add(hit.collider.gameObject);
                            hitBoardCard = true;
                        }
                    }
                }

                if (hitHandCard)
                {
                    if (hitCards.Count > 0)
                    {
                        hitCards = hitCards.OrderBy(x => x.GetComponent<SortingGroup>().sortingOrder).ToList();

                        BoardCard topmostBoardCard = _battlegroundController.GetBoardCardFromHisObject(hitCards[hitCards.Count - 1]);
                        if (topmostBoardCard != null && !topmostBoardCard.IsPreview)
                        {
                            float delta = Application.isMobilePlatform?Constants.PointerMinDragDelta * 2f:Constants.PointerMinDragDeltaMobile;
                            _pointerEventSolver.PushPointer(delta);

                            _startedOnClickDelay = true;
                            _isPreviewHandCard = true;
                            _topmostBoardCard = topmostBoardCard;
                        }
                    }
                }
                else if (hitBoardCard)
                {
                    if (hitCards.Count > 0)
                    {
                        StopHandTimer();

                        hitCards = hitCards.OrderBy(x => x.GetComponent<SortingGroup>().sortingOrder).ToList();
                        BoardUnit selectedBoardUnit = _battlegroundController.GetBoardUnitFromHisObject(hitCards[hitCards.Count - 1]);
                        if (selectedBoardUnit != null && (!_battlegroundController.IsPreviewActive || selectedBoardUnit.Card.InstanceId != _battlegroundController.CurrentPreviewedCardId))
                        {
                            float delta = Application.isMobilePlatform?Constants.PointerMinDragDelta * 2f:Constants.PointerMinDragDeltaMobile;
                            _pointerEventSolver.PushPointer(delta);

                            _startedOnClickDelay = true;
                            _isPreviewHandCard = false;
                            _selectedBoardUnit = selectedBoardUnit;
                        }
                    }
                }
                else
                {
                    if (_battlegroundController.IsPreviewActive)
                    {
                        HideCardPreview();
                    }
                    else
                    {
                        _timerManager.StopTimer(SetStatusZoomingFalse);
                        _cardsZooming = true;
                        _timerManager.AddTimer(SetStatusZoomingFalse, null, 1f);

                        _battlegroundController.CardsZoomed = false;
                        _battlegroundController.UpdatePositionOfCardsInPlayerHand();
                    }
                }
            }

            if (_startedOnClickDelay)
            {
                _delayTimerOfClick += Time.deltaTime;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _pointerEventSolver.PopPointer();
            }

            if (_boardArrowController.CurrentBoardArrow != null && _boardArrowController.CurrentBoardArrow.IsDragging())
            {
                _battlegroundController.DestroyCardPreview();
            }
        }

        private void PointerEventSolver_OnDragStartedEventHandler()
        {
            _topmostBoardCard.HandBoardCard.OnSelected();
            if (_tutorialManager.IsTutorial)
            {
                _tutorialManager.DeactivateSelectTarget();
            }

            if (_boardArrowController.CurrentBoardArrow == null)
            {
                HideCardPreview();
            }
        }

        private void PointerEventSolver_OnClickEventHandler()
        {
            if (_battlegroundController.CardsZoomed)
            {
                CheckCardPreviewShow();
            }
            else
            {
                _timerManager.StopTimer(SetStatusZoomingFalse);
                _cardsZooming = true;
                _timerManager.AddTimer(SetStatusZoomingFalse, null, .8f);

                _battlegroundController.CardsZoomed = true;
                _battlegroundController.UpdatePositionOfCardsInPlayerHand();
            }
        }

        private void PointerEventSolver_OnEndEventHandler()
        {
            _delayTimerOfClick = 0f;
            _startedOnClickDelay = false;
        }

        private void CheckCardPreviewShow()
        {
            if (_isPreviewHandCard)
            {
                if (_topmostBoardCard != null && !_cardsZooming)
                {
                    StopHandTimer();
                    _battlegroundController.DestroyCardPreview();

                    if (_boardArrowController.CurrentBoardArrow != null && _boardArrowController.CurrentBoardArrow is AbilityBoardArrow)
                    {
                    }
                    else
                    {
                        HandCardPreview(new object[] { _topmostBoardCard });
                    }
                }
            }
            else
            {
                if (_selectedBoardUnit != null && !_selectedBoardUnit.IsAttacking)
                {
                    StopHandTimer();
                    _battlegroundController.DestroyCardPreview();

                    if (_boardArrowController.CurrentBoardArrow != null && _boardArrowController.CurrentBoardArrow is AbilityBoardArrow)
                    {
                    }
                    else
                    {
                        HandCardPreview(new object[] { _selectedBoardUnit });
                    }
                }
            }
        }

        private void StopHandTimer()
        {
            GameClient.Get<ITimerManager>().StopTimer(HandCardPreview);
        }

        private void SetStatusZoomingFalse(object[] param)
        {
            _cardsZooming = false;
        }
    }
}
