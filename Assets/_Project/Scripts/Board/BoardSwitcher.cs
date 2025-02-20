using System.Collections.Generic;
using UnityEngine;

namespace Quantum.Game
{
    public unsafe class BoardSwitcher : MonoBehaviour
    {
        [SerializeField] private QuantumEntityViewUpdater _quantumEntityViewUpdater;
        [SerializeField] private Board _board;
        [SerializeField] private GameObject _board0;
        [SerializeField] private GameObject _board180;
        [SerializeField] private Transform _cameraParent;
        [SerializeField] private Material _skyboxMaterial;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventStartRound>(listener: this, handler: StartRound);
            QuantumEvent.Subscribe<EventEndRound>(listener: this, handler: EndRound);
            QuantumEvent.Subscribe<EventSetActiveEntity>(listener: this, handler: SetActiveEntity);
        }

        private void Start()
        {
            _skyboxMaterial.SetFloat("_Rotation", 0);
        }

        private void StartRound(EventStartRound eventStartRound)
        {
            Quaternion rotation;

            if (QuantumConnection.IsPlayerMe(eventStartRound.Player1))
            {
                _board0.SetActive(true);
                _board180.SetActive(false);
                rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (QuantumConnection.IsPlayerMe(eventStartRound.Player2))
            {
                _board0.SetActive(false);
                _board180.SetActive(true);
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                return;
            }

            _cameraParent.rotation = rotation;
            _skyboxMaterial.SetFloat("_Rotation", rotation.eulerAngles.y);

            ActiveSimulationBoard(eventStartRound.Heroes);
        }

        private void EndRound(EventEndRound eventEndRound)
        {
            _board0.SetActive(true);
            _board180.SetActive(false);
            _cameraParent.rotation = Quaternion.Euler(0, 0, 0);
            _board.SetActiveHeroes(true);
        }

        private void SetActiveEntity(EventSetActiveEntity eventSetActiveEntity)
        {
            if (QuantumConnection.IsPlayerMe(eventSetActiveEntity.PlayerRef))
            {
                SetActiveEntity(eventSetActiveEntity.Entity, eventSetActiveEntity.IsActive);
                SetLevelMesh(eventSetActiveEntity.EntityLevelData);
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

        private void SetLevelMesh(EntityLevelData entityLevelData)
        {
            if (entityLevelData.Ref == default)
            {
                return;
            }

            QuantumEntityView quantumEntityView = _quantumEntityViewUpdater.GetView(entityLevelData.Ref);

            if (quantumEntityView != null)
            {
                quantumEntityView.gameObject.GetComponentInChildren<HeroMesh>()?.SetMesh(entityLevelData.Level, entityLevelData.ID);
            }
        }
    }
}