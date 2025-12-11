using UnityEngine;
using System;

namespace OmniWorld.Core
{
    /// <summary>
    /// Core game manager - Singleton pattern
    /// Manages game state, initialization, and core systems
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
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
            Debug.Log("OmniWorld Game Manager Initialized");
            Debug.Log($"Starting in city: {currentCity}");
        }

        public void ChangeGameState(GameState newState)
        {
            if (currentGameState != newState)
            {
                currentGameState = newState;
                OnGameStateChanged?.Invoke(newState);
                Debug.Log($"Game State Changed: {newState}");
            }
        }

        public void SetCurrentCity(string cityName)
        {
            currentCity = cityName;
            Debug.Log($"Changed city to: {cityName}");
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OmniWorld shutting down...");
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
