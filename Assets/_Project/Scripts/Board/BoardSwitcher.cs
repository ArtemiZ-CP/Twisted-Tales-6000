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
            SetRotation(true);
        }

        private void StartRound(EventStartRound eventStartRound)
        {
            if (QuantumConnection.IsPlayerMe(eventStartRound.Player1))
            {
                SetRotation(true);
            }
            else if (QuantumConnection.IsPlayerMe(eventStartRound.Player2))
            {
                SetRotation(false);
            }
            else
            {
                return;
            }

            ActiveSimulationBoard(eventStartRound.Heroes);
        }

        private void EndRound(EventEndRound eventEndRound)
        {
            SetRotation(true);
            _board.SetActiveHeroes(true);
        }

        private void SetRotation(bool zero)
        {
            _board0.SetActive(zero);
            _board180.SetActive(zero == false);

            float rotation = zero ? 0 : 180;

            _cameraParent.rotation = Quaternion.Euler(0, rotation, 0); ;
            _skyboxMaterial.SetFloat("_Rotation", rotation);
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