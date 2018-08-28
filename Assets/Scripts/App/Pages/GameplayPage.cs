// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using System;
using System.Linq;

namespace LoomNetwork.CZB
{
    public class GameplayPage : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IPlayerManager _playerManager;
        private IDataManager _dataManager;
        private IGameplayManager _gameplayManager;
        private ISoundManager _soundManager;
        private ITimerManager _timerManager;


        private BattlegroundController _battlegroundController;
        private RanksController _ranksController;

        private GameObject _selfPage,
                           _playedCardPrefab;

        private Button _buttonBack;
        private ButtonShiftingContent _buttonKeep;

        private PlayerManaBarItem _playerManaBar,
                                  _opponentManaBar;

        private List<CardZoneOnBoardStatus> _deckStatus,
                             _graveyardStatus;

        private TextMeshPro _playerHealthText,
                            _opponentHealthText,
                            _playerCardDeckCountText,
                            _opponentCardDeckCountText,
                            _playerNameText,
                            _opponentNameText;

        private SpriteRenderer _playerDeckStatusTexture,
                               _opponentDeckStatusTexture,
                               _playerGraveyardStatusTexture,
                               _opponentGraveyardStatusTexture;

        private GameObject _zippingVFX;

        private int _currentDeckId;

        private bool _isPlayerInited = false;
        private int topOffset;

        private ReportPanelItem _reportGameActionsPanel;

        private GameObject _endTurnButton;

        public OnBehaviourHandler playerPrimarySkillHandler,
                                  playerSecondarySkillHandler;

        public OnBehaviourHandler opponentPrimarySkillHandler,
                                  opponentSecondarySkillHandler;


        public int CurrentDeckId
        {
            set { _currentDeckId = value; }
            get { return _currentDeckId; }
        }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _timerManager = GameClient.Get<ITimerManager>();
           
            _playedCardPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/GraveyardCardPreview");
          //  _cards = new List<CardInGraveyard>();

            _gameplayManager.OnGameInitializedEvent += OnGameInitializedEventHandler;
            _gameplayManager.OnGameEndedEvent += OnGameEndedEventHandler;

