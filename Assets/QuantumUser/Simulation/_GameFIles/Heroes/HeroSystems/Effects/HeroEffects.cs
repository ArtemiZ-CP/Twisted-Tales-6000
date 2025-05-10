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
            IncreaseHealAndArmor,
            IncreaseAttackSpeed,
            ReduceDefense,
            ReduceMagicDefense,
            HorizontalBlast,
            Blast,
            Stun,
            BlastStun,
            TemporaryArmor,
            Teleport,
            ExtraBaseDamage,
            FirebirdRebirth,
            Silence,
            BlastSilence,
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
                        HeroAttack.DamageHeroWithoutAddMana(f, ref ownerHero, board, ref target, damage, null, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                        break;
                    case EffectType.ReduceCurrentMana:
                        ReduceCurrentMana(f, target, board, effectQnt.Value);
                        break;
                    case EffectType.Blast:
                        HeroAttack.DamageHeroByBlastWithoutAddMana(f, ownerHero, target.Index, board, effectQnt.Value, effectQnt.Size, includeSelf: false, null, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
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
                        HeroAttack.DamageHeroByBlastWithoutAddMana(f, ownerHero, target.Index, board, 0, effectQnt.Size, includeSelf: true, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                        break;
                    case EffectType.BlastStun:
                        BlastStun(f, ref ownerHero, board, ref target, effectQnt);
                        break;
                    case EffectType.Teleport:
                        TeleportHero(f, ref target, effectQnt, board);
                        break;
                    case EffectType.FirebirdRebirth:
                        effectQnt.Size = ProcessFirebirdRebirth(f, ref target, board, damage);
                        isStunned = true;
                        break;
                }

                if (effectQnt.Duration <= 0)
                {
                    if (effectQnt.Index == (int)EffectType.FirebirdRebirth)
                    {
                        if (effectQnt.Size > 0)
                        {
                            target.CurrentHealth = target.Hero.Health / 2;

                            Effect effect = new()
                            {
                                Owner = target.Hero.Ref,
                                Type = EffectType.IncreaseAttackSpeed,
                                Value = effectQnt.MaxValue,
                                Duration = 4,
                                Size = 0
                            };

                            heroes[target.Index] = target;

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

        private static void BlastStun(Frame f, ref FightingHero ownerHero, Board board, ref FightingHero target, EffectQnt effectQnt)
        {
            Effect effect = new()
            {
                Owner = target.Hero.Ref,
                Type = EffectType.Stun,
                Duration = effectQnt.Duration,
            };

            HeroAttack.DamageHeroByBlastWithoutAddMana(f, ownerHero, target.Index, board, effectQnt.Value, effectQnt.Size, includeSelf: true, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
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

                FightingHero targetHero = targets[i];
                FP damage = globalEffectQnt.Duration < f.DeltaTime ? globalEffectQnt.Value * globalEffectQnt.Duration : globalEffectQnt.Value * f.DeltaTime;
                HeroAttack.DamageHeroWithoutAddMana(f, ref ownerHero, board, ref targetHero, damage, null, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
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

        private static int ProcessFirebirdRebirth(Frame f, ref FightingHero owner, Board board, FP value)
        {
            var heroesInRange = HeroBoard.GetAllTargetsInRange(f, owner, board);

            for (int i = 0; i < heroesInRange.Count; i++)
            {
                FightingHero target = heroesInRange[i];
                HeroAttack.DamageHeroWithoutAddMana(f, ref owner, board, ref target, value, null, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
            }

            QList<EffectQnt> effects = f.ResolveList(owner.Effects);

            foreach (EffectQnt effect in effects)
            {
                if (effect.Index == (int)EffectType.FirebirdRebirth)
                {
                    return effect.Size;
                }
            }

            return 0;
        }
    }
}
