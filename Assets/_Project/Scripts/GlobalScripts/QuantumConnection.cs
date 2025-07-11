using System.Collections.Generic;
using UnityEngine;

namespace Quantum.Game
{
    public class QuantumConnection : MonoBehaviour
    {
        [SerializeField] private AssetRef<GameConfig> _gameConfigRef;

        private static QuantumConnection _instance;
        private static GameConfig _gameConfig;
        private static bool _isFirstTimeConnection = true;

        public static event System.Action OnConnectedToQuantum;

        public static GameConfig GameConfig
        {
            get
            {
                if (_gameConfig == null && IsAbleToConnectQuantum())
                {
                    _gameConfig = QuantumUnityDB.GetGlobalAsset(_instance._gameConfigRef);
                }

                return _gameConfig;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            _isFirstTimeConnection = true;
        }

        private void Update()
        {
            if (_isFirstTimeConnection && IsAbleToConnectQuantum())
            {
                OnConnectedToQuantum?.Invoke();
                _isFirstTimeConnection = false;
            }
        }

        public static HeroInfo GetHeroInfo(int heroID, out HeroStats heroStats)
        {
            List<HeroInfo> heroInfos = GetAssetsList(GameConfig.HeroInfos);

            if (heroID >= 0 && heroID < heroInfos.Count)
            {
                Frame frame = QuantumRunner.Default.Game.Frames.Verified;
                PlayerRef playerRef = QuantumRunner.Default.Game.GetLocalPlayers()[0];
                HeroInfo heroInfo = heroInfos[heroID];
                heroStats = GameConfig.GetHeroStats(frame, playerRef, heroInfo);
                return heroInfo;
            }

            heroStats = default;
            return null;
        }

        public static List<T> GetAssetsList<T>(AssetRef<T>[] shopItemsID) where T : AssetObject
        {
            List<T> heroInfos = new();

            for (int i = 0; i < shopItemsID.Length; i++)
            {
                heroInfos.Add(QuantumUnityDB.GetGlobalAsset(shopItemsID[i]));
            }

            return heroInfos;
        }

        public static bool IsAbleToConnectQuantum()
        {
            return QuantumRunner.Default != null &&
                QuantumRunner.Default.IsRunning &&
                QuantumRunner.Default.Session.IsSpectating == false;
        }

        public static bool IsPlayerMe(PlayerRef playerRef)
        {
            if (IsAbleToConnectQuantum() == false)
            {
                return false;
            }

            return playerRef == QuantumRunner.Default.Game.GetLocalPlayers()[0];
        }
    }
}