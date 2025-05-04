using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public static unsafe class HeroEffects
    {
        public enum EffectType
        {
            None,
            Bleeding,
            TransferingBleeding,
            IncreaseTakingDamage,
            ReduceCurrentMana,
            IncreaseAttackSpeed,
            ReduceDefense,
            ReduceMagicDefense,
            HorizontalBlast,
            Blast,
            Stun,
            TemporaryArmor,
            Teleport,
            ExtraBaseDamage,
            FirebirdRebirth,
        }

        public enum GlobalEffectType
        {
            None,
            PoisonArea,
            HealArea
        }

        public class Effect
        {
            public EntityRef Owner;
            public EffectType Type = EffectType.None;
            public FP MaxValue = 0;
            public FP Value = 0;
            public FP MaxDuration = 0;
            public FP Duration = 0;
            public int Size = 0;

            public Effect()
            {
                Type = EffectType.None;
            }

            public Effect(EffectQnt effectQnt)
            {
                Owner = effectQnt.Owner;
                Type = (EffectType)effectQnt.Index;
                MaxValue = effectQnt.MaxValue;
                Value = effectQnt.Value;
                MaxDuration = effectQnt.MaxDuration;
                Duration = effectQnt.Duration;
                Size = effectQnt.Size;
            }
        }

        public class GlobalEffect
        {
            public int Center;
            public EntityRef Owner;
            public GlobalEffectType Type;
            public FP Value;
            public FP Duration;
            public int Size;

            public GlobalEffect()
            {
                Type = GlobalEffectType.None;
            }

            public GlobalEffect(GlobalEffectQnt effectQnt)
            {
                Center = effectQnt.Center;
                Owner = effectQnt.Owner;
                Type = (GlobalEffectType)effectQnt.Index;
                Value = effectQnt.Value;
                Duration = effectQnt.Duration;
                Size = effectQnt.Size;
            }
        }

        public static void AddGlobalEffects(Frame f, Board board, QList<GlobalEffectQnt> globalEffectQnts)
        {
            QList<GlobalEffectQnt> globalEffects = f.ResolveList(board.GlobalEffects);

            foreach (GlobalEffectQnt globalEffect in globalEffectQnts)
            {
                globalEffects.Add(globalEffect);

                if (HeroBoard.TryGetHeroCords(globalEffect.Center, out Vector2Int cords) == false)
                {
                    return;
                }

                FPVector3 position = HeroBoard.GetTilePosition(f, cords);

                switch ((GlobalEffectType)globalEffect.Index)
                {
                    case GlobalEffectType.PoisonArea:
                        f.Events.DisplayPoisonEffect(board.Player1.Ref, board.Player2.Ref, position, globalEffect.Size, globalEffect.Duration);
                        break;
                    case GlobalEffectType.HealArea:
                        f.Events.DisplayHealEffect(board.Player1.Ref, board.Player2.Ref, position, globalEffect.Size, globalEffect.Duration);
                        break;
                }
            }
        }

        public static void ProcessGlobalEffects(Frame f, Board board)
        {
            QList<GlobalEffectQnt> globalEffects = f.ResolveList(board.GlobalEffects);

            for (int i = 0; i < globalEffects.Count; i++)
            {
                GlobalEffectQnt globalEffect = globalEffects[i];
                globalEffect.Duration -= f.DeltaTime;

                switch ((GlobalEffectType)globalEffect.Index)
                {
                    case GlobalEffectType.PoisonArea:
                        ProcessPoisonArea(f, globalEffect, board);
                        break;
                    case GlobalEffectType.HealArea:
                        ProcessHealArea(f, globalEffect, board);
                        break;
                }

                if (globalEffect.Duration <= 0)
                {
                    globalEffects.RemoveAt(i);
                    i--;
                }
                else
                {
                    globalEffects[i] = globalEffect;
                }
            }
        }

        public static void ProcessEffects(Frame f, ref FightingHero target, Board board, out bool isStunned)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            target = heroes[target.Index];
            isStunned = false;

            if (f.Exists(target.Hero.Ref) == false || target.IsAlive == false)
            {
                return;
            }

            QList<EffectQnt> effects = f.ResolveList(target.Effects);

            for (int i = 0; i < effects.Count; i++)
            {
                EffectQnt effectQnt = effects[i];
                FightingHero ownerHero = HeroBoard.GetFightingHero(f, effects[i].Owner, board);
                effectQnt.Duration -= f.DeltaTime;

                FP damage = effectQnt.Duration < f.DeltaTime ? effectQnt.Value * effectQnt.Duration : effectQnt.Value * f.DeltaTime;

                switch ((EffectType)effectQnt.Index)
                {
                    case EffectType.Bleeding:
                    case EffectType.TransferingBleeding:
                        HeroAttack.DamageHeroWithDOT(f, ownerHero, board, target, damage, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                        break;
                    case EffectType.ReduceCurrentMana:
                        ReduceCurrentMana(f, target, board, effectQnt.Value);
                        break;
                    case EffectType.Blast:
                        HeroAttack.DamageHeroByBlastWithoutApplyingEffects(f, ownerHero, target.Index, board, effectQnt.Value, effectQnt.Size, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                        break;
                    case EffectType.Stun:
                        isStunned = true;
                        break;
                    case EffectType.Teleport:
                        TeleportHero(f, ref target, effectQnt, board);
                        break;
                    case EffectType.FirebirdRebirth:
                        ProcessFirebirdRebirth(f, target, board, damage);
                        break;
                }

                if (effectQnt.Duration <= 0)
                {
                    if ((EffectType)effectQnt.Index == EffectType.FirebirdRebirth)
                    {
                        if (effectQnt.Size > 0)
                        {
                            target.CurrentHealth = target.Hero.Health / 2;
                            HeroAttack.UpdateHero(f, target, board);

                            Effect effect = new()
                            {
                                Owner = target.Hero.Ref,
                                Type = EffectType.IncreaseAttackSpeed,
                                Value = effectQnt.MaxValue,
                                Duration = 4,
                                Size = 0
                            };

                            HeroAttack.ApplyEffectToTarget(f, ref target, board, ref target, effect);
                        }
                        else
                        {
                            HeroAttack.DestroyHero(f, target, board);
                        }
                    }

                    effects.RemoveAt(i);
                    i--;
                }
                else
                {
                    effects[i] = effectQnt;
                }
            }
        }

        private static void TeleportHero(Frame f, ref FightingHero fightingHero, EffectQnt effectQnt, Board board)
        {
            if (effectQnt.Duration <= 0)
            {
                if (HeroBoard.TryGetCloseEmptyTileInRange(f, effectQnt.Size, board, 8, out Vector2Int newPosition, includeSelf: true) == false)
                {
                    return;
                }

                QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);

                if (Hero.TrySetNewBoardPosition(heroes, ref fightingHero, newPosition) == false)
                {
                    return;
                }

                FPVector3 movePosition = HeroBoard.GetTilePosition(f, newPosition);
                Transform3D* transform = f.Unsafe.GetPointer<Transform3D>(fightingHero.Hero.Ref);

                if (HeroBoard.TrySetTarget(f, ref fightingHero, board))
                {
                    FightingHero targetHero = HeroBoard.GetFightingHero(f, fightingHero.AttackTarget, board);
                    FPVector3 targetHeroPosition = HeroBoard.GetHeroPosition(f, targetHero);
                    FPQuaternion rotation = FPQuaternion.LookRotation(targetHeroPosition - movePosition, FPVector3.Up);
                    transform->Teleport(f, movePosition, rotation);
                }

                transform->Teleport(f, movePosition);
            }
        }

        private static void ProcessPoisonArea(Frame f, GlobalEffectQnt globalEffectQnt, Board board)
        {
            FightingHero ownerHero = HeroBoard.GetFightingHero(f, globalEffectQnt.Owner, board);
            var targets = HeroBoard.GetAllTeamHeroesInRange(f, globalEffectQnt.Center, HeroBoard.GetEnemyTeamNumber(ownerHero.TeamNumber), board, globalEffectQnt.Size, includeSelf: true);

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].IsAlive == false)
                {
                    continue;
                }

                FP damage = globalEffectQnt.Duration < f.DeltaTime ? globalEffectQnt.Value * globalEffectQnt.Duration : globalEffectQnt.Value * f.DeltaTime;
                HeroAttack.DamageHeroWithDOT(f, ownerHero, board, targets[i], damage, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
            }
        }

        private static void ProcessHealArea(Frame f, GlobalEffectQnt globalEffectQnt, Board board)
        {
            FightingHero ownerHero = HeroBoard.GetFightingHero(f, globalEffectQnt.Owner, board);
            var targets = HeroBoard.GetAllTeamHeroesInRange(f, globalEffectQnt.Center, ownerHero.TeamNumber, board, globalEffectQnt.Size, includeSelf: true);

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].IsAlive == false)
                {
                    continue;
                }

                FP damage = globalEffectQnt.Duration < f.DeltaTime ? globalEffectQnt.Value * globalEffectQnt.Duration : globalEffectQnt.Value * f.DeltaTime;
                HeroAttack.HealHeroWithDOT(f, ownerHero, board, targets[i], damage, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
            }
        }

        private static void ReduceCurrentMana(Frame f, FightingHero target, Board board, FP value)
        {
            if (target.AbilityStage > 0)
            {
                return;
            }

            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            target = heroes[target.Index];
            target.CurrentMana -= value;
            heroes[target.Index] = target;
            f.Events.HeroManaChanged(board.Player1.Ref, board.Player2.Ref, target.Hero.Ref, target.CurrentMana, target.Hero.MaxMana);
        }

        private static void ProcessFirebirdRebirth(Frame f, FightingHero target, Board board, FP value)
        {
            var heroes = HeroBoard.GetAllTargetsInRange(f, target, board);

            foreach (var item in heroes)
            {
                HeroAttack.DamageHeroWithDOT(f, target, board, item, value, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
            }
        }
    }
}
