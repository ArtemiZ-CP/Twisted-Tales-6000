using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public static unsafe class CheshireCatAbilities
    {
        private static readonly FP TeleportDelay = FP._1_25;
        private static readonly FP AttackTimerDelayAfterTeleport = FP._1_50;

        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            int heroLevel = fightingHero.Hero.Level;
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, fightingHero.Hero.ID, out int _);

            if (heroLevel == Hero.Level1)
            {
                return TryCast(f, fightingHero, board, 150, FP._0_75, 50);
            }
            else if (heroLevel == Hero.Level2)
            {
                return TryCast(f, fightingHero, board, 225, FP._1, 75);
            }
            else if (heroLevel == Hero.Level3)
            {
                return TryCast(f, fightingHero, board, 300, FP._1_25, 100);
            }

            return false;
        }

        private static bool TryCast(Frame f, FightingHero fightingHero, Board board, FP damage, FP stunDuration, FP extraDamage)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            List<FightingHero> targets = HeroBoard.GetAllTargets(f, fightingHero, board);
            FPVector3 heroPosition = HeroBoard.GetHeroPosition(f, fightingHero);
            int heroIndex = fightingHero.Index;

            targets.Sort((a, b) =>
            {
                FP distanceA = FPVector3.DistanceSquared(heroPosition, HeroBoard.GetHeroPosition(f, a));
                FP distanceB = FPVector3.DistanceSquared(heroPosition, HeroBoard.GetHeroPosition(f, b));
                return distanceB.CompareTo(distanceA);
            });

            foreach (FightingHero targetToTeleport in targets)
            {
                if (HeroBoard.TryGetCloseEmptyTileInRange(f, targetToTeleport.Index, board, 1, out Vector2Int emptyTilePosition))
                {
                    if (Hero.TrySetNewBoardPosition(heroes, ref fightingHero, emptyTilePosition))
                    {
                        FPVector3 movePosition = HeroBoard.GetTilePosition(f, emptyTilePosition);
                        Transform3D* transform = f.Unsafe.GetPointer<Transform3D>(fightingHero.Hero.Ref);
                        FPQuaternion rotation = FPQuaternion.LookRotation(
                            HeroBoard.GetHeroPosition(f, targetToTeleport) - movePosition,
                            FPVector3.Up
                        );
                        transform->Teleport(f, movePosition, rotation);

                        HeroEffects.Effect teleportEffect = new()
                        {
                            Owner = fightingHero.Hero.Ref,
                            Type = HeroEffects.EffectType.Teleport,
                            Size = heroIndex,
                            Duration = TeleportDelay,
                        };

                        HeroEffects.Effect extraBaseDamageEffect = new()
                        {
                            Owner = fightingHero.Hero.Ref,
                            Type = HeroEffects.EffectType.ExtraBaseDamage,
                            Value = extraDamage,
                            Size = 1,
                            Duration = FP.MaxValue,
                        };

                        HeroAttack.ResetAttackTimer(f, ref fightingHero, board, AttackTimerDelayAfterTeleport);
                        HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref fightingHero, teleportEffect);
                        HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref fightingHero, extraBaseDamageEffect);

                        HeroEffects.Effect effect = new()
                        {
                            Owner = fightingHero.Hero.Ref,
                            Type = HeroEffects.EffectType.Stun,
                            Duration = stunDuration,
                        };

                        HeroAttack.DamageHero(f, fightingHero, board, targetToTeleport, damage, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}