            _deckStatus = new List<CardZoneOnBoardStatus>();
            _deckStatus.Add(new CardZoneOnBoardStatus(null, 0));
            _deckStatus.Add(new CardZoneOnBoardStatus(_loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_single"), 15));
            _deckStatus.Add(new CardZoneOnBoardStatus(_loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_couple"), 40));
            _deckStatus.Add(new CardZoneOnBoardStatus(_loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_bunch"), 60));
            _deckStatus.Add(new CardZoneOnBoardStatus(_loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_full"), 80));

            _graveyardStatus = new List<CardZoneOnBoardStatus>();
            _graveyardStatus.Add(new CardZoneOnBoardStatus(null, 0));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(_loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_single"), 10));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(_loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_couple"), 40));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(_loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_bunch"), 75));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(_loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_full"), 100));
        }

        private void OnGameEndedEventHandler(Enumerators.EndGameType endGameType)
        {
          //  ClearGraveyard();

            SetEndTurnButtonStatus(true);

            if (_reportGameActionsPanel != null)
                _reportGameActionsPanel.Clear();
        }

        public void Hide()
        {
            _isPlayerInited = false;

            if (_selfPage == null)
                return;

            _selfPage.SetActive (false);
            _reportGameActionsPanel.Dispose();
            _reportGameActionsPanel = null;
            GameObject.Destroy (_selfPage);
            _selfPage = null;
        }

        public void Dispose()
        {                                                                           

        }

        public void Update()
        {
            if (_selfPage == null || !_selfPage.activeSelf)
                return;

            if (_reportGameActionsPanel != null)
                _reportGameActionsPanel.Update();
        }

        public void Show()
        {
            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/GameplayPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonBack = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _buttonKeep = _selfPage.transform.Find("Button_Keep").GetComponent<ButtonShiftingContent>();

            _buttonBack.onClick.AddListener(BackButtonOnClickHandler);
            _buttonKeep.onClick.AddListener(KeepButtonOnClickHandler);

            _reportGameActionsPanel = new ReportPanelItem(_selfPage.transform.Find("ActionReportPanel").gameObject);

            if (_zippingVFX == null)
            {
                _zippingVFX = GameObject.Find("Background/Zapping").gameObject;
                _zippingVFX.SetActive(false);
            }
                
            StartGame();
            KeepButtonVisibility(false);
        }



        public void SetEndTurnButtonStatus(bool status)
        {
            _endTurnButton.GetComponent<EndTurnButton>().SetEnabled(status);
            // _endTurnButton.SetActive(status);
        }

        //public void ClearGraveyard()
        //{
        //    foreach (var item in _cards)
        //    {
        //        item.Dispose();
        //    }
        //    _cards.Clear();
        //}

        //TODO: pass parameters here and apply corresponding texture, since previews have not the same textures as cards
       

        private void DelayedCardDestroy(object[] card)
        {
            BoardUnit cardToDestroy = (BoardUnit)card[0];
            if (cardToDestroy != null)
            {
                cardToDestroy.transform.DOKill();
                GameObject.Destroy(cardToDestroy.gameObject);
            }
        }

        public void StartGame()
        {
            if (_battlegroundController == null)
            {
                _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

                _battlegroundController.OnPlayerGraveyardUpdatedEvent += OnPlayerGraveyardUpdatedEventHandler;
                _battlegroundController.OnOpponentGraveyardUpdatedEvent += OnOpponentGraveyardUpdatedEventHandler;

                _ranksController = _gameplayManager.GetController<RanksController>();
            }

            int deckId = _gameplayManager.PlayerDeckId = _currentDeckId;

            OpponentDeck randomOpponentDeck = 
                _dataManager.CachedOpponentDecksData.decks[UnityEngine.Random.Range(0, _dataManager.CachedOpponentDecksData.decks.Count)];
            _gameplayManager.OpponentDeckId = randomOpponentDeck.id;

            int heroId = Constants.TUTORIAL_PLAYER_HERO_ID; // TUTORIAL

            if (!_gameplayManager.IsTutorial)
                heroId = _dataManager.CachedDecksData.decks.First(o => o.id == _currentDeckId).heroId;

            int opponentHeroId = randomOpponentDeck.heroId;

            Hero currentPlayerHero = _dataManager.CachedHeroesData.Heroes[heroId];
            Hero currentOpponentHero = _dataManager.CachedHeroesData.Heroes[opponentHeroId];


            _playerDeckStatusTexture = GameObject.Find("Player/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _opponentDeckStatusTexture = GameObject.Find("Opponent/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _playerGraveyardStatusTexture = GameObject.Find("Player/Graveyard_Illustration/Graveyard").GetComponent<SpriteRenderer>();
            _opponentGraveyardStatusTexture = GameObject.Find("Opponent/Graveyard_Illustration/Graveyard").GetComponent<SpriteRenderer>();

            _playerHealthText = GameObject.Find("Player/Avatar/LivesCircle/DefenceText").GetComponent<TextMeshPro>();
            _opponentHealthText = GameObject.Find("Opponent/Avatar/LivesCircle/DefenceText").GetComponent<TextMeshPro>();

            _playerManaBar = new PlayerManaBarItem(GameObject.Find("PlayerManaBar"), "GooOverflowPlayer", new Vector3(-3.55f, 0, -6.07f));
            _opponentManaBar = new PlayerManaBarItem(GameObject.Find("OpponentManaBar"), "GooOverflowOpponent", new Vector3(9.77f, 0, 4.75f));


            // improve find to get it from OBJECTS ON BOARD!!
            _playerNameText = GameObject.Find("Player/NameBoard/NameText").GetComponent<TextMeshPro>();
            _opponentNameText = GameObject.Find("Opponent/NameBoard/NameText").GetComponent<TextMeshPro>();


            _playerCardDeckCountText = GameObject.Find("Player/CardDeckText").GetComponent<TextMeshPro>();
            _opponentCardDeckCountText = GameObject.Find("Opponent/CardDeckText").GetComponent<TextMeshPro>();

            _endTurnButton = GameObject.Find("EndTurnButton");

            playerPrimarySkillHandler = GameObject.Find("Player/Object_SpellPrimary").GetComponent<OnBehaviourHandler>();
            playerSecondarySkillHandler = GameObject.Find("Player/Object_SpellSecondary").GetComponent<OnBehaviourHandler>();

            opponentPrimarySkillHandler = GameObject.Find("Opponent/Object_SpellPrimary").GetComponent<OnBehaviourHandler>();
            opponentSecondarySkillHandler = GameObject.Find("Opponent/Object_SpellSecondary").GetComponent<OnBehaviourHandler>();

            if (currentPlayerHero != null)
            {
                SetHeroInfo(currentPlayerHero, "Player", playerPrimarySkillHandler.gameObject, playerSecondarySkillHandler.gameObject);
                _playerNameText.text = currentPlayerHero.FullName;
            }
            if (currentOpponentHero != null)
            {
                SetHeroInfo(currentOpponentHero, "Opponent", opponentPrimarySkillHandler.gameObject, opponentSecondarySkillHandler.gameObject);
                _opponentNameText.text = currentOpponentHero.FullName;
            }

            _isPlayerInited = true;
        }

        public void SetHeroInfo(Hero hero, string objectName, GameObject skillPrimary, GameObject skillSecondary)
        {
            var skillPrim = hero.skills[hero.primarySkill];
            var skillSecond = hero.skills[hero.secondarySkill];

            skillPrimary.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/heroability_" + hero.heroElement.ToString() +"_" + skillPrim.skill.ToLower());
            skillSecondary.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/heroability_" + hero.heroElement.ToString() + "_" + skillSecond.skill.ToLower());

            var heroTexture = _loadObjectsManager.GetObjectByPath<Texture2D>("Images/Heroes/CZB_2D_Hero_Portrait_" + hero.heroElement.ToString() + "_EXP");
            var transfHeroObject = GameObject.Find(objectName + "/Avatar/Hero_Object").transform;

            Material heroAvatarMaterial = new Material(Shader.Find("Sprites/Default"));
            heroAvatarMaterial.mainTexture = heroTexture;

            for (int i = 0; i < transfHeroObject.childCount; i++)
                transfHeroObject.GetChild(i).GetComponent<Renderer>().material = heroAvatarMaterial;

            var heroHighlight = _loadObjectsManager.GetObjectByPath<Sprite>
                ("Images/Heroes/CZB_2D_Hero_Decor_" + hero.heroElement.ToString() + "_EXP");
            GameObject.Find(objectName + "/Avatar/HeroHighlight").GetComponent<SpriteRenderer>().sprite = heroHighlight;
        }


        public void SetPlayerDeckCards(int cards)
        {
            _playerCardDeckCountText.text = cards.ToString();
            if (cards == 0 && _playerDeckStatusTexture.gameObject.activeInHierarchy)
                _playerDeckStatusTexture.gameObject.SetActive(false);
        }

        public void SetOpponentDeckCards(int cards)
        {
            _opponentCardDeckCountText.text = cards.ToString();
            if (cards == 0 && _opponentDeckStatusTexture.gameObject.activeInHierarchy)
                _opponentDeckStatusTexture.gameObject.SetActive(false);
        }


        private int GetPercentFromMaxDeck(int index)
        {
            return 100 * index / (int)Constants.DECK_MAX_SIZE;
        }

        #region event handlers

        private void OnGameInitializedEventHandler()
        {
            var player = _gameplayManager.CurrentPlayer;
            var opponent = _gameplayManager.OpponentPlayer;

            player.DeckChangedEvent += OnPlayerDeckChangedEventHandler;
            player.PlayerHPChangedEvent += OnPlayerHPChanged;
            player.PlayerGooChangedEvent += OnPlayerGooChanged;
            player.PlayerVialGooChangedEvent += OnPlayerVialGooChanged;
            opponent.DeckChangedEvent += OnOpponentDeckChangedEventHandler;
            opponent.PlayerHPChangedEvent += OnOpponentHPChanged;
            opponent.PlayerGooChangedEvent += OnOpponentGooChanged;
            opponent.PlayerVialGooChangedEvent += OnOpponentVialGooChanged;

            player.OnStartTurnEvent += OnStartTurnEventHandler;

            OnPlayerDeckChangedEventHandler(player.CardsInDeck.Count);
            OnPlayerHPChanged(player.HP);
            OnPlayerGooChanged(player.Goo);
            OnPlayerVialGooChanged(player.GooOnCurrentTurn);
            OnOpponentDeckChangedEventHandler(opponent.CardsInDeck.Count);
            OnOpponentHPChanged(opponent.HP);
            OnOpponentGooChanged(opponent.GooOnCurrentTurn);
            OnOpponentVialGooChanged(opponent.GooOnCurrentTurn);
        }

        private void OnPlayerDeckChangedEventHandler(int index)
        {
            if (!_isPlayerInited)
                return;

            _playerCardDeckCountText.text = index.ToString();

            if (index == 0)
                _playerDeckStatusTexture.sprite = _deckStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearest = _deckStatus.OrderBy(x => Math.Abs(x.percent - percent)).Where(y => y.percent > 0).First();

                _playerDeckStatusTexture.sprite = nearest.statusSprite;
            }
        }

        private void OnPlayerGraveyardUpdatedEventHandler(int index)
        {
            if (!_isPlayerInited)
                return;

            if (index == 0)
                _playerGraveyardStatusTexture.sprite = _graveyardStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearestObjects = _graveyardStatus.OrderBy(x => Math.Abs(x.percent - percent)).Where(y => y.percent > 0).ToList();

                CardZoneOnBoardStatus nearest = null;

                if (nearestObjects[0].percent > 0)
                    nearest = nearestObjects[0];
                else
                    nearest = nearestObjects[1];

                _playerGraveyardStatusTexture.sprite = nearest.statusSprite;
            }
        }

        private void OnOpponentDeckChangedEventHandler(int index)
        {
            if (!_isPlayerInited)
                return;

            _opponentCardDeckCountText.text = index.ToString();

            if (index == 0)
                _opponentDeckStatusTexture.sprite = _deckStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearest = _deckStatus.OrderBy(x => Math.Abs(x.percent - percent)).Where(y => y.percent > 0).First();

                _opponentDeckStatusTexture.sprite = nearest.statusSprite;
            }
        }

        private void OnOpponentGraveyardUpdatedEventHandler(int index)
        {
            if (!_isPlayerInited)
                return;

            if (index == 0)
                _opponentGraveyardStatusTexture.sprite = _deckStatus.Find(x => x.percent == index).statusSprite;
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                var nearestObjects = _graveyardStatus.OrderBy(x => Math.Abs(x.percent - percent)).Where(y => y.percent > 0).ToList();

                CardZoneOnBoardStatus nearest = null;

                if (nearestObjects[0].percent > 0)
                    nearest = nearestObjects[0];
                else
                    nearest = nearestObjects[1];

                _opponentGraveyardStatusTexture.sprite = nearest.statusSprite;
            }
        }

