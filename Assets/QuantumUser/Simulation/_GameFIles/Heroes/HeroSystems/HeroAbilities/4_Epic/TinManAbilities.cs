using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class TinManAbilities
    {
        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            int heroLevel = fightingHero.Hero.Level;
            int secondHeroAbilityIndex = HeroAbility.GetSecondHeroAbilityIndex(f, playerLink, fightingHero.Hero.ID);
            int thirdHeroAbilityIndex = HeroAbility.GetThirdHeroAbilityIndex(f, playerLink, fightingHero.Hero.ID);
            int abilityStage = fightingHero.AbilityStage;

            FP damage1;
            FP damage2;
            FP defenseReduse2;
            FP damage3;
            FP armor3;
            FP reloadTime = 1 / fightingHero.Hero.AttackSpeed;

            if (heroLevel == 0)
            {
                damage1 = 60;
                damage2 = 140;
                defenseReduse2 = 10;
                damage3 = 100;
                armor3 = 150;
            }
            else if (heroLevel == 1)
            {
                damage1 = 90;
                damage2 = 210;
                defenseReduse2 = 15;
                damage3 = 150;
                armor3 = 225;
            }
            else if (heroLevel == 2)
            {
                damage1 = 135;
                damage2 = 315;
                defenseReduse2 = 20;
                damage3 = 225;
                armor3 = 340;
            }
            else
            {
                return false;
            }

            if (abilityStage == 0 && TryCastV1(f, fightingHero, board, damage1))
            {
                fightingHero = heroes[fightingHero.Index];
                fightingHero.AbilityStage = 1;
                heroes[fightingHero.Index] = fightingHero;
                HeroAttack.ResetAttackTimer(f, ref fightingHero, board, reloadTime * 2);
                return false;
            }
            else if (abilityStage == 1 && TryCastV2(f, fightingHero, board, damage2, defenseReduse2))
            {
                fightingHero = heroes[fightingHero.Index];
                fightingHero.AbilityStage = 2;
                heroes[fightingHero.Index] = fightingHero;
                HeroAttack.ResetAttackTimer(f, ref fightingHero, board, reloadTime / 2);
                return false;
            }
            else if (abilityStage == 2 && TryCastV3(f, fightingHero, board, damage3, armor3))
            {
                fightingHero = heroes[fightingHero.Index];
                fightingHero.AbilityStage = 0;
                heroes[fightingHero.Index] = fightingHero;
                HeroAttack.ResetAttackTimer(f, ref fightingHero, board);
                return true;
            }

            return false;
        }

        private static bool TryCastV1(Frame f, FightingHero fightingHero, Board board, FP damage)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroAttack.DamageHero(f, fightingHero, board, target, damage, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }

        private static bool TryCastV2(Frame f, FightingHero fightingHero, Board board, FP damage, FP defenseReduse)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroEffects.Effect effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.ReduceDefense,
                    Value = defenseReduse,
                    Duration = 3,
                };

                HeroAttack.DamageHero(f, fightingHero, board, target, damage, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }

        private static bool TryCastV3(Frame f, FightingHero fightingHero, Board board, FP damage, FP armor)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroAttack.AddArmorToHero(f, fightingHero, board, fightingHero, armor);
                HeroAttack.DamageHero(f, fightingHero, board, target, damage, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }
    }
}