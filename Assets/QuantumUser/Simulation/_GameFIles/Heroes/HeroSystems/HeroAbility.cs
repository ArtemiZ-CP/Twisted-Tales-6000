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

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, DamageType damageType, AttackType attackType)
        {
            HeroProjectilesSystem.SpawnProjectile(f, fightingHero, board, targetHero, damage, null, null, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, HeroEffects.Effect effect, DamageType damageType, AttackType attackType)
        {
            HeroProjectilesSystem.SpawnProjectile(f, fightingHero, board, targetHero, damage, new[] { effect }, null, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, HeroEffects.GlobalEffect globalEffect, DamageType damageType, AttackType attackType)
        {
            HeroProjectilesSystem.SpawnProjectile(f, fightingHero, board, targetHero, damage, null, new[] { globalEffect }, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, HeroEffects.GlobalEffect[] globalEffects, DamageType damageType, AttackType attackType)
        {
            HeroProjectilesSystem.SpawnProjectile(f, fightingHero, board, targetHero, damage, null, globalEffects, damageType, attackType);
        }

        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, HeroEffects.Effect[] effects, HeroEffects.GlobalEffect[] globalEffects, DamageType damageType, AttackType attackType)
        {
            HeroProjectilesSystem.SpawnProjectile(f, fightingHero, board, targetHero, damage, effects, globalEffects, damageType, attackType);
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

        public static bool TryGetAbility(Frame f, FightingHero fightingHero, out Func<Frame, FightingHero, Board, bool> tryCastAbility)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            HeroNameEnum heroName = gameConfig.GetHeroInfo(f, fightingHero.Hero.ID).Name;

            switch (heroName)
            {
                case HeroNameEnum.RedHat:
                    tryCastAbility = RedHatAbilities.TryCastAbility;
                    return true;
                case HeroNameEnum.WhiteRabbit:
                    tryCastAbility = WhiteRabbitAbilities.TryCastAbility;
                    return true;
                case HeroNameEnum.SlyFox:
                    tryCastAbility = SlyFoxAbilities.TryCastAbility;
                    return true;
                case HeroNameEnum.SnowWhite:
                    tryCastAbility = SnowWhiteAbilities.TryCastAbility;
                    return true;

                case HeroNameEnum.RobinHood:
                    tryCastAbility = RobinHoodAbilities.TryCastAbility;
                    return true;
                case HeroNameEnum.Beast:
                    tryCastAbility = BeastAbilities.TryCastAbility;
                    return true;
                case HeroNameEnum.Cinderella:
                    tryCastAbility = CinderellaAbilities.TryCastAbility;
                    return true;
                case HeroNameEnum.Hatter:
                    tryCastAbility = HatterAbilities.TryCastAbility;
                    return true;

                case HeroNameEnum.Alice:
                    tryCastAbility = AliceAbilities.TryCastAbility;
                    return true;
                case HeroNameEnum.Scarecrow:
                    tryCastAbility = ScarecrowAbilities.TryCastAbility;
                    return true;
                case HeroNameEnum.Aladdin:
                    tryCastAbility = AladdinAbilities.TryCastAbility;
                    return true;
                case HeroNameEnum.PussInBoots:
                    tryCastAbility = PussInBootsAbilities.TryCastAbility;
                    return true;
                case HeroNameEnum.Nutcracker:
                    tryCastAbility = NutcrackerAbilities.TryCastAbility;
                    return true;

                case HeroNameEnum.BabaYaga:
                    tryCastAbility = BabaYagaAbilities.TryCastAbility;
                    return true;
                case HeroNameEnum.CheshireCat:
                    tryCastAbility = CheshireCatAbilities.TryCastAbility;
                    return true;

                default:
                    tryCastAbility = default;
                    return false;
            }
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