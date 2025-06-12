using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class BabaYagaAbilities : IHeroAbility
    {
        private const int AbilityLineLength = 3;
        private const int CoinsRewardFromKill = 3;

        private static readonly FP PassiveIncreaseDamageMultiplier = FP._1_25;
        private static readonly FP AbilityDamageMultiplier = FP._1_50 + FP._0_10;
        private static readonly FP AbilityDOTDamageMultiplier = FP._0_75 + FP._0_05;
        private static readonly FP AbilityDuration = 4;
        private static readonly FP AbilityDOTDamageMultiplierLevel2 = FP._1_20;
        private static readonly FP AbilityDurationLevel2 = 6;
        private static readonly FP AbilityCooldown = 7;
        private static readonly FP AbilityCooldownLevel2 = 5;

        public override FP GetDamageMultiplier(Frame f, ref FightingHero fightingHero, Board board, ref FightingHero target, QList<FightingHero> heroes)
        {
            if (fightingHero.Hero.NameIndex != (int)HeroNameEnum.BabaYaga)
            {
                return 1;
            }

            QList<EffectQnt> effectQnt = f.ResolveList(target.Effects);

            foreach (EffectQnt effect in effectQnt)
            {
                if (HeroEffects.IsWeakening(effect))
                {
                    return PassiveIncreaseDamageMultiplier;
                }
            }

            return 1;
        }

        public override void ProcessAbilityOnKill(Frame f, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);

            if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant1)
            {
                playerLink->Info.Coins += CoinsRewardFromKill;
            }
        }

        public override void ProcessPassiveAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);

            if (selectedHeroAbility.ThirdAbilityIndex != Hero.UpgradeVariant2)
            {
                return;
            }

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
                Type = HeroEffects.EffectType.ManaRegeneration,
                Value = 5,
                Duration = FP.MaxValue,
            };

            HeroAttack.ApplyEffectToTarget(f, ref fightingHero, board, ref fightingHero, effect);
        }

        public override (bool, FP) TryCastAbility(Frame f, PlayerLink* playerLink, FightingHero fightingHero, Board board, QList<FightingHero> heroes)
        {
            fightingHero = heroes[fightingHero.Index];
            SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, *playerLink, fightingHero.Hero.ID, out int _);

            if (HeroAttack.TryFindClosestTargetInAttackRange(f, fightingHero, board, out FightingHero target, range: 1))
            {
                Vector2Int ownerPosition = HeroBoard.GetHeroCords(fightingHero);
                Vector2Int direction = HeroBoard.GetHeroCords(target) - ownerPosition;
                Vector2Int targetPosition = ownerPosition;
                FP damage = fightingHero.Hero.AttackDamage * AbilityDamageMultiplier;
                FP duration = AbilityDuration;
                FP dotDamageMultiplier = AbilityDOTDamageMultiplier;
                FP abilityCooldown = AbilityCooldown;

                if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant1)
                {
                    dotDamageMultiplier = AbilityDOTDamageMultiplierLevel2;
                    duration = AbilityDurationLevel2;
                }
                else if (selectedHeroAbility.SecondAbilityIndex == Hero.UpgradeVariant2)
                {
                    HeroAttack.HealHero(f, ref fightingHero, board, fightingHero, damage);
                }

                if (selectedHeroAbility.ThirdAbilityIndex == Hero.UpgradeVariant2)
                {
                    abilityCooldown = AbilityCooldownLevel2;
                }

                HeroEffects.Effect effect = new()
                {
                    Owner = fightingHero.Hero.Ref,
                    Type = HeroEffects.EffectType.Bleeding,
                    Value = dotDamageMultiplier * damage / duration,
                    Duration = duration,
                };

                for (int i = 0; i < AbilityLineLength; i++)
                {
                    targetPosition += direction;

                    if (HeroBoard.TryGetHeroIndexFromCords(targetPosition, out int index))
                    {
                        FightingHero targetHero = heroes[index];

                        if (targetHero.Hero.Ref != default)
                        {
                            HeroAttack.DamageHero(f, ref fightingHero, board, ref targetHero, damage, effect, HeroAttack.DamageType.Physical, HeroAttack.AttackType.Ability);
                        }
                    }
                }

                return (true, abilityCooldown);
            }

            return (false, 0);
        }
    }
}
