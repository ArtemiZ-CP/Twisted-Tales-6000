using UnityEngine;

namespace Quantum.Game
{
    public class PlayerInventorySlot : MonoBehaviour
    {
        [SerializeField] private HeroObject _hero;
        [SerializeField] private GameObject _heroParent;
        [SerializeField] private Transform _slot;

        public HeroObject Hero => _hero;
        public Vector3 HeroParentPosition => _heroParent.transform.position;
        public Vector3 SlotPosition => _slot.position;
        public bool IsSlotEmpty => _hero.Id < 0;

        public void SetInventoryItem(int heroId, int level)
        {
            if (heroId < 0)
            {
                _heroParent.SetActive(false);
                _hero.SetHeroState(this, heroId: -1);
            }
            else
            {
                _heroParent.SetActive(true);
                _hero.SetHeroState(this, heroId);
            }

            _hero.SetLevel(level);
        }
    }
}