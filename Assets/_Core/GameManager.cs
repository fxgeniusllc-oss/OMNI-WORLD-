using UnityEngine;
using System;

namespace OmniWorld.Core
{
    /// <summary>
    /// Core game manager - Optimized singleton pattern
    /// Manages game state, initialization, and core systems
    /// 
    /// OPTIMIZATION NOTES:
    /// - Thread-safe singleton with double-check locking
    /// - Event-based state management for loose coupling
    /// - Proper cleanup to prevent memory leaks
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        private static readonly object _lock = new object();
        
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindObjectOfType<GameManager>();
                            if (_instance == null)
                            {
                                GameObject go = new GameObject("GameManager");
                                _instance = go.AddComponent<GameManager>();
                                DontDestroyOnLoad(go);
                            }
                        }
                    }
                }
                return _instance;
            }
        }

        [Header("Game State")]
        public GameState currentGameState = GameState.MainMenu;
        
        [Header("Current City")]
        public string currentCity = "OmniLanta";
        
        [Header("Player Data")]
        public string playerWalletAddress;
        public float playerBalance;

        public event Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeGame();
        }

        private void InitializeGame()
        {
            LogManager.Info("=== OmniWorld Game Manager Initialized ===", new { currentCity });
        }

        public void ChangeGameState(GameState newState)
        {
            if (currentGameState != newState)
            {
                GameState oldState = currentGameState;
                currentGameState = newState;
                OnGameStateChanged?.Invoke(newState);
                LogManager.Info("Game State Changed", new { from = oldState, to = newState });
            }
        }

        public void SetCurrentCity(string cityName)
        {
            currentCity = cityName;
            LogManager.Info("City Changed", new { cityName });
        }

        private void OnApplicationQuit()
        {
            LogManager.Info("=== OmniWorld Shutting Down ===");
        }
        
        private void OnDestroy()
        {
            // Cleanup event subscriptions to prevent memory leaks
            OnGameStateChanged = null;
        }
    }

    public enum GameState
    {
        MainMenu,
        Loading,
        InGame,
        Paused,
        Transaction,
        MarketPlace
    }
}