        private void OnPlayerHPChanged(int health)
        {
            if (!_isPlayerInited)
                return;
            _playerHealthText.text = health.ToString();

            if (health > 9)
                _playerHealthText.color = Color.white;
            else
                _playerHealthText.color = Color.red;
        }

        private void OnPlayerGooChanged(int goo)
        {
            if (!_isPlayerInited)
                return;
            _playerManaBar.SetGoo(goo);
        }

        private void OnPlayerVialGooChanged(int currentTurnGoo)
        {
            if (!_isPlayerInited)
                return;
            _playerManaBar.SetVialGoo(currentTurnGoo);
        }

        private void OnOpponentHPChanged(int health)
        {
            if (!_isPlayerInited)
                return;
            _opponentHealthText.text = health.ToString();

            if (health > 9)
                _opponentHealthText.color = Color.white;
            else
                _opponentHealthText.color = Color.red;
        }

        private void OnOpponentGooChanged(int goo)
        {
            if (!_isPlayerInited)
                return;
            _opponentManaBar.SetGoo(goo);
        }

        private void OnOpponentVialGooChanged(int currentTurnGoo)
        {
            if (!_isPlayerInited)
                return;
            _opponentManaBar.SetVialGoo(currentTurnGoo);
        }


        private void OnStartTurnEventHandler()
        {
            _zippingVFX.SetActive(_gameplayManager.GetController<PlayerController>().IsActive);
        }


