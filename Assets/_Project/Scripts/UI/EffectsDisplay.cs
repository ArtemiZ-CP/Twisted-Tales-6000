using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Quantum.Game
{
    public class EffectsDisplay : MonoBehaviour
    {
        [SerializeField] private HeroRangeDisplay _poisonEffectPrefab = null;
        [SerializeField] private HeroRangeDisplay _healEffectPrefab = null;

        private ObjectPool<HeroRangeDisplay> _poisonEffectPool;
        private ObjectPool<HeroRangeDisplay> _healEffectPool;

        private void Awake()
        {
            _poisonEffectPool = InitializePool(_poisonEffectPrefab);
            _healEffectPool = InitializePool(_healEffectPrefab);

            QuantumEvent.Subscribe<EventDisplayPoisonEffect>(listener: this, handler: OnDisplayPoisonEffect);
            QuantumEvent.Subscribe<EventDisplayHealEffect>(listener: this, handler: OnDisplayHealEffect);
        }

        private void OnDisplayPoisonEffect(EventDisplayPoisonEffect ev)
        {
            if (QuantumConnection.IsPlayerMe(ev.Player1) || QuantumConnection.IsPlayerMe(ev.Player2))
            {
                HeroRangeDisplay heroRangeDisplay = _poisonEffectPool.Get();
                heroRangeDisplay.Setup(ev.Range);
                heroRangeDisplay.transform.position = ev.Position.ToUnityVector3();
                StartCoroutine(ReturnEffectToPool(_poisonEffectPool, (float)ev.Duration, heroRangeDisplay));
            }
        }

        private void OnDisplayHealEffect(EventDisplayHealEffect ev)
        {
            if (QuantumConnection.IsPlayerMe(ev.Player1) || QuantumConnection.IsPlayerMe(ev.Player2))
            {
                HeroRangeDisplay heroRangeDisplay = _healEffectPool.Get();
                heroRangeDisplay.Setup(ev.Range);
                heroRangeDisplay.transform.position = ev.Position.ToUnityVector3();
                StartCoroutine(ReturnEffectToPool(_healEffectPool, (float)ev.Duration, heroRangeDisplay));
            }
        }

        private IEnumerator ReturnEffectToPool(ObjectPool<HeroRangeDisplay> pool, float delay, HeroRangeDisplay effect)
        {
            yield return new WaitForSeconds(delay);
            pool.Release(effect);
        }

        private ObjectPool<HeroRangeDisplay> InitializePool(HeroRangeDisplay prefab)
        {
            return new ObjectPool<HeroRangeDisplay>(
                createFunc: () =>
                {
                    var go = Instantiate(prefab, transform);
                    go.SetActive(false);
                    return go;
                },
                actionOnGet: (go) => go.SetActive(true),
                actionOnRelease: (go) => go.SetActive(false),
                actionOnDestroy: (go) => Destroy(go)
            );
        }
    }
}