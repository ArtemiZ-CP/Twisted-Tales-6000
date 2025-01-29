using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine.Scripting;
using System.Collections.Generic;
using System;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class BaseHeroFightingSystem : SystemMainThreadGroup
    {
        public BaseHeroFightingSystem() : base(nameof(BaseHeroFightingSystem))
        {
        }

        public BaseHeroFightingSystem(string name, params SystemMainThread[] children) : base(name, children)
        {
        }

        public static void UpdateHeroes<T>(Frame f,
            Action<Frame, FightingHero, HeroAttack.DamageType> Attack, bool isHeroAttackWhileMooving) where T : unmanaged, IComponent
        {
            if (f.Global->IsBuyPhase || f.Global->IsDelayPassed == false || f.Global->IsFighting == false) return;

            List<FightingHero> heroesPtr = new();

            if (TryGetHeroes<T>(f, ref heroesPtr))
            {
                foreach (FightingHero fightingHero in heroesPtr)
                {
                    UpdateHero(f, fightingHero, Attack, isHeroAttackWhileMooving);
                }
            }
        }

        private static void UpdateHero(Frame f, FightingHero fightingHero,
            Action<Frame, FightingHero, HeroAttack.DamageType> Attack, bool isHeroAttackWhileMooving)
        {
            Board board = HeroBoard.GetBoard(f, fightingHero);
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];

            if (fightingHero.Hero.Ref == default || fightingHero.IsAlive == false)
            {
                return;
            }

            HeroAttack.Update(f, fightingHero);

            if (HeroBoard.IsHeroMoving(f, fightingHero))
            {
                MoveHero(f, fightingHero, HeroBoard.GetHeroPosition(f, fightingHero));

                if (isHeroAttackWhileMooving == false) return;
            }

            if (HeroBoard.TrySetTarget(f, fightingHero))
            {
                if (fightingHero.AttackTarget != default && f.Exists(fightingHero.AttackTarget))
                {
                    Hero.Rotate(f, fightingHero.Hero, f.Get<Transform3D>(fightingHero.AttackTarget).Position);
                }

                Attack(f, fightingHero, (HeroAttack.DamageType)fightingHero.Hero.AttackDamageType);
                return;
            }
        }

        private static bool TryGetHeroes<T>(Frame f, ref List<FightingHero> heroes) where T : unmanaged, IComponent
        {
            List<Board> boards = BoardSystem.GetBoards(f);

            foreach (Board board in boards)
            {
                QList<FightingHero> fightingHeroes = f.ResolveList(board.FightingHeroesMap);

                foreach (FightingHero fightingHero in fightingHeroes)
                {
                    if (fightingHero.Hero.Ref == default || fightingHero.IsAlive == false)
                    {
                        continue;
                    }

                    if (f.Unsafe.TryGetPointer(fightingHero.Hero.Ref, out T* _))
                    {
                        heroes.Add(fightingHero);
                    }
                }
            }

            if (heroes.Count > 0)
            {
                return true;
            }

            return false;
        }

        private static void MoveHero(Frame f, FightingHero fightingHero, FPVector3 movePosition)
        {
            GameConfig gameConfig = f.FindAsset(f.RuntimeConfig.GameConfig);
            Transform3D* transform = f.Unsafe.GetPointer<Transform3D>(fightingHero.Hero.Ref);
            FP moveOffset = gameConfig.HeroMoveSpeed * f.DeltaTime;

            Hero.Rotate(f, fightingHero.Hero, movePosition);

            transform->Position = FPVector3.MoveTowards(transform->Position, movePosition, moveOffset);
        }
    }
}