        #endregion


        #region Buttons Handlers
        public void BackButtonOnClickHandler()
        {
            Action callback = () =>
            {
                _uiManager.HidePopup<YourTurnPopup>();

                _gameplayManager.EndGame(Enumerators.EndGameType.CANCEL);
                GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU);

                _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);
                _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);
            };

            _uiManager.DrawPopup<ConfirmationPopup>(callback);
            _soundManager.PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
        }

        public void KeepButtonOnClickHandler()
        {
            _gameplayManager.GetController<CardsController>().EndCardDistribution();
            KeepButtonVisibility(false);
        }

        public void KeepButtonVisibility(bool visible)
        {
            _buttonKeep.gameObject.SetActive(visible);
        }

        #endregion

        class CardInGraveyard
        {
            public GameObject selfObject;
            public Image image;

            public CardInGraveyard(GameObject gameObject, Sprite sprite = null)
            {
                selfObject = gameObject;
                image = selfObject.transform.Find("Image").GetComponent<Image>();

                if (sprite != null)
                    image.sprite = sprite;
            }

            public void Dispose()
            {
                if (selfObject != null)
                    GameObject.Destroy(selfObject);
            }
        }

        class CardZoneOnBoardStatus
        {
            public int percent;
            public Sprite statusSprite;

            public CardZoneOnBoardStatus(Sprite statusSprite, int percent)
            {
                this.statusSprite = statusSprite;
                this.percent = percent;
            }
        }
    }
}