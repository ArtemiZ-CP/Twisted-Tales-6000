using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class CerberusAbilities
    {
        public static readonly FP FirstAbilityPercent = FP._0_50;
        public static readonly FP SecondAbilityPercent = FP._0_20 + FP._0_10;

        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            // QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            // fightingHero = heroes[fightingHero.Index];
            // PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            // int heroLevel = fightingHero.Hero.Level;
            // SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, fightingHero.Hero.ID, out int _);
            // FP hpPercent = fightingHero.CurrentHealth / fightingHero.Hero.Health;

            // if (hpPercent > FirstAbilityPercent)
            // {
            //     FP damage = heroLevel switch
            //     {
            //         Hero.Level1 => 300,
            //         Hero.Level2 => 450,
            //         Hero.Level3 => 675,
            //         _ => 0
            //     };

            //     FP armor = heroLevel switch
            //     {
            //         Hero.Level1 => 250,
            //         Hero.Level2 => 375,
            //         Hero.Level3 => 525,
            //         _ => 0
            //     };

            //     return TryCastV1(f, fightingHero, board, damage, armor);
            // }
            // else if (hpPercent > SecondAbilityPercent)
            // {
            //     FP damage = heroLevel switch
            //     {
            //         Hero.Level1 => 400,
            //         Hero.Level2 => 600,
            //         Hero.Level3 => 900,
            //         _ => 0
            //     };

            //     FP reduceAttackSpeed = heroLevel switch
            //     {
            //         Hero.Level1 => FP._0_10 + FP._0_05,
            //         Hero.Level2 => FP._0_10 * 2,
            //         Hero.Level3 => FP._0_25,
            //         _ => 0
            //     };

            //     return TryCastV2(f, fightingHero, board, damage, reduceAttackSpeed);
            // }
            // else
            // {
            //     FP damage = heroLevel switch
            //     {
            //         Hero.Level1 => 600,
            //         Hero.Level2 => 900,
            //         Hero.Level3 => 1350,
            //         _ => 0
            //     };

            //     FP healPercent = heroLevel switch
            //     {
            //         Hero.Level1 => FP._0_10 * 2,
            //         Hero.Level2 => FP._0_20 + FP._0_10,
            //         Hero.Level3 => FP._0_50,
            //         _ => 0
            //     };

            //     FP increaseAttackSpeed = heroLevel switch
            //     {
            //         Hero.Level1 => FP._0_25,
            //         Hero.Level2 => FP._0_25 + FP._0_10,
            //         Hero.Level3 => FP._0_50,
            //         _ => 0
            //     };

            //     return TryCastV3(f, fightingHero, board, damage, healPercent, increaseAttackSpeed);
            // }

            return false;
        }

        private static bool TryCastV1(Frame f, FightingHero fightingHero, Board board, FP damage, FP armor)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroEffects.Effect effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.TemporaryArmor,
                    Value = armor,
                    Duration = 3,
                };

                HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref fightingHero, effect);
                HeroAttack.DamageHeroByBlast(f, fightingHero, target.Index, board, damage, fightingHero.Hero.Range, includeSelf: true, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }
    }
}