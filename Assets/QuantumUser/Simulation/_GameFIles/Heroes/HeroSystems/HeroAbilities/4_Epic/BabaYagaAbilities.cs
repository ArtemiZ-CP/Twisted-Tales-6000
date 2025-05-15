using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class BabaYagaAbilities
    {
        private const int BleedingDuration = 3;
        private const int BleedingTransferSize = 2;

        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            int heroLevel = fightingHero.Hero.Level;
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, fightingHero.Hero.ID, out int _);

            if (heroLevel == Hero.Level1)
            {
                return TryCast(f, fightingHero, board, 80);
            }
            else if (heroLevel == Hero.Level2)
            {
                return TryCast(f, fightingHero, board, 120);
            }
            else if (heroLevel == Hero.Level3)
            {
                return TryCast(f, fightingHero, board, 180);
            }

            return false;
        }

        private static bool TryCast(Frame f, FightingHero fightingHero, Board board, FP bleedingDamage)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                HeroEffects.Effect effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.TransferingBleeding,
                    Value = bleedingDamage,
                    MaxDuration = BleedingDuration,
                    Duration = BleedingDuration,
                    Size = BleedingTransferSize,
                };

                HeroAbility.ProjectileAttack(f, fightingHero, board, target, 0, effect, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                return true;
            }

            return false;
        }
    }
}