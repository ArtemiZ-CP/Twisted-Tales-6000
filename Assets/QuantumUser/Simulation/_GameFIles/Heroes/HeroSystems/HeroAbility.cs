using System;
using Photon.Deterministic;
using Quantum.Collections;
using static Quantum.Game.HeroAttack;

namespace Quantum.Game
{
    public interface IHeroAbility
    {
        void ProcessPassiveAbility(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes);
        void ProcessAbilityOnDeath(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes);
        (bool, FP) TryCastAbility(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes);
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

        public static bool TryGetAbility(Frame f, FightingHero fightingHero,
            out Func<Frame, FightingHero, Board, QList<FightingHero>, (bool, FP)> TryCastAbility,
            out Action<Frame, FightingHero, Board, QList<FightingHero>> ProcessPassiveAbility)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            HeroNameEnum heroName = gameConfig.GetHeroInfo(f, fightingHero.Hero.ID).Name;

            IHeroAbility abilityClass = GetAbilityClass(heroName);

            if (abilityClass == null)
            {
                TryCastAbility = null;
                ProcessPassiveAbility = null;
                return false;
            }

            TryCastAbility = abilityClass.TryCastAbility;
            ProcessPassiveAbility = abilityClass.ProcessPassiveAbility;
            return true;
        }

        public static void ProcessAbilityOnDeath(Frame f, ref FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            HeroNameEnum heroName = gameConfig.GetHeroInfo(f, fightingHero.Hero.ID).Name;
            IHeroAbility abilityClass = GetAbilityClass(heroName);

            if (abilityClass == null)
            {
                return;
            }

            abilityClass.ProcessAbilityOnDeath(f, fightingHero, board, heroes);
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
                // Rare
                HeroNameEnum.TinMan => new TinManAbilities(),
                _ => null
            };
        }
    }
}