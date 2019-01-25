using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Loom.ZombieBattleground
{
    public class DeckBuilderCard : MonoBehaviour, IScrollHandler
    {
        public HordeEditingPage Page;

        public IReadOnlyCard Card;

        public bool IsHordeItem;

        private MultiPointerClickHandler _multiPointerClickHandler;

        public void OnScroll(PointerEventData eventData)
        {
            Page?.ScrollCardList(IsHordeItem, eventData.scrollDelta);
        }

        private void Awake()
        {
            _multiPointerClickHandler = gameObject.AddComponent<MultiPointerClickHandler>();
            _multiPointerClickHandler.SingleClickReceived += SingleClickAction;
            _multiPointerClickHandler.DoubleClickReceived += DoubleClickAction;
        }

        private void SingleClickAction()
        {
            Page?.SelectCard(this, Card);
        }

        private void DoubleClickAction()
        {
            if (GameClient.Get<ITutorialManager>().IsTutorial &&
                !GameClient.Get<ITutorialManager>().CurrentTutorial.IsGameplayTutorial() &&
                (GameClient.Get<ITutorialManager>().CurrentTutorialStep.ToMenuStep().CardsInteractingLocked ||
                !GameClient.Get<ITutorialManager>().CurrentTutorialStep.ToMenuStep().CanDoubleTapCards))
                return;

            if (!IsHordeItem)
            {
                Page?.AddCardToDeck(this, Card);

                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.CardAdded);
            }
            else
            {
                Page?.RemoveCardFromDeck(this, Card);

                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.CardRemoved);
            }
        }
    }
}
