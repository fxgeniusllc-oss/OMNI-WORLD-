using UnityEngine;
using System;
using System.Collections.Generic;

namespace OmniWorld.Core
{
    /// <summary>
    /// Network manager for multiplayer synchronization
    /// Handles player connections, state sync, and real-time events
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        private static NetworkManager _instance;
        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<NetworkManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("NetworkManager");
                        _instance = go.AddComponent<NetworkManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Connection Settings")]
        public string serverUrl = "ws://localhost:8000";
        public bool isConnected = false;
        
        [Header("Player Stats")]
        public int connectedPlayers = 0;
        public List<string> activePlayers = new List<string>();

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnPlayerJoined;
        public event Action<string> OnPlayerLeft;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeNetwork();
        }

        private void InitializeNetwork()
        {
            Debug.Log("OmniWorld Network Manager Initialized");
            Debug.Log($"Server URL: {serverUrl}");
        }

        public void Connect()
        {
            // TODO: Implement actual network connection (Mirror/Photon Fusion)
            Debug.Log("Attempting to connect to server...");
            
            // Simulate connection for now
            isConnected = true;
            OnConnected?.Invoke();
            Debug.Log("Connected to OmniWorld network");
        }

        public void Disconnect()
        {
            Debug.Log("Disconnecting from server...");
            
            isConnected = false;
            connectedPlayers = 0;
            activePlayers.Clear();
            OnDisconnected?.Invoke();
        }

        public void BroadcastTransactionEvent(string transactionHash, string eventType)
        {
            Debug.Log($"Broadcasting transaction: {transactionHash} - Type: {eventType}");
            // TODO: Implement actual broadcast to other players
        }

        public void SyncPlayerState(string playerId, string stateData)
        {
            Debug.Log($"Syncing state for player: {playerId}");
            // TODO: Implement state synchronization
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }
    }
}
