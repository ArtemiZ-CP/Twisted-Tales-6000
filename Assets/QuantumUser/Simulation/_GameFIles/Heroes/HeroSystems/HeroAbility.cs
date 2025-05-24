using System;
using Photon.Deterministic;
using Quantum.Collections;
using static Quantum.Game.HeroAttack;

namespace Quantum.Game
{
    public static unsafe class HeroAbility
    {
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

        public static SelectedHeroAbility GetSelectedHeroAbility(Frame f, PlayerLink* playerLink, int heroID, out int index)
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
                SecondAbilityIndex = Hero.UpgradeClosed,
                ThirdAbilityIndex = Hero.UpgradeClosed
            };

            abilities.Add(newSelectedHeroAbility);
            index = abilities.Count - 1;
            return newSelectedHeroAbility;
        }

        public static bool TryGetAbility(Frame f, FightingHero fightingHero, out FP reloadTime,
            out Func<Frame, FightingHero, Board, QList<FightingHero>, bool> TryCastAbility,
            out Action<Frame, FightingHero, Board, QList<FightingHero>> ProcessPassiveAbility)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            HeroNameEnum heroName = gameConfig.GetHeroInfo(f, fightingHero.Hero.ID).Name;

            switch (heroName)
            {
                // case HeroNameEnum.RedHat:
                //     TryCastAbility = RedHatAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.WhiteRabbit:
                //     TryCastAbility = WhiteRabbitAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.SlyFox:
                //     TryCastAbility = SlyFoxAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.SnowWhite:
                //     TryCastAbility = SnowWhiteAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;

                // case HeroNameEnum.RobinHood:
                //     TryCastAbility = RobinHoodAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.Beast:
                //     TryCastAbility = BeastAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.Cinderella:
                //     TryCastAbility = CinderellaAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.Hatter:
                //     TryCastAbility = HatterAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;

                // case HeroNameEnum.Alice:
                //     TryCastAbility = AliceAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.Scarecrow:
                //     TryCastAbility = ScarecrowAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.Aladdin:
                //     TryCastAbility = AladdinAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.PussInBoots:
                //     TryCastAbility = PussInBootsAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.Nutcracker:
                //     TryCastAbility = NutcrackerAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;

                // case HeroNameEnum.BabaYaga:
                //     TryCastAbility = BabaYagaAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.CheshireCat:
                //     TryCastAbility = CheshireCatAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                case HeroNameEnum.TinMan:
                    TryCastAbility = TinManAbilities.TryCastAbility;
                    ProcessPassiveAbility = TinManAbilities.ProcessPassiveAbility;
                    reloadTime = TinManAbilities.AbilityReloadTime;
                    return true;
                // case HeroNameEnum.KingArthur:
                //     TryCastAbility = KingArthurAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;

                // case HeroNameEnum.Firebird:
                //     TryCastAbility = FirebirdAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.Cerberus:
                //     TryCastAbility = CerberusAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;
                // case HeroNameEnum.Merlin:
                //     TryCastAbility = MerlinAbilities.TryCastAbility;
                //     ProcessPassiveAbility = default;
                //     return true;

                default:
                    TryCastAbility = default;
                    ProcessPassiveAbility = default;
                    reloadTime = default;
                    return false;
            }
        }
    }
}