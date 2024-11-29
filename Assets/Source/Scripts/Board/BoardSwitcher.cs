using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class BoardSwitcher : MonoBehaviour
    {
        [SerializeField] private QuantumEntityViewUpdater _quantumEntityViewUpdater;
        [SerializeField] private Board _board;
        [SerializeField] private Transform _cameraParent;

        private EntityRef _playerBoardRef;
        private List<EntityLevelData> _heroes = new();

        private void Awake()
        {
            QuantumEvent.Subscribe<EventStartRound>(listener: this, handler: StartRound);
            QuantumEvent.Subscribe<EventEndRound>(listener: this, handler: EndRound);
            QuantumEvent.Subscribe<EventDestroyHero>(listener: this, handler: DestroyHero);
            QuantumEvent.Subscribe<EventGetProjectiles>(listener: this, handler: GetProjectiles);
        }

        private void StartRound(EventStartRound eventStartRound)
        {
            if (QuantumConnection.IsPlayerMe(eventStartRound.Player1))
            {
                _cameraParent.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (QuantumConnection.IsPlayerMe(eventStartRound.Player2))
            {
                _cameraParent.rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                return;
            }

            _heroes = eventStartRound.Heroes;
            _playerBoardRef = eventStartRound.BoardEntity;

            SetActiveSimulationBoard(true);
        }

        private void EndRound(EventEndRound eventEndRound)
        {
            _cameraParent.rotation = Quaternion.Euler(0, 0, 0);
            _board.gameObject.SetActive(true);
        }

        private void DestroyHero(EventDestroyHero eventDestroyHero)
        {
            if (QuantumConnection.IsPlayerMe(eventDestroyHero.PlayerRef1) ||
                QuantumConnection.IsPlayerMe(eventDestroyHero.PlayerRef2))
            {
                EntityRef heroRef = eventDestroyHero.HeroEntity;

                if (_heroes.Any(hero => hero.Ref == heroRef))
                {
                    EntityLevelData hero = _heroes.Find(hero => hero.Ref == heroRef);
                    _heroes.Remove(hero);
                    SetActiveEntity(hero.Ref, false);
                }
            }
        }

        private void GetProjectiles(EventGetProjectiles eventGetProjectiles)
        {
            if (QuantumConnection.IsPlayerMe(eventGetProjectiles.Player1) ||
                QuantumConnection.IsPlayerMe(eventGetProjectiles.Player2))
            {
                foreach (EntityLevelData projectileData in eventGetProjectiles.ProjectileList)
                {
                    SetActiveEntity(projectileData.Ref, true);
                    SetLevelMesh(projectileData);
                }
            }
        }

        private void SetActiveSimulationBoard(bool isActive)
        {
            foreach (EntityLevelData heroData in _heroes)
            {
                SetActiveEntity(heroData.Ref, isActive);
                SetLevelMesh(heroData);
            }

            SetActiveEntity(_playerBoardRef, isActive);
            _board.gameObject.SetActive(isActive == false);
        }

        private void SetActiveEntity(EntityRef entityRef, bool isActive)
        {
            if (entityRef != default)
            {
                QuantumEntityView quantumEntityView = _quantumEntityViewUpdater.GetView(entityRef);

                if (quantumEntityView != null)
                {
                    quantumEntityView.gameObject.SetActive(isActive);
                }
            }
        }

        private void SetLevelMesh(EntityLevelData hero)
        {
            QuantumEntityView quantumEntityView = _quantumEntityViewUpdater.GetView(hero.Ref);

            if (quantumEntityView != null)
            {
                quantumEntityView.gameObject.GetComponentInChildren<HeroMesh>().SetMesh(hero.Level);
            }
        }
    }
}