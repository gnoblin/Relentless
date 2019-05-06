using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Loom.ZombieBattleground
{
    public class ValueHistory
    {
        public int ValueInteger;
        public Enumerators.ReasonForValueChange Source;
        public bool Enabled;
        public bool Forced;

        public ValueHistory(int valueInteger, Enumerators.ReasonForValueChange source, bool enabled = true, bool forced = false)
        {
            ValueInteger = valueInteger;
            Source = source;
            Enabled = enabled;
            Forced = forced;
        }
    }
    public class BoardUnitModel : OwnableBoardObject, IInstanceIdOwner
    {
        private static readonly ILog Log = Logging.GetLog(nameof(BoardUnitModel));

        public bool AttackedThisTurn;

        public bool HasFeral;

        public bool HasHeavy;

        public int NumTurnsOnBoard;

        public bool HasUsedBuffShield;

        public bool HasSwing;

        public bool CanAttackByDefault;

        public UniqueList<BoardObject> AttackedBoardObjectsThisTurn;

        public Enumerators.AttackRestriction AttackRestriction = Enumerators.AttackRestriction.ANY;

        private readonly IGameplayManager _gameplayManager;

        private readonly ITutorialManager _tutorialManager;

        private readonly ILoadObjectsManager _loadObjectsManager;

        private readonly BattlegroundController _battlegroundController;

        private readonly BattleController _battleController;

        private readonly ActionsQueueController _actionsQueueController;

        private readonly AbilitiesController _abilitiesController;

        private readonly PlayerController _playerController;

        private readonly IPvPManager _pvpManager;

        private int _stunTurns;

        public bool IsDead { get; private set; }

        public InstanceId InstanceId => Card.InstanceId;

        public override Player OwnerPlayer => Card.Owner;

        public List<Enumerators.SkillTarget> AttackTargetsAvailability;

        public int TutorialObjectId => Card.TutorialObjectId;

        public Sprite CardPicture { get; private set; }

        public BoardUnitModel(WorkingCard card)
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();
            _battleController = _gameplayManager.GetController<BattleController>();
            _actionsQueueController = _gameplayManager.GetController<ActionsQueueController>();
            _abilitiesController = _gameplayManager.GetController<AbilitiesController>();
            _playerController = _gameplayManager.GetController<PlayerController>();
            _pvpManager = GameClient.Get<IPvPManager>();

            BuffsOnUnit = new List<Enumerators.BuffType>();
            AttackedBoardObjectsThisTurn = new UniqueList<BoardObject>();

            CurrentDamageHistory = new List<ValueHistory>();
            CurrentCostHistory = new List<ValueHistory>();

            CurrentDefenseHistory = new List<ValueHistory>();

            IsCreatedThisTurn = true;

            CanAttackByDefault = true;

            UnitSpecialStatus = Enumerators.UnitSpecialStatus.NONE;

            AttackTargetsAvailability = new List<Enumerators.SkillTarget>()
            {
                Enumerators.SkillTarget.OPPONENT,
                Enumerators.SkillTarget.OPPONENT_CARD
            };

            IsAllAbilitiesResolvedAtStart = true;

            SetObjectInfo(card);
        }

        public event Action TurnStarted;

        public event Action TurnEnded;

        public event Action<bool> Stunned;

        public event Action UnitDied;

        public event Action UnitDying;

        public event Action<BoardObject, int, bool> UnitAttacked;

        public event Action UnitAttackedEnded;

        public event Action<BoardObject> UnitDamaged;

        public event Action<BoardObject> PrepairingToDie;

        public event PropertyChangedEvent<int> UnitDefenseChanged;

        public event PropertyChangedEvent<int> UnitDamageChanged;

        public event Action<Enumerators.CardType> CardTypeChanged;

        public event Action<Enumerators.BuffType> BuffApplied;

        public event Action<bool> BuffShieldStateChanged;

        public event Action CreaturePlayableForceSet;

        public event Action UnitFromDeckRemoved;

        public event Action UnitDistracted;

        public event Action<bool> UnitDistractEffectStateChanged;

        public event Action<BoardUnitModel> KilledUnit;

        public event Action<bool> BuffSwingStateChanged;

        public event Action GameMechanicDescriptionsOnUnitChanged;

        public event Action CardPictureWasUpdated;

        public event Action UnitAttackStateFinished;

        public Enumerators.CardType InitialUnitType { get; private set; }

        public List<ValueHistory> CurrentDamageHistory;

        public List<ValueHistory> CurrentDefenseHistory;

        public int BuffedDamage { get; set; }

        public int CurrentDamage
        {
            get => CalculateValueBasedOnHistory(CurrentDamageHistory, Card.Prototype.Damage);
        }

        public int MaxCurrentDamage => Card.Prototype.Damage + BuffedDamage;

        public int BuffedDefense { get; set; }

        public int CurrentCost
        {
            get => CalculateValueBasedOnHistory(CurrentCostHistory, Card.Prototype.Cost);
        }

        public List<ValueHistory> CurrentCostHistory;

        public int CurrentDefense
        {
            get => CalculateValueBasedOnHistory(CurrentDefenseHistory, Card.Prototype.Defense);
        }

        public int MaxCurrentDefense => Card.Prototype.Defense + BuffedDefense;

        public bool IsPlayable { get; set; }

        public WorkingCard Card { get; set; }

        public bool IsStun => _stunTurns > 0;

        public bool IsCreatedThisTurn { get; private set; }

        public List<Enumerators.BuffType> BuffsOnUnit { get; }

        public bool HasBuffRush { get; set; }

        public bool HasBuffHeavy { get; set; }

        public bool HasBuffShield { get; set; }

        public bool TakeFreezeToAttacked { get; set; }

        public int AdditionalDamage { get; set; }

        public int DamageDebuffUntillEndOfTurn { get; set; }

        public int HpDebuffUntillEndOfTurn { get; set; }

        public bool IsAttacking { get; private set; }

        public bool IsAllAbilitiesResolvedAtStart { get; set; }

        public bool IsReanimated { get; set; }

        public bool AttackAsFirst { get; set; }

        public bool AgileEnabled { get; private set; }

        public Enumerators.UnitSpecialStatus UnitSpecialStatus { get; set; }

        public Enumerators.Faction LastAttackingSetType { get; set; }

        public bool CantAttackInThisTurnBlocker { get; set; } = false;

        public IFightSequenceHandler FightSequenceHandler;

        public bool IsHeavyUnit => HasBuffHeavy || HasHeavy;

        public List<Enumerators.GameMechanicDescription> GameMechanicDescriptionsOnUnit { get; } = new List<Enumerators.GameMechanicDescription>();

        public GameplayQueueAction<object> ActionForDying;

        public bool WasDistracted { get; private set; }

        public bool IsUnitActive { get; private set; } = true;

        public int MaximumDamageFromAnySource { get; private set; } = 999;

        // =================== REMOVE HARD

        public Player Owner
        {
            get => Card.Owner;
            set => Card.Owner = value;
        }

        public CardInstanceSpecificData InstanceCard => Card.InstanceCard;

        public IReadOnlyCard Prototype
        {
            get => Card.Prototype   ;
            set => Card.Prototype    = value;
        }

        public string Name => Card.Prototype.Name;

        public Enumerators.Faction Faction => Card.Prototype.Faction;

        // ===================

        private int CalculateValueBasedOnHistory(List<ValueHistory> valueHistory, int initValue = 0)
        {
            int totalValue;
            ValueHistory forcedValue = FindFirstForcedValueInValueHistory(valueHistory);

            if (forcedValue != null)
            {
                totalValue = forcedValue.ValueInteger;
            }
            else
            {
                totalValue = GetBackTotalValueFromValueHistory(valueHistory, initValue);
            }

            totalValue = Mathf.Max(0, totalValue);
            return totalValue;
        }

        public int GetBackTotalValueFromValueHistory (List<ValueHistory> valueHistory, int initValue = 0)
        {
            int totalValue = initValue;

            for (int i = 0; i < valueHistory.Count; i++) 
            {
                if (valueHistory[i].Enabled)
                {
                    totalValue += valueHistory[i].ValueInteger;
                }
            }

            return totalValue;
        }

        public ValueHistory FindFirstForcedValueInValueHistory (List<ValueHistory> valueHistory)
        {
            for (int i = valueHistory.Count-1; i >= 0; i--) 
            {
                if (valueHistory[i].Forced && valueHistory[i].Enabled)
                {
                    return valueHistory[i];
                }
            }

            return null;
        }

        public void DisableBuffsOnValueHistory (List<ValueHistory> valueHistory)
        {
            int oldDefence = CurrentDefense;
            int oldDamage = CurrentDamage;
            for (int i = 0; i < valueHistory.Count; i++)
            {
                if (valueHistory[i].Source == Enumerators.ReasonForValueChange.AbilityBuff)
                {
                    valueHistory[i].Enabled = false;
                }
            }
            UnitDefenseChanged?.Invoke(oldDefence, CurrentDefense);
            UnitDamageChanged?.Invoke(oldDamage, CurrentDamage);
        }

        public void AddToCurrentDamageHistory(int value, Enumerators.ReasonForValueChange reason, bool forced = false)
        {
            int oldValue = CurrentDamage;
            CurrentDamageHistory.Add(new ValueHistory(value, reason, forced: forced));
            UnitDamageChanged?.Invoke(oldValue, CurrentDamage);
        }


        public void AddToCurrentCostHistory(int value, Enumerators.ReasonForValueChange reason, bool forced = false)
        {
            int oldValue = CurrentDamage;
            CurrentCostHistory.Add(new ValueHistory(value, reason, forced: forced));
        }
        public void AddToCurrentDefenseHistory(int value, Enumerators.ReasonForValueChange reason)
        {
            int oldValue = CurrentDefense;
            CurrentDefenseHistory.Add(new ValueHistory(value, reason));
            UnitDefenseChanged?.Invoke(oldValue, CurrentDefense);
        }

        public void HandleDefenseBuffer(int damage)
        {
            if(CurrentDefense - Mathf.Min(damage, MaximumDamageFromAnySource) <= 0 && !HasBuffShield)
            {
                SetUnitActiveStatus(false);
            }
        }

        public void SetUnitActiveStatus(bool isActive)
        {
            IsUnitActive = isActive;
        }

        public void Die(bool forceUnitDieEvent= false, bool withDeathEffect = true, bool updateBoard = true, bool isDead = true)
        {
            UnitDying?.Invoke();

            IsDead = isDead;
            if (!forceUnitDieEvent)
            {
                _battlegroundController.KillBoardCard(this, withDeathEffect, updateBoard);
            }
            else
            {
                InvokeUnitDied();
            }
        }

        public void ResolveBuffShield () {
            if (HasUsedBuffShield) {
                HasUsedBuffShield = false;
                UseShieldFromBuff();
            }
        }

        public void AddBuff(Enumerators.BuffType type)
        {
            if (GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Distract))
            {
                DisableDistract();
            }

            BuffsOnUnit.Add(type);
        }

        public void ApplyBuff(Enumerators.BuffType type, bool ignoreTurnsOnBoard = false)
        {
            switch (type)
            {
                case Enumerators.BuffType.ATTACK:
                    AddToCurrentDamageHistory(1, Enumerators.ReasonForValueChange.AbilityBuff);
                    BuffedDamage++;
                    AddBuff(Enumerators.BuffType.ATTACK);
                    break;
                case Enumerators.BuffType.DAMAGE:
                    break;
                case Enumerators.BuffType.DEFENCE:
                    AddToCurrentDefenseHistory(1, Enumerators.ReasonForValueChange.AbilityBuff);
                    BuffedDefense++;
                    break;
                case Enumerators.BuffType.FREEZE:
                    TakeFreezeToAttacked = true;
                    AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Freeze);
                    break;
                case Enumerators.BuffType.HEAVY:
                    HasBuffHeavy = true;
                    break;
                case Enumerators.BuffType.BLITZ:
                    if ((ignoreTurnsOnBoard || NumTurnsOnBoard == 0) && !HasFeral)
                    {
                        AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Blitz);
                        HasBuffRush = true;
                    }
                    break;
                case Enumerators.BuffType.GUARD:
                    HasBuffShield = true;
                    break;
                case Enumerators.BuffType.REANIMATE:
                    if (!GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Reanimate))
                    {
                        AddBuff(Enumerators.BuffType.REANIMATE);
                        _abilitiesController.BuffUnitByAbility(
                            Enumerators.AbilityType.REANIMATE_UNIT,
                            this,
                            Card.Prototype.Kind,
                            this,
                            OwnerPlayer
                            );
                    }
                    break;
                case Enumerators.BuffType.DESTROY:
                    if (!GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Destroy))
                    {
                        _abilitiesController.BuffUnitByAbility(
                        Enumerators.AbilityType.DESTROY_TARGET_UNIT_AFTER_ATTACK,
                        this,
                        Card.Prototype.Kind,
                        this,
                        OwnerPlayer
                        );
                    }
                    break;
            }

            BuffApplied?.Invoke(type);

            UpdateCardType();
        }

        public void UseShieldFromBuff()
        {
            if (!HasBuffShield)
                return;

            HasBuffShield = false;
            BuffsOnUnit.Remove(Enumerators.BuffType.GUARD);
            BuffShieldStateChanged?.Invoke(false);

            RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Guard);
        }

        public void AddBuffShield()
        {
            AddBuff(Enumerators.BuffType.GUARD);
            HasBuffShield = true;
            BuffShieldStateChanged?.Invoke(true);

            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Guard);
        }

        public void AddBuffSwing()
        {
            HasSwing = true;
            BuffSwingStateChanged?.Invoke(true);

            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.SwingX);
        }

        public void UpdateCardType()
        {
            if (HasBuffHeavy)
            {
                SetAsHeavyUnit();
            }
            else
            {
                switch (InitialUnitType)
                {
                    case Enumerators.CardType.WALKER:
                        SetAsWalkerUnit();
                        break;
                    case Enumerators.CardType.FERAL:
                        SetAsFeralUnit();
                        break;
                    case Enumerators.CardType.HEAVY:
                        SetAsHeavyUnit();
                        break;
                }
            }
        }

        private void ClearUnitTypeEffects()
        {
            RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Heavy);
            RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Feral);
        }

        public void SetAsHeavyUnit()
        {
            if (HasHeavy)
                return;

            if (GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Distract))
            {
                DisableDistract();
            }

            ClearUnitTypeEffects();
            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Heavy);

            HasHeavy = true;
            HasFeral = false;
            InitialUnitType = Enumerators.CardType.HEAVY;
            CardTypeChanged?.Invoke(InitialUnitType);

            if (!AttackedThisTurn && NumTurnsOnBoard == 0)
            {
                IsPlayable = false;
            }
        }

        public void SetAsWalkerUnit()
        {
            if (!HasHeavy && !HasFeral && !HasBuffHeavy)
                return;

            ClearUnitTypeEffects();

            HasHeavy = false;
            HasFeral = false;
            HasBuffHeavy = false;
            InitialUnitType = Enumerators.CardType.WALKER;

            CardTypeChanged?.Invoke(InitialUnitType);
        }

        public void SetAsFeralUnit()
        {
            if (HasFeral)
                return;

            if (GameMechanicDescriptionsOnUnit.Contains(Enumerators.GameMechanicDescription.Distract))
            {
                DisableDistract();
            }

            ClearUnitTypeEffects();
            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Feral);

            HasHeavy = false;
            HasBuffHeavy = false;
            HasFeral = true;
            InitialUnitType = Enumerators.CardType.FERAL;

            if (!AttackedThisTurn && !IsPlayable)
            {
                ForceSetCreaturePlayable();
            }

            CardTypeChanged?.Invoke(InitialUnitType);
        }

        public void SetInitialUnitType()
        {
            HasHeavy = false;
            HasBuffHeavy = false;
            HasFeral = false;

            ClearUnitTypeEffects();

            InitialUnitType = Card.Prototype.Type;

            CardTypeChanged?.Invoke(InitialUnitType);
        }

        public void AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription gameMechanic)
        {
            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.CardWithAbilityPlayed, this, gameMechanic.ToString());

            if (!GameMechanicDescriptionsOnUnit.Contains(gameMechanic))
            {
                GameMechanicDescriptionsOnUnit.Add(gameMechanic);
                GameMechanicDescriptionsOnUnitChanged?.Invoke();
            }
        }

        public void RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription gameMechanic)
        {
            if (GameMechanicDescriptionsOnUnit.Contains(gameMechanic))
            {
                GameMechanicDescriptionsOnUnit.Remove(gameMechanic);
                GameMechanicDescriptionsOnUnitChanged?.Invoke();
            }
        }

        public void ClearEffectsOnUnit()
        {
            GameMechanicDescriptionsOnUnit.Clear();

            GameMechanicDescriptionsOnUnitChanged?.Invoke();
        }

        private void SetObjectInfo(WorkingCard card)
        {
            Card = card;

            BuffedDamage = 0;
            BuffedDefense = 0;

            InitialUnitType = Card.Prototype.Type;

            LastAttackingSetType = Faction;

            ClearUnitTypeEffects();

            SetPicture();
        }

        public void OnStartTurn()
        {
            AttackedBoardObjectsThisTurn.Clear();
            NumTurnsOnBoard++;

            if (_stunTurns > 0)
            {
                _stunTurns--;
            }

            if (_stunTurns == 0)
            {
                IsPlayable = true;
                UnitSpecialStatus = Enumerators.UnitSpecialStatus.NONE;
            }

            if (OwnerPlayer != null && _gameplayManager.CurrentTurnPlayer.Equals(OwnerPlayer))
            {
                if (IsPlayable)
                {
                    AttackedThisTurn = false;

                    IsCreatedThisTurn = false;
                }

                // RANK buff attack should be removed at next player turn
                if (BuffsOnUnit != null)
                {
                    int attackToRemove = BuffsOnUnit.FindAll(x => x == Enumerators.BuffType.ATTACK).Count;

                    if (attackToRemove > 0)
                    {
                        BuffsOnUnit.RemoveAll(x => x == Enumerators.BuffType.ATTACK);

                        BuffedDamage -= attackToRemove;
                        AddToCurrentDamageHistory(-attackToRemove, Enumerators.ReasonForValueChange.Attack);
                    }
                }
            }

            TurnStarted?.Invoke();
        }

        public void OnEndTurn()
        {
            IsPlayable = false;
            if(HasBuffRush)
            {
                RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Blitz);
                HasBuffRush = false;
            }
            CantAttackInThisTurnBlocker = false;
            TurnEnded?.Invoke();
        }

        public void Stun(Enumerators.StunType stunType, int turns = 1)
        {
            if (IsStun)
                return;

            if (AttackedThisTurn || NumTurnsOnBoard == 0 || !_gameplayManager.CurrentTurnPlayer.Equals(OwnerPlayer))
                turns++;

            if (turns > _stunTurns)
            {
                _stunTurns = turns;
            }

            IsPlayable = false;

            UnitSpecialStatus = Enumerators.UnitSpecialStatus.FROZEN;

            Stunned?.Invoke(true);
        }

        public void RevertStun()
        {
            UnitSpecialStatus = Enumerators.UnitSpecialStatus.NONE;
            _stunTurns = 0;
            Stunned?.Invoke(false);
        }

        public void Distract()
        {
            WasDistracted = true;

            AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Distract);

            UpdateVisualStateOfDistract(true);
            UnitDistracted?.Invoke();
        }

        public void DisableDistract()
        {
            RemoveGameMechanicDescriptionFromUnit(Enumerators.GameMechanicDescription.Distract);

            UpdateVisualStateOfDistract(false);
        }

        public void SetAgileStatus(bool status)
        {
            AgileEnabled = status;
        }

        public void SetMaximumDamageToUnit(int maximumDamage)
        {
            MaximumDamageFromAnySource = maximumDamage;
        }

        public void UpdateVisualStateOfDistract(bool status)
        {
            UnitDistractEffectStateChanged?.Invoke(status);
        }

        public void ForceSetCreaturePlayable()
        {
            if (IsStun)
                return;

            IsPlayable = true;
            CreaturePlayableForceSet?.Invoke();
        }

        public virtual bool CanBePlayed(Player owner)
        {
            if (!Constants.DevModeEnabled)
            {
                return _playerController.IsActive; // && owner.manaStat.effectiveValue >= manaCost;
            }
            else
            {
                return true;
            }
        }

        public virtual bool CanBeBuyed(Player owner)
        {
            if (!Constants.DevModeEnabled)
            {
                if (_gameplayManager.AvoidGooCost)
                    return true;

                return owner.CurrentGoo >= CurrentCost;
            }
            else
            {
                return true;
            }
        }

        public void DoCombat(BoardObject target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            IsAttacking = true;

            switch (target)
            {
                case Player targetPlayer:
                    IsPlayable = false;
                    AttackedThisTurn = true;

                    _actionsQueueController.AddNewActionInToQueue(
                        (parameter, completeCallback) =>
                        {
                            if (targetPlayer.Defense <= 0 || !IsUnitActive)
                            {
                                IsPlayable = true;
                                AttackedThisTurn = false;
                                IsAttacking = false;
                                completeCallback?.Invoke();
                                return;
                            }


                            if (_gameplayManager.IsTutorial &&
                                !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().
                                SpecificBattlegroundInfo.DisabledInitialization && OwnerPlayer.IsLocalPlayer)
                            {
                                if (!_tutorialManager.GetCurrentTurnInfo().UseBattleframesSequence.Exists(info => info.TutorialObjectId == TutorialObjectId &&
                                 info.Target == Enumerators.SkillTarget.OPPONENT))
                                {
                                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordTriedToUseUnsequentionalBattleframe);
                                    _tutorialManager.ActivateSelectHandPointer(Enumerators.TutorialObjectOwner.PlayerBattleframe);
                                    IsPlayable = true;
                                    AttackedThisTurn = false;
                                    IsAttacking = false;
                                    completeCallback?.Invoke();
                                    return;
                                }
                            }

                            if (!AttackedBoardObjectsThisTurn.Contains(targetPlayer))
                            {
                                AttackedBoardObjectsThisTurn.Add(targetPlayer);
                            }

                            FightSequenceHandler.HandleAttackPlayer(
                                completeCallback,
                                targetPlayer,
                                () =>
                                {
                                    if(!_pvpManager.UseBackendGameLogic)
                                        _battleController.AttackPlayerByUnit(this, targetPlayer);
                                },
                                () =>
                                {
                                    IsAttacking = false;
                                    UnitAttackedEnded?.Invoke();
                                }
                            );
                        }, Enumerators.QueueActionType.UnitCombat);
                    break;
                case BoardUnitModel targetCardModel:

                    IsPlayable = false;
                    AttackedThisTurn = true;

                    _actionsQueueController.AddNewActionInToQueue(
                        (parameter, completeCallback) =>
                        {
                            targetCardModel.IsAttacking = true;

                            if (targetCardModel.CurrentDefense <= 0 ||
                                targetCardModel.IsDead ||
                                !IsUnitActive ||
                                !targetCardModel.IsUnitActive)
                            {
                                IsPlayable = true;
                                AttackedThisTurn = false;
                                IsAttacking = false;
                                targetCardModel.IsAttacking = false;
                                completeCallback?.Invoke();
                                return;
                            }

                            if (_tutorialManager.IsTutorial && OwnerPlayer.IsLocalPlayer)
                            {
                                if (_tutorialManager.GetCurrentTurnInfo() != null &&
                                    !_tutorialManager.GetCurrentTurnInfo().UseBattleframesSequence.Exists(info =>
                                     info.TutorialObjectId == TutorialObjectId &&
                                     (info.TargetTutorialObjectId == targetCardModel.TutorialObjectId ||
                                         info.TargetTutorialObjectId == 0 && info.Target != Enumerators.SkillTarget.OPPONENT)))
                                {
                                    _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordTriedToUseUnsequentionalBattleframe);
                                    _tutorialManager.ActivateSelectHandPointer(Enumerators.TutorialObjectOwner.PlayerBattleframe);
                                    IsPlayable = true;
                                    AttackedThisTurn = false;
                                    IsAttacking = false;
                                    targetCardModel.IsAttacking = false;
                                    completeCallback?.Invoke();
                                    return;
                                }
                            }

                            ActionForDying = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.UnitDeath, blockQueue: true);
                            targetCardModel.ActionForDying = _actionsQueueController.AddNewActionInToQueue(null, Enumerators.QueueActionType.UnitDeath, blockQueue: true);

                            if (!AttackedBoardObjectsThisTurn.Contains(targetCardModel))
                            {
                                AttackedBoardObjectsThisTurn.Add(targetCardModel);
                            }

                            FightSequenceHandler.HandleAttackCard(
                                completeCallback,
                                targetCardModel,
                                () =>
                                {
                                    _battleController.AttackUnitByUnit(this, targetCardModel, AdditionalDamage);

                                    InvokeUnitAttackStateFinished();
                                    targetCardModel.InvokeUnitAttackStateFinished();

                                    if (HasSwing)
                                    {
                                        List<BoardUnitModel> adjacent = _battlegroundController.GetAdjacentUnitsToUnit(targetCardModel);

                                        foreach (BoardUnitModel unit in adjacent)
                                        {
                                            _battleController.AttackUnitByUnit(this, unit, AdditionalDamage, false);
                                            unit.InvokeUnitAttackStateFinished();
                                        }
                                    }

                                    if (TakeFreezeToAttacked && targetCardModel.CurrentDefense > 0)
                                    {
                                        targetCardModel.Stun(Enumerators.StunType.FREEZE, 1);
                                        targetCardModel.UseShieldFromBuff();
                                    }

                                    targetCardModel.ResolveBuffShield();
                                    this.ResolveBuffShield();                                
                                },
                                () =>
                                {
                                    targetCardModel.IsAttacking = false;
                                    IsAttacking = false;
                                    UnitAttackedEnded?.Invoke();
                                }
                                );
                        }, Enumerators.QueueActionType.UnitCombat);
                    break;
                default:
                    throw new NotSupportedException(target.GetType().ToString());
            }
        }

        public bool UnitCanBeUsable()
        {
            if (IsDead ||
                CurrentDefense <= 0 ||
                CurrentDamage <= 0 ||
                IsStun ||
                CantAttackInThisTurnBlocker ||
                !CanAttackByDefault || !IsUnitActive)
            {
                return false;
            }

            if (IsPlayable)
            {
                if (HasFeral)
                {
                    return true;
                }

                if (NumTurnsOnBoard >= 1)
                {
                    return true;
                }
            }
            else if (!AttackedThisTurn && HasBuffRush)
            {
                return true;
            }

            return false;
        }

        public void MoveUnitFromBoardToDeck()
        {
            try
            {
                Die(true);

                RemoveUnitFromBoard();
            }
            catch (Exception ex)
            {
                Helpers.ExceptionReporter.SilentReportException(ex);
                Log.Warn(ex.ToString());
            }
        }

        public void InvokeUnitDamaged(BoardObject from)
        {
            UnitDamaged?.Invoke(from);
        }

        public void InvokeUnitAttacked(BoardObject target, int damage, bool isAttacker)
        {
            UnitAttacked?.Invoke(target, damage, isAttacker);
        }

        public void InvokeUnitDied()
        {
            UnitDied?.Invoke();
        }

        public void InvokeKilledUnit(BoardUnitModel boardUnit)
        {
            KilledUnit?.Invoke(boardUnit);
        }

        public void InvokeUnitAttackStateFinished()
        {
            UnitAttackStateFinished?.Invoke(); 
        }

        public IReadOnlyList<BoardUnitModel> GetEnemyUnitsList(BoardUnitModel unit)
        {
            if (_gameplayManager.CurrentPlayer.CardsOnBoard.Contains(unit))
            {
                return _gameplayManager.OpponentPlayer.CardsOnBoard;
            }

            return _gameplayManager.CurrentPlayer.CardsOnBoard;
        }

        public void RemoveUnitFromBoard()
        {
            _battlegroundController.BoardUnitViews.Remove(_battlegroundController.GetBoardUnitViewByModel<BoardUnitView>(this));
            OwnerPlayer.PlayerCardsController.RemoveCardFromBoard(this);
            OwnerPlayer.PlayerCardsController.AddCardToGraveyard(this);

            UnitFromDeckRemoved?.Invoke();
        }

        public void InvokeUnitPrepairingToDie()
        {
            PrepairingToDie?.Invoke(this);
        }

        public void SetPicture(string name = "", string attribute = "")
        {
            string imagePath = $"{Constants.PathToCardsIllustrations}";

            if (!string.IsNullOrEmpty(name))
            {
                imagePath += $"{name}";
            }
            else
            {
                imagePath += $"{Prototype.Picture.ToLowerInvariant()}";
            }

            if (!string.IsNullOrEmpty(attribute))
            {
                imagePath += $"_{attribute}";
            }

            CardPicture = _loadObjectsManager.GetObjectByPath<Sprite>(imagePath);
            CardPictureWasUpdated?.Invoke();
        }

        public void ArriveUnitOnBoard()
        {
            switch (InitialUnitType)
            {
                case Enumerators.CardType.FERAL:
                    HasFeral = true;
                    IsPlayable = true;
                    AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Feral);
                    break;
                case Enumerators.CardType.HEAVY:
                    HasHeavy = true;
                    AddGameMechanicDescriptionOnUnit(Enumerators.GameMechanicDescription.Heavy);
                    break;
                case Enumerators.CardType.WALKER:
                default:
                    break;
            }

            if (Card.InstanceCard.Abilities != null)
            {
                foreach (AbilityData ability in Card.InstanceCard.Abilities)
                {
                    TooltipContentData.GameMechanicInfo gameMechanicInfo = GameClient.Get<IDataManager>().GetGameMechanicInfo(ability.GameMechanicDescription);

                    if (gameMechanicInfo != null && !string.IsNullOrEmpty(gameMechanicInfo.Name))
                    {
                        AddGameMechanicDescriptionOnUnit(ability.GameMechanicDescription);
                    }
                }
            }
        }

        public void ResetToInitial()
        {
            Card.InstanceCard.Abilities = new List<AbilityData>(Card.Prototype.Abilities);
            Card.InstanceCard.Cost = Card.Prototype.Cost;
            Card.InstanceCard.Damage = Card.Prototype.Damage;
            Card.InstanceCard.Defense = Card.Prototype.Defense;
            InitialUnitType = Card.Prototype.Type;
            BuffedDefense = 0;
            BuffedDamage = 0;
            NumTurnsOnBoard = 0;
            _stunTurns = 0;
            HpDebuffUntillEndOfTurn = 0;
            DamageDebuffUntillEndOfTurn = 0;
            WasDistracted = false;
            IsCreatedThisTurn = true;
            CanAttackByDefault = true;
            TakeFreezeToAttacked = false;
            HasSwing = false;
            AttackedThisTurn = false;
            HasBuffRush = false;
            HasBuffHeavy = false;
            HasFeral = false;
            IsPlayable = false;
            IsAttacking = false;
            IsDead = false;
            AttackAsFirst = false;
            IsUnitActive = true;
            CantAttackInThisTurnBlocker = false;
            UnitSpecialStatus = Enumerators.UnitSpecialStatus.NONE;
            AttackRestriction = Enumerators.AttackRestriction.ANY;
            LastAttackingSetType = Card.Prototype.Faction;
            BuffsOnUnit.Clear();
            CurrentDamageHistory.Clear();
            CurrentCostHistory.Clear();
            CurrentDefenseHistory.Clear();
            AttackedBoardObjectsThisTurn.Clear();
            UseShieldFromBuff();
            ClearUnitTypeEffects();
            MaximumDamageFromAnySource = 999;
        }

        public bool HasActiveMechanic(Enumerators.GameMechanicDescription gameMechanic)
        {
            return GameMechanicDescriptionsOnUnit.Contains(gameMechanic);
        }

        public bool IsAlive()
        {
            return CurrentDefense > 0 && IsUnitActive && !IsDead;
        }

        public override string ToString()
        {
            return $"({nameof(OwnerPlayer)}: {OwnerPlayer}, {nameof(Card)}: {Card})";
        }
    }
}
