using System;
using Photon.Deterministic;
using Quantum.Collections;
using static Quantum.Game.HeroAttack;

namespace Quantum.Game
{
    public unsafe interface IHeroAbility
    {
        FP GetDamageMultiplier(Frame f, FightingHero fightingHero, Board board, FightingHero target, QList<FightingHero> heroes);
        void ProcessPassiveAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes);
        void ProcessAbilityOnDeath(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes);
        void ProcessAbilityOnKill(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes);
        (bool, FP) TryCastAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes);
        HeroStats GetHeroStats(Frame f, PlayerLink playerLink, HeroInfo heroInfo);
    }

    public static unsafe class HeroAbility
    {
        public static void ProjectileAttack(Frame f, FightingHero fightingHero, Board board, FightingHero targetHero, FP damage, HeroEffects.Effect[] effects, HeroEffects.GlobalEffect[] globalEffects, DamageType damageType, AttackType attackType)
        {
            HeroProjectilesSystem.SpawnProjectile(f, fightingHero, board, targetHero, damage, effects, globalEffects, damageType, attackType);
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

        public static SelectedHeroAbility GetSelectedHeroAbility(Frame f, PlayerLink playerLink, int heroID, out int index)
        {
            QList<SelectedHeroAbility> abilities = f.ResolveList(playerLink.Info.Board.Abilities);

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

        public static (bool, FP) TryCastAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            HeroNameEnum heroName = (HeroNameEnum)fightingHero.Hero.NameIndex;
            IHeroAbility abilityClass = GetAbilityClass(heroName);

            if (abilityClass == null)
            {
                return (false, 0);
            }

            return abilityClass.TryCastAbility(f, playerLink, fightingHero, board, heroes);
        }

        public static void ProcessPassiveAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            HeroNameEnum heroName = (HeroNameEnum)fightingHero.Hero.NameIndex;
            IHeroAbility abilityClass = GetAbilityClass(heroName);

            if (abilityClass == null)
            {
                return;
            }

            abilityClass.ProcessPassiveAbility(f, playerLink, fightingHero, board, heroes);
        }

        public static void ProcessAbilityOnDeath(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            HeroNameEnum heroName = (HeroNameEnum)fightingHero.Hero.NameIndex;
            IHeroAbility abilityClass = GetAbilityClass(heroName);

            if (abilityClass == null)
            {
                return;
            }

            abilityClass.ProcessAbilityOnDeath(f, fightingHero, board, heroes);
        }

        public static void ProcessAbilityOnKill(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            HeroNameEnum heroName = (HeroNameEnum)fightingHero.Hero.NameIndex;
            IHeroAbility abilityClass = GetAbilityClass(heroName);

            if (abilityClass == null)
            {
                return;
            }

            abilityClass.ProcessAbilityOnKill(f, fightingHero, board, heroes);
        }

        public static FP GetDamageMultiplier(Frame f, FightingHero fightingHero, Board board, FightingHero target, QList<FightingHero> heroes)
        {
            FP damageMultiplier = 1;

            foreach (HeroNameEnum name in Enum.GetValues(typeof(HeroNameEnum)))
            {
                IHeroAbility abilityClass = GetAbilityClass(name);

                if (abilityClass == null)
                {
                    continue;
                }

                damageMultiplier *= abilityClass.GetDamageMultiplier(f, fightingHero, board, target, heroes);
            }

            return damageMultiplier;
        }

        public static HeroStats GetHeroStats(Frame f, PlayerLink playerLink, HeroInfo heroInfo)
        {
            IHeroAbility abilityClass = GetAbilityClass(heroInfo.Name);

            if (abilityClass == null)
            {
                return heroInfo.Stats;
            }

            return abilityClass.GetHeroStats(f, playerLink, heroInfo);
        }

        private static IHeroAbility GetAbilityClass(HeroNameEnum heroName)
        {
            return heroName switch
            {
                // Base
                HeroNameEnum.Nutcracker => new NutcrackerAbilities(),
                // Common
                HeroNameEnum.Beast => new BeastAbilities(),
                HeroNameEnum.StoneGolem => new StoneGolemAbilities(),
                // Rare
                HeroNameEnum.TinMan => new TinManAbilities(),
                // Epic
                // Legendary
                HeroNameEnum.KingArthur => new KingArthurAbilities(),
                _ => null
            };
        }
    }
}