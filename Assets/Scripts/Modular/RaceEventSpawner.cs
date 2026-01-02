using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.Racing
{
    /// <summary>
    /// Spawns and manages racing events across urban zones
    /// Supports street races, track races, and drift competitions
    /// </summary>
    public class RaceEventSpawner : MonoBehaviour
    {
        private static RaceEventSpawner _instance;
        public static RaceEventSpawner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RaceEventSpawner>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("RaceEventSpawner");
                        _instance = go.AddComponent<RaceEventSpawner>();
                    }
                }
                return _instance;
            }
        }

        [Header("Race Configuration")]
        public int maxRacers = 8;
        public float entryFee = 500f;
        public float prizePool = 3500f;

        [Header("Race Types")]
        public List<string> raceTypes = new List<string> { "Street Race", "Track Race", "Drift Competition", "Time Attack" };

        [Header("Active Races")]
        public int activeRaceCount = 0;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("RaceEventSpawner initialized - Racing system ready");
        }

        public void SpawnRaceEvent(string raceType, string zoneName)
        {
            Debug.Log($"Spawning {raceType} in {zoneName}");
            Debug.Log($"Entry Fee: {entryFee} OMNI");
            Debug.Log($"Prize Pool: {prizePool} OMNI");
            Debug.Log($"Max Racers: {maxRacers}");
            activeRaceCount++;
            // TODO: Implement race event spawning and management
        }

        public void JoinRace(string raceId, string vehicleId, string walletAddress)
        {
            Debug.Log($"Player {walletAddress} joining race {raceId} with vehicle {vehicleId}");
            // TODO: Process entry fee and add player to race
        }
    }
}
