using System;
using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Object = UnityEngine.Object; 

namespace Loom.ZombieBattleground
{
    public class DamageTargetAbility : AbilityBase
    {
        public int Value { get; }

        public DamageTargetAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
            TargetUnitStatusType = ability.TargetUnitStatusType;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            DealDamageToUnitOwner();
        }

        protected override void InputEndedHandler()
        {
            base.InputEndedHandler();

            if (IsAbilityResolved)
            {
                InvokeActionTriggered();
            }
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.END ||
                !GameplayManager.CurrentTurnPlayer.Equals(PlayerCallerOfAbility))
                return;

            DealDamageToUnitOwner();
        }

        private void DealDamageToUnitOwner()
        {
            if (AbilityTargetTypes.Contains(Enumerators.AbilityTargetType.ITSELF))
            {
                if (GetCaller() == AbilityUnitOwner)
                {
                    AbilityProcessingAction = ActionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.AbilityUsageBlocker);

                    BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, AbilityUnitOwner);

                    AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Character);

                    ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
                    {
                        ActionType = Enumerators.ActionType.CardAffectingCard,
                        Caller = GetCaller(),
                        TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                        {
                            new PastActionsPopup.TargetEffectParam()
                            {
                                ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                                Target = AbilityUnitOwner,
                                HasValue = true,
                                Value = -AbilityData.Value
                            }
                        }
                    });

                    AbilityProcessingAction?.ForceActionDone();
                } 
            }
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            object caller = GetCaller();

            object target = null;

            Enumerators.ActionType actionType;

            switch (AffectObjectType)
            {
                case Enumerators.AffectObjectType.Player:
                    BattleController.AttackPlayerByAbility(caller, AbilityData, TargetPlayer);
                    AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                    {
                        TargetPlayer
                    }, AbilityData.AbilityType, Enumerators.AffectObjectType.Player);

                    target = TargetPlayer;
                    actionType = Enumerators.ActionType.CardAffectingOverlord;
                    break;
                case Enumerators.AffectObjectType.Character:
                    BattleController.AttackUnitByAbility(caller, AbilityData, TargetUnit);
                    AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>()
                    {
                        TargetUnit
                    }, AbilityData.AbilityType, Enumerators.AffectObjectType.Character);

                    target = TargetUnit;
                    actionType = Enumerators.ActionType.CardAffectingCard;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(AffectObjectType), AffectObjectType, null);
            }

            ActionsQueueController.PostGameActionReport(new PastActionsPopup.PastActionParam()
            {
                ActionType = actionType,
                Caller = GetCaller(),
                TargetEffects = new List<PastActionsPopup.TargetEffectParam>()
                    {
                        new PastActionsPopup.TargetEffectParam()
                        {
                            ActionEffectType = Enumerators.ActionEffectType.ShieldDebuff,
                            Target = target,
                            HasValue = true,
                            Value = -AbilityData.Value
                        }
                    }
            });
        }
    }
}
