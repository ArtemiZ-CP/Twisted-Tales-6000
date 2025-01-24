using System.Collections.Generic;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class BoardSwitcher : MonoBehaviour
    {
        [SerializeField] private QuantumEntityViewUpdater _quantumEntityViewUpdater;
        [SerializeField] private Board _board;
        [SerializeField] private Transform _cameraParent;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventStartRound>(listener: this, handler: StartRound);
            QuantumEvent.Subscribe<EventEndRound>(listener: this, handler: EndRound);
            QuantumEvent.Subscribe<EventGetProjectiles>(listener: this, handler: GetProjectiles);
        }

        private void StartRound(EventStartRound eventStartRound)
        {
            Quaternion rotation;

            if (QuantumConnection.IsPlayerMe(eventStartRound.Player1))
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (QuantumConnection.IsPlayerMe(eventStartRound.Player2))
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                return;
            }

            _cameraParent.rotation = rotation;
            _board.transform.rotation = rotation;
            
            ActiveSimulationBoard(eventStartRound.Heroes);
        }

        private void EndRound(EventEndRound eventEndRound)
        {
            _cameraParent.rotation = Quaternion.Euler(0, 0, 0);
            _board.transform.rotation = Quaternion.Euler(0, 0, 0);
            _board.SetActiveHeroes(true);
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

        private void ActiveSimulationBoard(IEnumerable<EntityLevelData> heroes)
        {
            foreach (EntityLevelData heroData in heroes)
            {
                SetActiveEntity(heroData.Ref, true);
                SetLevelMesh(heroData);
            }

            _board.SetActiveHeroes(false);
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
                quantumEntityView.gameObject.GetComponentInChildren<HeroMesh>().SetMesh(hero.Level, hero.ID);
            }
        }
    }
}