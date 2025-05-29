using System.Collections.Generic;
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
            Bleeding, // Owner, Type, Value, Duration
            TransferingBleeding, // Owner, Type, Value, MaxDuration, Duration, Size
            IncreaseTakingDamage, // Owner, Type, Value, Duration (multiply)
            IncreaseCurrentMana, // Owner, Type, Value (additive)
            IncreaseManaIncome, // Owner, Type, Value (multiply)
            IncreaseHealAndArmor, // Owner, Type, Value, Duration (multiply)
            IncreaseAttackSpeed, // Owner, Type, Value, Duration (multiply)
            IncreaseOutgoingDamage, // Owner, Type, Value, Duration (multiply)
            ReduceDefense, // Owner, Type, Value, Duration (additive)
            ReduceMagicDefense, // Owner, Type, Value, Duration (additive)
            HorizontalBlast, // Owner, Type, Value, Size
            Blast, // Owner, Type, Value, Size
            Stun, // Owner, Type, Duration
            BlastStun, // Owner, Type, Duration, Size
            TemporaryArmor, // Owner, Type, Value, Duration
            Teleport, // Owner, Type, Size (position to teleport), Duration (teleport delay)
            ExtraBaseDamage, // Owner, Type, Value, Duration, Size (additive)
            Silence, // Owner, Type, Duration
            BlastSilence, // Owner, Type, Duration, Size
            Immortal, // Owner, Type, Duration
            Thorns, // Owner, Type, Value, Duration (percentage of damage taken)
            Delayed, // Type, Duration, ... (Other parameters are the same as in required EffectType)
        }

        public enum GlobalEffectType
        {
            None,
            PoisonArea, // Owner, Type, Value, Center, Duration, Size
            HealArea, // Owner, Type, Value, Center, Duration, Size
            TauntedArea, // Owner, Type, Duration, Size
        }

        public class Effect
        {
            public EntityRef Owner;
            public EffectType Type = EffectType.None;
            public EffectType DelayedType = EffectType.None;
            public FP DurationAfterDelay = 0;
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
            foreach (GlobalEffectQnt globalEffect in globalEffectQnts)
            {
                AddGlobalEffect(f, board, globalEffect);
            }
        }

        public static void AddGlobalEffect(Frame f, Board board, GlobalEffect globalEffect)
        {
            GlobalEffectQnt globalEffectQnt = new()
            {
                Center = globalEffect.Center,
                Owner = globalEffect.Owner,
                Index = (int)globalEffect.Type,
                Value = globalEffect.Value,
                Duration = globalEffect.Duration,
                Size = globalEffect.Size
            };

            AddGlobalEffect(f, board, globalEffectQnt);
        }

        public static void AddGlobalEffect(Frame f, Board board, GlobalEffectQnt globalEffect)
        {
            QList<GlobalEffectQnt> globalEffects = f.ResolveList(board.GlobalEffects);

            globalEffects.Add(globalEffect);

            if (HeroBoard.TryGetHeroCordsFromIndex(globalEffect.Center, out Vector2Int cords) == false)
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

        public static bool TryProcessTauntEffect(Frame f, FightingHero fightingHero, Board board, out FightingHero tauntTarget, out Vector2Int moveTargetPosition)
        {
            QList<GlobalEffectQnt> globalEffects = f.ResolveList(board.GlobalEffects);

            for (int i = 0; i < globalEffects.Count; i++)
            {
                GlobalEffectQnt globalEffectQnt = globalEffects[i];

                if (globalEffectQnt.Index == (int)GlobalEffectType.TauntedArea)
                {
                    if (f.Exists(globalEffectQnt.Owner) == false)
                    {
                        continue;
                    }

                    tauntTarget = HeroBoard.GetFightingHero(f, globalEffectQnt.Owner, board);

                    if (tauntTarget.TeamNumber == fightingHero.TeamNumber)
                    {
                        continue;
                    }

                    if (HeroBoard.IsHeroInRange(fightingHero, tauntTarget.Index, globalEffectQnt.Size))
                    {
                        int[,] boardMap = HeroBoard.GetBoardMap(f.ResolveList(board.FightingHeroesMap));

                        if (PathFinder.TryFindPath(boardMap, HeroBoard.GetHeroCords(fightingHero),
                            HeroBoard.GetHeroCords(tauntTarget), fightingHero.Hero.Range, out moveTargetPosition))
                        {
                            if (HeroBoard.IsHeroInRange(fightingHero, tauntTarget.Index, fightingHero.Hero.Range))
                            {
                                moveTargetPosition = HeroBoard.GetHeroCords(fightingHero);
                            }

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            tauntTarget = default;
            moveTargetPosition = default;
            return false;
        }

        public static void ProcessEffects(Frame f, ref FightingHero target, Board board, out bool isStunned, out bool isSilenced)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            target = heroes[target.Index];
            isStunned = false;
            isSilenced = false;

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
                        HeroAttack.DamageHeroWithoutAddMana(f, ref ownerHero, board, ref target, ref damage, null, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                        break;
                    case EffectType.IncreaseCurrentMana:
                        IncreaseCurrentMana(f, target, board, effectQnt.Value);
                        break;
                    case EffectType.Blast:
                        HeroAttack.DamageHeroByBlastWithoutAddMana(f, ref ownerHero, target.Index, board, effectQnt.Value, effectQnt.Size, includeSelf: false, null, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                        break;
                    case EffectType.Stun:
                        isStunned = true;
                        break;
                    case EffectType.Silence:
                        isSilenced = true;
                        break;
                    case EffectType.BlastSilence:
                        Effect effect = new()
                        {
                            Owner = effectQnt.Owner,
                            Type = EffectType.Silence,
                            Duration = effectQnt.Duration
                        };
                        HeroAttack.DamageHeroByBlastWithoutAddMana(f, ref ownerHero, target.Index, board, 0, effectQnt.Size, includeSelf: true, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                        break;
                    case EffectType.BlastStun:
                        BlastStun(f, ref ownerHero, board, ref target, effectQnt);
                        break;
                    case EffectType.Teleport:
                        TeleportHero(f, ref target, effectQnt, board);
                        break;
                }

                if (effectQnt.Duration <= 0)
                {
                    if (effectQnt.Index == (int)EffectType.Delayed)
                    {
                        Effect delayedEffect = new()
                        {
                            Owner = effectQnt.Owner,
                            Type = (EffectType)effectQnt.DelayedIndex,
                            MaxValue = effectQnt.MaxValue,
                            Value = effectQnt.MaxValue,
                            MaxDuration = effectQnt.MaxDuration,
                            Duration = effectQnt.DurationAfterDelay,
                            Size = effectQnt.Size,
                        };

                        FightingHero fightingHero = HeroBoard.GetFightingHero(f, effectQnt.Owner, board);

                        HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref target, delayedEffect);
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

        public static void ProcessTransferingBleedingEffect(Frame f, ref FightingHero targetHero, Board board, QList<EffectQnt> effectQnts, int transferingBleedingEffectIndex)
        {
            if (transferingBleedingEffectIndex >= 0)
            {
                EffectQnt effectQnt = effectQnts[transferingBleedingEffectIndex];

                Effect bleedEffect = new(effectQnt)
                {
                    Duration = effectQnt.MaxDuration
                };

                List<FightingHero> alies = HeroBoard.GetAllTeamHeroesInRange(f, targetHero.Index, targetHero.TeamNumber, board, bleedEffect.Size);

                if (alies.Count > 0)
                {
                    FightingHero closestTarget = HeroBoard.GetClosestTarget(f, alies, targetHero);
                    FightingHero owner = HeroBoard.GetFightingHero(f, bleedEffect.Owner, board);
                    HeroAttack.ApplyEffectToTarget(f, ref owner, board, ref closestTarget, bleedEffect);
                }
            }
        }

        private static void BlastStun(Frame f, ref FightingHero ownerHero, Board board, ref FightingHero target, EffectQnt effectQnt)
        {
            Effect effect = new()
            {
                Owner = target.Hero.Ref,
                Type = EffectType.Stun,
                Duration = effectQnt.Duration,
            };

            HeroAttack.DamageHeroByBlastWithoutAddMana(f, ref ownerHero, target.Index, board, effectQnt.Value, effectQnt.Size, includeSelf: true, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
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

                if (!f.Exists(fightingHero.Hero.Ref))
                {
                    return;
                }

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

                FightingHero targetHero = targets[i];
                FP damage = globalEffectQnt.Duration < f.DeltaTime ? globalEffectQnt.Value * globalEffectQnt.Duration : globalEffectQnt.Value * f.DeltaTime;
                HeroAttack.DamageHeroWithoutAddMana(f, ref ownerHero, board, ref targetHero, ref damage, null, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                targets[i] = targetHero;
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

        private static void IncreaseCurrentMana(Frame f, FightingHero target, Board board, FP value)
        {
            if (target.AbilityStage > 0)
            {
                return;
            }

            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            target = heroes[target.Index];
            target.CurrentMana += value;
            target.CurrentMana = FPMath.Clamp(target.CurrentMana, 0, target.Hero.MaxMana);
            heroes[target.Index] = target;
            f.Events.HeroManaChanged(board.Player1.Ref, board.Player2.Ref, target.Hero.Ref, target.CurrentMana, target.Hero.MaxMana);
        }
    }
}
