// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;
using DG.Tweening;
using LoomNetwork.Internal;

namespace LoomNetwork.CZB
{
    public class HealTargetAbility : AbilityBase
    {
        public int value = 1;

        public HealTargetAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();

            _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");
        }

        public override void Update() { }

        public override void Dispose() { }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();

            if (_isAbilityResolved)
            {
                Action();
            }
        }
        public override void Action(object info = null)
        {
            base.Action(info);

            switch (affectObjectType)
            {
                case Enumerators.AffectObjectType.PLAYER:
                    //if (targetPlayer.playerInfo.netId == playerCallerOfAbility.netId)
                    //    CreateAndMoveParticle(() => { playerCallerOfAbility.HealPlayerBySkill(value, false); }, targetPlayer.transform.position);
                    //else
                    //    CreateAndMoveParticle(() => { playerCallerOfAbility.HealPlayerBySkill(value); }, targetPlayer.transform.position);
                    _battleController.HealPlayerByAbility(playerCallerOfAbility, abilityData, targetPlayer);
                    break;
                case Enumerators.AffectObjectType.CHARACTER:
                    _battleController.HealCreatureByAbility(playerCallerOfAbility, abilityData, targetCreature);
                    //  CreateAndMoveParticle(() => { playerCallerOfAbility.HealCreatureBySkill(value, targetCreature.card); }, targetCreature.transform.position);
                    break;
                default: break;
            }
        }

        private void CreateAndMoveParticle(Action callback, Vector3 target)
        {
            target = Utilites.CastVFXPosition(target);
            if (abilityEffectType == Enumerators.AbilityEffectType.HEAL)
            {
                Vector3 startPosition = cardKind == Enumerators.CardKind.CREATURE ? boardCreature.transform.position : selectedPlayer.Transform.position;
                _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellTargetLifeAttack");

                CreateVFX(startPosition);
                _vfxObject.transform.DOMove(target, 0.5f).OnComplete(() => {

                    ClearParticles();
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

                    CreateVFX(target, true);
                    callback();
                });
            }
            else if(abilityEffectType == Enumerators.AbilityEffectType.HEAL_DIRECTLY)
            {
                CreateVFX(target, true);
                callback();
            }
        }
    }
}