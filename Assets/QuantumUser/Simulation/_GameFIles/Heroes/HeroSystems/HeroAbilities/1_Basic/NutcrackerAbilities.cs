using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class NutcrackerAbilities : IHeroAbility
    {
        private readonly static FP ReduceManaPercentage = FP._0_25;
        private readonly static FP PassiveReduceMagicDefense = 20;
        private readonly static FP AbilityDamageMultiplier = FP._1_50;
        private readonly static FP AbilityReloadTime = 7;
        private readonly static FP MinManaPercentageForStun = FP._0_50;
        private readonly static FP SplitDamagePercentage = FP._0_50;
        private const int HealthMultiplier = 2;
        private const int StunDuration = 1;

        public HeroStats GetHeroStats(Frame f, PlayerLink playerLink, HeroInfo heroInfo)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, gameConfig.GetHeroID(f, heroInfo.Name), out int _);
            HeroStats heroStats = heroInfo.Stats;

            if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant2)
            {
                HeroLevelStats[] heroLevelStats = new HeroLevelStats[heroStats.LevelStats.Length];

                for (int i = 0; i < heroStats.LevelStats.Length; i++)
                {
                    heroLevelStats[i] = heroStats.LevelStats[i];
                    heroLevelStats[i].Health *= HealthMultiplier;
                }

                heroStats.LevelStats = heroLevelStats;
                return heroStats;
            }

            return heroStats;
        }

        public void ProcessAbilityOnDeath(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);

            if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant1)
            {
                TryCastAbility(f, fightingHero, board, heroes);
            }
        }

        public void ProcessPassiveAbility(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            if (fightingHero.IsPassiveAbilityActivated)
            {
                return;
            }

            fightingHero = heroes[fightingHero.Index];
            fightingHero.IsPassiveAbilityActivated = true;
            heroes[fightingHero.Index] = fightingHero;
            HeroEffects.Effect effect = new()
            {
                Owner = fightingHero.Hero.Ref,
                Type = HeroEffects.EffectType.ReduceMagicDefense,
                Value = PassiveReduceMagicDefense,
                Duration = FP.MaxValue,
            };
            HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref fightingHero, effect);
        }

        public (bool, FP) TryCastAbility(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            fightingHero = heroes[fightingHero.Index];
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);
            bool isAbilityWithStun = false;
            bool isAttackSplit = false;

            if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant1)
            {
                isAbilityWithStun = true;
            }
            else if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant2)
            {
                isAttackSplit = true;
            }

            if (TryCastAbility(f, ref fightingHero, board, heroes, isAbilityWithStun, isAttackSplit))
            {
                return (true, AbilityReloadTime);
            }

            return (false, 0);
        }

        private bool TryCastAbility(Frame f, ref FightingHero fightingHero, Board board, QList<FightingHero> heroes, bool isAbilityWithStun, bool isAttackSplit)
        {
            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target))
            {
                FP reducedMana = ReduceManaPercentage * target.Hero.MaxMana;

                HeroEffects.Effect manaEffect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.IncreaseCurrentMana,
                    Value = -reducedMana,
                };

                HeroEffects.Effect[] effects = new HeroEffects.Effect[1]
                {
                    manaEffect
                };

                if (isAbilityWithStun)
                {
                    if (fightingHero.CurrentMana - reducedMana <= fightingHero.Hero.MaxMana * MinManaPercentageForStun)
                    {
                        HeroEffects.Effect stunEffect = new()
                        {
                            Owner = fightingHero.Hero.Ref,
                            Type = HeroEffects.EffectType.Stun,
                            Duration = StunDuration,
                        };

                        effects = new HeroEffects.Effect[2]
                        {
                            manaEffect,
                            stunEffect
                        };
                    }
                }

                FP damage = fightingHero.Hero.AttackDamage * AbilityDamageMultiplier;
                HeroAttack.DamageHero(f, ref fightingHero, board, ref target, damage, effects, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);

                if (isAttackSplit)
                {
                    damage *= SplitDamagePercentage;

                    if (HeroBoard.TryGetHeroCordsFromIndex(target.Index, out Vector2Int cords))
                    {
                        if (HeroBoard.TryGetHeroIndexFromCords(cords + Vector2Int.left, out int leftIndex))
                        {
                            FightingHero leftHero = heroes[leftIndex];

                            if (leftHero.Hero.Ref != default)
                            {
                                HeroAttack.DamageHero(f, ref fightingHero, board, ref leftHero, damage, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                            }
                        }

                        if (HeroBoard.TryGetHeroIndexFromCords(cords + Vector2Int.right, out int rightIndex))
                        {
                            FightingHero rightHero = heroes[rightIndex];

                            if (rightHero.Hero.Ref != default)
                            {
                                HeroAttack.DamageHero(f, ref fightingHero, board, ref rightHero, damage, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                            }
                        }

                    }
                }

                return true;
            }

            return false;
        }
    }
}
