using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace OmniWorld.World
{
    /// <summary>
    /// Airport Manager - Controls the OmniGate Travel Network
    /// Manages terminal access, flight booking, and city transitions
    /// </summary>
    public class AirportManager : MonoBehaviour
    {
        private static AirportManager _instance;
        public static AirportManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AirportManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AirportManager");
                        _instance = go.AddComponent<AirportManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Current Airport")]
        public AirportData currentAirport;
        public bool isAtAirport = false;
        
        [Header("Airport Registry")]
        private Dictionary<string, AirportData> airportRegistry = new Dictionary<string, AirportData>();
        
        [Header("Flight Settings")]
        public float flightDuration = 5.0f; // Cinematic transition time in seconds
        public bool skipFlightCinematic = false;
        
        [Header("References")]
        public TransitSystem transitSystem;
        public MusicBiomeController musicBiomeController;
        
        [Header("Economy")]
        public float baseLandingFeeMultiplier = 1.0f;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAirports();
        }

        private void Start()
        {
            // Get references
            transitSystem = TransitSystem.Instance;
            musicBiomeController = MusicBiomeController.Instance;
            
            // Start at OmniLanta airport
            EnterAirport("OmniLanta");
        }

        /// <summary>
        /// Initialize all airport terminals in the network
        /// </summary>
        private void InitializeAirports()
        {
            Debug.Log("Initializing OmniGate Travel Network...");
            
            string[] cities = { "OmniLanta", "OmniVegas", "OmniTokyo", "OmniNYC", "OmniDubai", "OmniLA", "OmniParis", "OmniLagos" };
            
            foreach (string city in cities)
            {
                AirportData airport = AirportPresets.GetAirportForCity(city);
                if (airport != null)
                {
                    airportRegistry[city] = airport;
                    Debug.Log($"Registered airport: {airport.airportName} ({airport.airportCode})");
                }
            }
            
            Debug.Log($"OmniGate Network initialized with {airportRegistry.Count} terminals");
        }

        /// <summary>
        /// Enter an airport terminal
        /// </summary>
        public void EnterAirport(string cityName)
        {
            if (!airportRegistry.ContainsKey(cityName))
            {
                Debug.LogWarning($"No airport found for city: {cityName}");
                return;
            }

            currentAirport = airportRegistry[cityName];
            isAtAirport = true;
            
            Debug.Log($"Entered {currentAirport.airportName}");
            Debug.Log($"Terminal Type: {currentAirport.terminalType}");
            Debug.Log($"Architecture: {currentAirport.architectureStyle}");
            
            if (currentAirport.availableDestinations.Count > 0)
            {
                Debug.Log($"Available destinations: {string.Join(", ", currentAirport.availableDestinations)}");
            }
        }

        /// <summary>
        /// Leave airport and enter the city
        /// </summary>
        public void LeaveAirport()
        {
            if (!isAtAirport || currentAirport == null)
            {
                Debug.LogWarning("Not currently at an airport");
                return;
            }

            Debug.Log($"Leaving {currentAirport.airportName}, entering {currentAirport.cityName}");
            isAtAirport = false;
            
            // Load music biome for the city
            if (musicBiomeController != null)
            {
                musicBiomeController.LoadBiomeForCity(currentAirport.cityName);
            }
        }

        /// <summary>
        /// Book a flight to destination city
        /// </summary>
        public bool BookFlight(string destinationCity, string walletAddress)
        {
            if (!isAtAirport || currentAirport == null)
            {
                Debug.LogWarning("Must be at an airport to book a flight");
                return false;
            }

            if (currentAirport.cityName == destinationCity)
            {
                Debug.LogWarning("Already at destination");
                return false;
            }

            if (!airportRegistry.ContainsKey(destinationCity))
            {
                Debug.LogWarning($"Unknown destination: {destinationCity}");
                return false;
            }

            AirportData destAirport = airportRegistry[destinationCity];

            // Check if destination is unlocked
            if (!destAirport.isUnlocked)
            {
                Debug.LogWarning($"{destAirport.airportName} is locked");
                Debug.Log($"Requirements: {destAirport.unlockCost} $OMNI, {destAirport.requiredReputation} reputation");
                return false;
            }

            // Check if destination is available from current airport
            if (!currentAirport.availableDestinations.Contains(destinationCity))
            {
                Debug.LogWarning($"No direct flights to {destinationCity} from {currentAirport.cityName}");
                Debug.Log($"Available destinations: {string.Join(", ", currentAirport.availableDestinations)}");
                return false;
            }

            // Calculate costs
            float travelCost = CalculateFlightCost(currentAirport.cityName, destinationCity);
            float landingFee = destAirport.baseLandingFee * baseLandingFeeMultiplier;
            float totalCost = travelCost + landingFee;

            Debug.Log($"Flight booked: {currentAirport.cityName} → {destinationCity}");
            Debug.Log($"Travel Cost: {travelCost:F2} $OMNI");
            Debug.Log($"Landing Fee: {landingFee:F2} $OMNI");
            Debug.Log($"Total Cost: {totalCost:F2} $OMNI");

            // TODO: Deduct cost from wallet
            // Economy.DominionEconomy.Instance.DeductFunds(walletAddress, totalCost);

            // Execute flight
            ExecuteFlight(destinationCity);

            return true;
        }

        /// <summary>
        /// Execute flight transition to destination
        /// </summary>
        private void ExecuteFlight(string destinationCity)
        {
            Debug.Log($"Taking off from {currentAirport.airportName}...");
            
            // TODO: Show loading screen / cinematic flight sequence
            if (!skipFlightCinematic)
            {
                Debug.Log($"Flight cinematic playing ({flightDuration}s)...");
                // Could trigger a coroutine for cinematic here
            }

            // Arrive at destination
            ArriveAtDestination(destinationCity);
        }

        /// <summary>
        /// Arrive at destination airport
        /// </summary>
        private void ArriveAtDestination(string destinationCity)
        {
            Debug.Log($"Landing at {destinationCity}...");
            
            // Update transit system
            if (transitSystem != null)
            {
                transitSystem.TravelToCity(destinationCity, ""); // Empty wallet address for now
            }

            // Enter destination airport
            EnterAirport(destinationCity);
            
            // Update game manager
            Core.GameManager.Instance?.SetCurrentCity(destinationCity);
            
            Debug.Log($"Arrived at {currentAirport.airportName}");
            Debug.Log("Ready to explore the city or book another flight");
        }

        /// <summary>
        /// Calculate flight cost between cities
        /// </summary>
        private float CalculateFlightCost(string fromCity, string toCity)
        {
            if (transitSystem != null)
            {
                // Use existing transit system distance calculation
                float distance = CalculateDistance(fromCity, toCity);
                return transitSystem.CalculateTravelCost(distance);
            }
            
            // Fallback: flat rate based on tier
            return 50f;
        }

        /// <summary>
        /// Calculate distance between cities (simplified)
        /// </summary>
        private float CalculateDistance(string fromCity, string toCity)
        {
            if (!airportRegistry.ContainsKey(fromCity) || !airportRegistry.ContainsKey(toCity))
                return 1000f;

            var fromCoords = airportRegistry[fromCity].gpsCoordinates;
            var toCoords = airportRegistry[toCity].gpsCoordinates;

            float dx = toCoords.x - fromCoords.x;
            float dy = toCoords.y - fromCoords.y;
            
            return Mathf.Sqrt(dx * dx + dy * dy) * 111f; // Rough conversion to km
        }

        /// <summary>
        /// Unlock a destination airport
        /// </summary>
        public bool UnlockAirport(string cityName, string walletAddress, int playerReputation)
        {
            if (!airportRegistry.ContainsKey(cityName))
            {
                Debug.LogWarning($"Airport not found: {cityName}");
                return false;
            }

            AirportData airport = airportRegistry[cityName];

            if (airport.isUnlocked)
            {
                Debug.Log($"{airport.airportName} is already unlocked");
                return true;
            }

            // Check reputation requirement
            if (playerReputation < airport.requiredReputation)
            {
                Debug.LogWarning($"Insufficient reputation. Required: {airport.requiredReputation}, Current: {playerReputation}");
                return false;
            }

            // Check unlock requirements (quests, achievements)
            if (airport.unlockRequirements.Count > 0)
            {
                Debug.Log($"Additional requirements: {string.Join(", ", airport.unlockRequirements)}");
                // TODO: Check if requirements are met
            }

            // Check cost
            Debug.Log($"Unlock cost: {airport.unlockCost} $OMNI");
            // TODO: Deduct cost from wallet
            // Economy.DominionEconomy.Instance.DeductFunds(walletAddress, airport.unlockCost);

            // Unlock the airport
            airport.isUnlocked = true;
            
            // Also unlock in transit system
            if (transitSystem != null)
            {
                transitSystem.UnlockCity(cityName);
            }

            Debug.Log($"✈️ {airport.airportName} unlocked!");
            Debug.Log($"You can now travel to {cityName}");

            return true;
        }

        /// <summary>
        /// Get list of unlocked airports
        /// </summary>
        public List<AirportData> GetUnlockedAirports()
        {
            return airportRegistry.Values.Where(a => a.isUnlocked).ToList();
        }

        /// <summary>
        /// Get list of locked airports
        /// </summary>
        public List<AirportData> GetLockedAirports()
        {
            return airportRegistry.Values.Where(a => !a.isUnlocked).ToList();
        }

        /// <summary>
        /// Get available destinations from current airport
        /// </summary>
        public List<string> GetAvailableDestinations()
        {
            if (currentAirport == null)
                return new List<string>();

            return currentAirport.availableDestinations
                .Where(dest => airportRegistry.ContainsKey(dest) && airportRegistry[dest].isUnlocked)
                .ToList();
        }

        /// <summary>
        /// Get airport data for a city
        /// </summary>
        public AirportData GetAirport(string cityName)
        {
            if (airportRegistry.ContainsKey(cityName))
                return airportRegistry[cityName];
            
            return null;
        }

        /// <summary>
        /// Check if player is at customs (entering city for first time)
        /// </summary>
        public bool IsAtCustoms()
        {
            return isAtAirport && currentAirport != null && currentAirport.hasCustomsNPC;
        }

        /// <summary>
        /// Access mission board at airport
        /// </summary>
        public void AccessMissionBoard()
        {
            if (!isAtAirport || currentAirport == null)
            {
                Debug.LogWarning("Not at an airport");
                return;
            }

            if (!currentAirport.hasMissionBoard)
            {
                Debug.LogWarning("This airport doesn't have a mission board");
                return;
            }

            Debug.Log($"Accessing mission board at {currentAirport.airportName}");
            Debug.Log("Available missions for this city:");
            
            // TODO: Load city-specific missions
            // AI.ProceduralGeneration.Instance.GenerateCityQuests(currentAirport.cityName);
        }

        /// <summary>
        /// Get airport info for UI display
        /// </summary>
        public string GetAirportInfo()
        {
            if (currentAirport == null)
                return "Not at an airport";

            string info = $"{currentAirport.airportName} ({currentAirport.airportCode})\n";
            info += $"Type: {currentAirport.terminalType}\n";
            info += $"Style: {currentAirport.architectureStyle}\n";
            info += $"Landing Fee: {currentAirport.baseLandingFee} $OMNI\n";
            
            if (currentAirport.availableDestinations.Count > 0)
            {
                info += $"Destinations: {currentAirport.availableDestinations.Count}";
            }

            return info;
        }
    }
}
