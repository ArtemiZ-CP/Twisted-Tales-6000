using System;
using Photon.Deterministic;
using Quantum.Collections;
using static Quantum.Game.HeroAttack;

namespace Quantum.Game
{
    public static unsafe class HeroAbility
    {
        public static void SelectSecondHeroAbility(Frame f, PlayerLink* playerLink, int heroID, int secondAbilityIndex)
        {
            SelectedHeroAbility selectedHeroAbility = SetSecondHeroAbility(f, playerLink, heroID, secondAbilityIndex);
            selectedHeroAbility.SecondAbilityIndex = secondAbilityIndex;
        }

        public static void SelectThirdHeroAbility(Frame f, PlayerLink* playerLink, int heroID, int thirdAbilityIndex)
        {
            SelectedHeroAbility selectedHeroAbility = SetThirdHeroAbility(f, playerLink, heroID, thirdAbilityIndex);
            selectedHeroAbility.ThirdAbilityIndex = thirdAbilityIndex;
        }

        public static int GetSecondHeroAbilityIndex(Frame f, PlayerLink* playerLink, int heroID)
        {
            SelectedHeroAbility selectedHeroAbility = GetSelectedHeroAbility(f, playerLink, heroID, out _);
            return selectedHeroAbility.SecondAbilityIndex;
        }

        public static int GetThirdHeroAbilityIndex(Frame f, PlayerLink* playerLink, int heroID)
        {
            SelectedHeroAbility selectedHeroAbility = GetSelectedHeroAbility(f, playerLink, heroID, out _);
            return selectedHeroAbility.ThirdAbilityIndex;
        }

        public static bool TryGetAbility(Frame f, FightingHero fightingHero, out Func<Frame, FightingHero, Board, bool> ability)
        {
            switch (fightingHero.Hero.ID)
            {
                case 3:
                    ability = RedHatAbilities.TryCastAbility;
                    return true;
                default:
                    ability = default;
                    return false;
            }
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, FightingHero targetHero, FP damage, HeroEffects.Effect effect, DamageType damageType, AttackType attackType)
        {
            HeroProjectilesSystem.SpawnProjectile(f, fightingHero, targetHero, damage, effect, damageType, attackType);
        }

        private static SelectedHeroAbility GetSelectedHeroAbility(Frame f, PlayerLink* playerLink, int heroID, out int index)
        {
            QList<SelectedHeroAbility> abilities = f.ResolveList(playerLink->Info.Board.Abilities);

            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i].HeroID == heroID)
                {
                    index = i;
                    return abilities[i];
                }
            }

            SelectedHeroAbility newSelectedHeroAbility = new()
            {
                HeroID = heroID,
                SecondAbilityIndex = -1,
                ThirdAbilityIndex = -1
            };

            abilities.Add(newSelectedHeroAbility);
            index = abilities.Count - 1;
            return newSelectedHeroAbility;
        }

        private static SelectedHeroAbility SetSecondHeroAbility(Frame f, PlayerLink* playerLink, int heroID, int secondAbilityIndex)
        {
            SelectedHeroAbility selectedHeroAbility = GetSelectedHeroAbility(f, playerLink, heroID, out int index);
            selectedHeroAbility.SecondAbilityIndex = secondAbilityIndex;
            QList<SelectedHeroAbility> abilities = f.ResolveList(playerLink->Info.Board.Abilities);
            abilities[index] = selectedHeroAbility;
            return selectedHeroAbility;
        }

        private static SelectedHeroAbility SetThirdHeroAbility(Frame f, PlayerLink* playerLink, int heroID, int thirdAbilityIndex)
        {
            SelectedHeroAbility selectedHeroAbility = GetSelectedHeroAbility(f, playerLink, heroID, out int index);
            selectedHeroAbility.ThirdAbilityIndex = thirdAbilityIndex;
            QList<SelectedHeroAbility> abilities = f.ResolveList(playerLink->Info.Board.Abilities);
            abilities[index] = selectedHeroAbility;
            return selectedHeroAbility;
        }
    }
}