using System.Collections.Generic;
using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine;

namespace Quantum.Game
{
    public static unsafe class CheshireCatAbilities
    {
        private static readonly FP TeleportDelay = FP._1_25;
        private static readonly FP AttackTimerDelayAfterTeleport = FP._1_50;

        public static bool TryCastAbility(Frame f, FightingHero fightingHero, Board board)
        {
            // QList<FightingHero> heroes = f.ResolveList(board.FightingHeroesMap);
            // fightingHero = heroes[fightingHero.Index];
            // PlayerLink* playerLink = Player.GetPlayerPointer(f, fightingHero.Hero.Player);
            // int heroLevel = fightingHero.Hero.Level;
            // SelectedHeroAbility selectedHeroAbility = HeroAbility.GetSelectedHeroAbility(f, playerLink, fightingHero.Hero.ID, out int _);

            // if (heroLevel == Hero.Level1)
            // {
            //     return TryCast(f, fightingHero, board, 150, FP._0_75, 50);
            // }
            // else if (heroLevel == Hero.Level2)
            // {
            //     return TryCast(f, fightingHero, board, 225, FP._1, 75);
            // }
            // else if (heroLevel == Hero.Level3)
            // {
            //     return TryCast(f, fightingHero, board, 300, FP._1_25, 100);
            // }

            return false;
        }
    }
}