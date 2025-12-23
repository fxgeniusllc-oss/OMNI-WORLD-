using UnityEngine;

namespace OmniWorld.World
{
    /// <summary>
    /// Manages city transit system across urban zones
    /// Handles fast travel, public transportation, and vehicle spawning
    /// </summary>
    public class CityTransitManager : MonoBehaviour
    {
        private static CityTransitManager _instance;
        public static CityTransitManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CityTransitManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CityTransitManager");
                        _instance = go.AddComponent<CityTransitManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Transit Configuration")]
        public float fastTravelCost = 25f;
        public float taxiBaseFare = 10f;
        public float taxiPerMileCost = 2f;

        [Header("Available Zones")]
        public string[] urbanZones = {
            "OmniDowntown",
            "OmniHollywood",
            "OmniCoastline",
            "OmniSuburbs",
            "OmniSouthside",
            "OmniDesert"
        };

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("CityTransitManager initialized - Transit system ready");
            Debug.Log($"Available zones: {string.Join(", ", urbanZones)}");
        }

        public void FastTravel(string destinationZone, string walletAddress)
        {
            Debug.Log($"Fast traveling to {destinationZone}");
            Debug.Log($"Cost: {fastTravelCost} OMNI");
            // TODO: Process payment and teleport player
        }

        public void CallTaxi(string destinationZone, string walletAddress)
        {
            float distance = CalculateDistance(destinationZone);
            float totalCost = taxiBaseFare + (distance * taxiPerMileCost);
            Debug.Log($"Taxi called to {destinationZone}");
            Debug.Log($"Distance: {distance:F2} miles");
            Debug.Log($"Cost: {totalCost:F2} OMNI");
            // TODO: Spawn taxi and process payment
        }

        private float CalculateDistance(string destination)
        {
            // Placeholder distance calculation
            return Random.Range(1f, 10f);
        }
    }
}
