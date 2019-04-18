using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class AttackOverlordAbility : AbilityBase
    {
        public int Value { get; }

        public List<Enumerators.Target> TargetTypes { get; }

        public AttackOverlordAbility() : base()
        {

        }

        public AttackOverlordAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            TargetTypes = ability.Targets;
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            InvokeActionTriggered();
        }

        protected override void UnitDiedHandler()
        {
            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
            {
                base.UnitDiedHandler();
                return;
            }

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            InvokeActionTriggered();
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityTrigger != Enumerators.AbilityTrigger.ATTACK || !isAttacker)
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            InvokeActionTriggered();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.END ||
                !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

            InvokeActionTriggered();
        }

        protected override void ChangeRageStatusAction(bool status)
        {
            base.ChangeRageStatusAction(status);

            if (AbilityTrigger != Enumerators.AbilityTrigger.RAGE)
                return;

            if (status)
            {
                AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

                InvokeActionTriggered();
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            Player targetObject = null; 

            foreach (Enumerators.Target target in TargetTypes)
            {
                switch (target)
                {
                    case Enumerators.Target.OPPONENT:
                        targetObject = GetOpponentOverlord();
                        break;
                    case Enumerators.Target.PLAYER:
                        targetObject = PlayerCallerOfAbility;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }

                if(!PvPManager.UseBackendGameLogic)
                    BattleController.AttackPlayerByAbility(AbilityUnitOwner, AbilityData, targetObject);
            }

            if (AbilityTrigger == Enumerators.AbilityTrigger.DEATH)
            {
                base.UnitDiedHandler();
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = Enumerators.ActionType.CardAffectingOverlord,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = targetObject,
                            HasValue = true,
                            Value = -AbilityData.Value
                        }
                    }
            });

            AbilityProcessingAction?.ForceActionDone();
        }

        public void ActivateAbility(AttackOverlordOutcome outcome)
        {
            BoardObject boardObject = BattlegroundController.GetBoardObjectByInstanceId(outcome.PlayerInstanceId);
            if (boardObject is Player targetOverlord)
            {
                BattleController.AttackPlayer(targetOverlord, outcome.Damage);
                targetOverlord.Defense = outcome.NewDefence;
            }
            else
            {
                throw new Exception("Attack overlord should apply only on Player Overlord");
            }
        }
    }

    public class AttackOverlordOutcome
    {
        public InstanceId PlayerInstanceId;
        public int Damage;
        public int NewDefence;
    }
}
