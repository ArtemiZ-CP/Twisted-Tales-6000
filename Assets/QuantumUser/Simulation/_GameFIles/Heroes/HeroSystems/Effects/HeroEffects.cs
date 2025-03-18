using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum.Game
{
    public static unsafe class HeroEffects
    {
        public enum EffectType
        {
            None,
            Bleeding,
            Curse,
            IncteaseTakingDamage,
        }

        public class Effect
        {
            public int OwnerIndex;
            public EffectType Type;
            public FP Value;
            public FP Duration;

            public Effect()
            {
                Type = EffectType.None;
            }

            public Effect(EffectQnt effectQnt)
            {
                OwnerIndex = effectQnt.OwnerIndex;
                Type = (EffectType)effectQnt.EffectIndex;
                Value = effectQnt.EffectValue;
                Duration = effectQnt.EffectDuration;
            }
        }

        public static void ProcessEffects(Frame f, FightingHero target, Board board)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            target = heroes[target.Index];

            if (f.Exists(target.Hero.Ref) == false || target.IsAlive == false)
            {
                return;
            }

            QList<EffectQnt> effects = f.ResolveList(target.Effects);

            for (int i = 0; i < effects.Count; i++)
            {
                EffectQnt effectQnt = effects[i];
                FightingHero ownerHero = heroes[effectQnt.OwnerIndex];

                FP damage = effectQnt.EffectDuration < f.DeltaTime ? effectQnt.EffectValue * effectQnt.EffectDuration : effectQnt.EffectValue * f.DeltaTime;

                switch ((EffectType)effectQnt.EffectIndex)
                {
                    case EffectType.Bleeding:
                        HeroAttack.DamageHeroByEffect(f, ownerHero, board, target, damage, HeroAttack.DamageType.Magical, HeroAttack.AttackType.Ability);
                        break;
                }

                effectQnt.EffectDuration -= f.DeltaTime;

                if (effectQnt.EffectDuration <= 0)
                {
                    effects.RemoveAt(i);
                    i--;
                }
                else
                {
                    effects[i] = effectQnt;
                }
            }
        }
    }
}
