using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine.Scripting;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace Quantum.Game
{
    [Preserve]
    public unsafe class BaseHeroFightingSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            if (f.Global->IsBuyPhase || f.Global->IsDelayPassed == false || f.Global->IsFighting == false)
            {
                return;
            }

            QList<Board> boards = BoardSystem.GetBoards(f);

            UpdateHeroes(f, boards);

            foreach (Board board in boards)
            {
                HeroEffects.ProcessGlobalEffects(f, board);
            }

            foreach ((EntityRef _, PlayerLink playerLink) in f.GetComponentIterator<PlayerLink>())
            {
                if (playerLink.Info.SpectatingHero == default)
                {
                    f.Events.GetFightingHero(playerLink.Ref, new FightingHero());
                    continue;
                }

                Board board = BoardSystem.GetBoard(f, playerLink.Ref, boards);
                QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
                bool isHeroFound = false;

                foreach (FightingHero hero in heroes)
                {
                    if (hero.Hero.Ref == playerLink.Info.SpectatingHero)
                    {
                        isHeroFound = true;
                        f.Events.GetFightingHero(playerLink.Ref, hero);
                        break;
                    }
                }

                if (isHeroFound == false)
                {
                    f.Events.GetFightingHero(playerLink.Ref, new FightingHero());
                }
            }
        }

        private static void UpdateHeroes(Frame f, QList<Board> boards)
        {
            foreach (Board board in boards)
            {
                QList<FightingHero> fightingHeroes = f.ResolveList(board.FightingHeroesMap);

                foreach (FightingHero fightingHero in fightingHeroes)
                {
                    if (fightingHero.Hero.Ref == default || fightingHero.IsAlive == false)
                    {
                        continue;
                    }

                    if (f.Unsafe.TryGetPointer(fightingHero.Hero.Ref, out MeleeHero* _))
                    {
                        UpdateHero(f, fightingHero, board, HeroAttack.InstantAttack, false);
                    }
                    else if (f.Unsafe.TryGetPointer(fightingHero.Hero.Ref, out RangedHero* _))
                    {
                        UpdateHero(f, fightingHero, board, HeroAttack.ProjectileAttack, false);
                    }
                }
            }
        }

        private static void UpdateHero(Frame f, FightingHero fightingHero, Board board,
            Func<Frame, FightingHero, HeroAttack.DamageType, HeroAttack.AttackType, bool> Attack, bool isHeroAttackWhileMooving)
        {
            QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            fightingHero = heroes[fightingHero.Index];
            
            if (fightingHero.Hero.Ref == default || fightingHero.IsAlive == false || f.Exists(fightingHero.Hero.Ref) == false)
            {
                return;
            }

            HeroAttack.Update(f, ref fightingHero, board, out bool isStunned);

            if (isStunned)
            {
                return;
            }

            if (f.Exists(fightingHero.Hero.Ref) == false)
            {
                return;
            }

            if (HeroBoard.IsHeroMoving(f, fightingHero))
            {
                MoveHero(f, fightingHero, HeroBoard.GetHeroPosition(f, fightingHero));

                if (isHeroAttackWhileMooving == false)
                {
                    return;
                }
            }

            if (HeroBoard.TrySetTarget(f, ref fightingHero, board) == false)
            {
                return;
            }

            if (fightingHero.AttackTarget != default && f.Exists(fightingHero.AttackTarget))
            {
                Hero.Rotate(f, fightingHero.Hero, f.Get<Transform3D>(fightingHero.AttackTarget).Position);
            }

            if (Attack(f, fightingHero, (HeroAttack.DamageType)fightingHero.Hero.AttackDamageType, HeroAttack.AttackType.Base))
            {
                fightingHero = heroes[fightingHero.Index];
                fightingHero.AttackStage++;
                heroes[fightingHero.Index] = fightingHero;
            }
        }

        private static bool TryGetHeroes<T>(Frame f, QList<Board> boards, ref List<(FightingHero, Board)> heroes) where T : unmanaged, IComponent
        {
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
                        heroes.Add((fightingHero, board));
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