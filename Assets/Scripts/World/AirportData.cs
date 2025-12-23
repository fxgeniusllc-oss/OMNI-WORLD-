using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.World
{
    /// <summary>
    /// Data structure for airport terminals
    /// Represents OmniGate Travel Network locations
    /// </summary>
    [System.Serializable]
    public class AirportData
    {
        [Header("Airport Identity")]
        public string airportCode; // e.g., "ATL", "LAS", "TYO"
        public string airportName;
        public string cityName;
        public string terminalType; // "International", "Domestic", "Private"
        
        [Header("Location")]
        public Vector3 worldPosition;
        public Vector2 gpsCoordinates;
        
        [Header("Access Control")]
        public bool isUnlocked = false;
        public float unlockCost = 0f; // $OMNI cost to unlock
        public int requiredReputation = 0; // City reputation required
        public List<string> unlockRequirements = new List<string>(); // Quest IDs or achievement names
        
        [Header("Terminal Features")]
        public List<string> availableDestinations = new List<string>();
        public bool hasMissionBoard = true;
        public bool hasCustomsNPC = true;
        public bool hasLounge = false;
        public bool hasCargoArea = false;
        
        [Header("Visual Style")]
        public string architectureStyle; // "Modern", "Futuristic", "Classic"
        public string ambientLighting; // "Warm", "Cool", "Neon"
        public List<string> culturalDecor = new List<string>();
        
        [Header("Services")]
        public bool offersRentals = true; // Vehicle/property rentals
        public bool offersInsurance = true;
        public bool offersCurrencyExchange = true;
        public bool offersNFTMarketplace = true;
        
        [Header("Economy")]
        public float baseLandingFee = 10f; // Cost to use this airport
        public float economicActivityMultiplier = 1.0f;
    }
    
    /// <summary>
    /// Predefined airport configurations for OmniGate Network
    /// </summary>
    public static class AirportPresets
    {
        public static AirportData GetAirportForCity(string cityName)
        {
            switch (cityName)
            {
                case "OmniLanta":
                    return new AirportData
                    {
                        airportCode = "ATL",
                        airportName = "OmniGate Atlanta International",
                        cityName = "OmniLanta",
                        terminalType = "International",
                        worldPosition = new Vector3(0, 0, 0),
                        gpsCoordinates = new Vector2(33.6407f, -84.4277f),
                        isUnlocked = true, // Starting city
                        unlockCost = 0f,
                        requiredReputation = 0,
                        availableDestinations = new List<string> { "OmniVegas", "OmniNYC" },
                        hasMissionBoard = true,
                        hasCustomsNPC = true,
                        hasLounge = true,
                        hasCargoArea = true,
                        architectureStyle = "Modern Southern",
                        ambientLighting = "Warm",
                        culturalDecor = new List<string> { "trap_murals", "recording_studio_exhibits", "creator_hall_of_fame" },
                        offersRentals = true,
                        offersInsurance = true,
                        offersCurrencyExchange = true,
                        offersNFTMarketplace = true,
                        baseLandingFee = 5f,
                        economicActivityMultiplier = 1.2f
                    };
                    
                case "OmniVegas":
                    return new AirportData
                    {
                        airportCode = "LAS",
                        airportName = "OmniGate Las Vegas Terminal",
                        cityName = "OmniVegas",
                        terminalType = "International",
                        worldPosition = new Vector3(1000, 0, 500),
                        gpsCoordinates = new Vector2(36.0840f, -115.1537f),
                        isUnlocked = true, // Available from start
                        unlockCost = 0f,
                        requiredReputation = 0,
                        availableDestinations = new List<string> { "OmniLanta", "OmniLA", "OmniNYC" },
                        hasMissionBoard = true,
                        hasCustomsNPC = true,
                        hasLounge = true,
                        hasCargoArea = false,
                        architectureStyle = "Neon Glitz",
                        ambientLighting = "Neon",
                        culturalDecor = new List<string> { "slot_machines", "poker_tables", "neon_art" },
                        offersRentals = true,
                        offersInsurance = true,
                        offersCurrencyExchange = true,
                        offersNFTMarketplace = true,
                        baseLandingFee = 15f,
                        economicActivityMultiplier = 1.8f
                    };
                    
                case "OmniTokyo":
                    return new AirportData
                    {
                        airportCode = "TYO",
                        airportName = "OmniGate Tokyo Narita",
                        cityName = "OmniTokyo",
                        terminalType = "International",
                        worldPosition = new Vector3(-2000, 0, -1500),
                        gpsCoordinates = new Vector2(35.7720f, 140.3929f),
                        isUnlocked = false,
                        unlockCost = 500f,
                        requiredReputation = 50,
                        unlockRequirements = new List<string> { "complete_asia_expansion_quest" },
                        availableDestinations = new List<string> { "OmniDubai", "OmniLA", "OmniNYC" },
                        hasMissionBoard = true,
                        hasCustomsNPC = true,
                        hasLounge = true,
                        hasCargoArea = true,
                        architectureStyle = "Cyber Minimalist",
                        ambientLighting = "Cool",
                        culturalDecor = new List<string> { "anime_exhibits", "tech_displays", "zen_gardens" },
                        offersRentals = true,
                        offersInsurance = true,
                        offersCurrencyExchange = true,
                        offersNFTMarketplace = true,
                        baseLandingFee = 25f,
                        economicActivityMultiplier = 2.0f
                    };
                    
                case "OmniNYC":
                    return new AirportData
                    {
                        airportCode = "JFK",
                        airportName = "OmniGate JFK International",
                        cityName = "OmniNYC",
                        terminalType = "International",
                        worldPosition = new Vector3(2000, 0, 1000),
                        gpsCoordinates = new Vector2(40.6413f, -73.7781f),
                        isUnlocked = false,
                        unlockCost = 250f,
                        requiredReputation = 25,
                        availableDestinations = new List<string> { "OmniLanta", "OmniVegas", "OmniParis", "OmniLA" },
                        hasMissionBoard = true,
                        hasCustomsNPC = true,
                        hasLounge = true,
                        hasCargoArea = true,
                        architectureStyle = "Classic Metropolitan",
                        ambientLighting = "Cool",
                        culturalDecor = new List<string> { "hip_hop_history", "wall_street_art", "yankees_memorabilia" },
                        offersRentals = true,
                        offersInsurance = true,
                        offersCurrencyExchange = true,
                        offersNFTMarketplace = true,
                        baseLandingFee = 30f,
                        economicActivityMultiplier = 2.5f
                    };
                    
                case "OmniDubai":
                    return new AirportData
                    {
                        airportCode = "DXB",
                        airportName = "OmniGate Dubai International",
                        cityName = "OmniDubai",
                        terminalType = "International",
                        worldPosition = new Vector3(3000, 0, -500),
                        gpsCoordinates = new Vector2(25.2532f, 55.3657f),
                        isUnlocked = false,
                        unlockCost = 1000f,
                        requiredReputation = 75,
                        unlockRequirements = new List<string> { "achieve_mogul_status", "complete_luxury_expansion" },
                        availableDestinations = new List<string> { "OmniTokyo", "OmniParis", "OmniNYC" },
                        hasMissionBoard = true,
                        hasCustomsNPC = true,
                        hasLounge = true,
                        hasCargoArea = true,
                        architectureStyle = "Ultra Luxury",
                        ambientLighting = "Warm",
                        culturalDecor = new List<string> { "gold_accents", "luxury_cars", "desert_art" },
                        offersRentals = true,
                        offersInsurance = true,
                        offersCurrencyExchange = true,
                        offersNFTMarketplace = true,
                        baseLandingFee = 50f,
                        economicActivityMultiplier = 3.0f
                    };
                    
                case "OmniLA":
                    return new AirportData
                    {
                        airportCode = "LAX",
                        airportName = "OmniGate Los Angeles International",
                        cityName = "OmniLA",
                        terminalType = "International",
                        worldPosition = new Vector3(-1000, 0, 1500),
                        gpsCoordinates = new Vector2(33.9416f, -118.4085f),
                        isUnlocked = false,
                        unlockCost = 300f,
                        requiredReputation = 30,
                        availableDestinations = new List<string> { "OmniVegas", "OmniTokyo", "OmniNYC" },
                        hasMissionBoard = true,
                        hasCustomsNPC = true,
                        hasLounge = true,
                        hasCargoArea = false,
                        architectureStyle = "West Coast Modern",
                        ambientLighting = "Warm",
                        culturalDecor = new List<string> { "hollywood_stars", "beach_art", "lowrider_exhibits" },
                        offersRentals = true,
                        offersInsurance = true,
                        offersCurrencyExchange = true,
                        offersNFTMarketplace = true,
                        baseLandingFee = 20f,
                        economicActivityMultiplier = 1.7f
                    };
                    
                case "OmniParis":
                    return new AirportData
                    {
                        airportCode = "CDG",
                        airportName = "OmniGate Charles de Gaulle",
                        cityName = "OmniParis",
                        terminalType = "International",
                        worldPosition = new Vector3(1500, 0, -2000),
                        gpsCoordinates = new Vector2(49.0097f, 2.5479f),
                        isUnlocked = false,
                        unlockCost = 750f,
                        requiredReputation = 60,
                        unlockRequirements = new List<string> { "complete_european_expansion", "cultural_ambassador_achievement" },
                        availableDestinations = new List<string> { "OmniNYC", "OmniDubai", "OmniLA" },
                        hasMissionBoard = true,
                        hasCustomsNPC = true,
                        hasLounge = true,
                        hasCargoArea = true,
                        architectureStyle = "French Elegance",
                        ambientLighting = "Warm",
                        culturalDecor = new List<string> { "art_galleries", "fashion_displays", "eiffel_tower_model" },
                        offersRentals = true,
                        offersInsurance = true,
                        offersCurrencyExchange = true,
                        offersNFTMarketplace = true,
                        baseLandingFee = 35f,
                        economicActivityMultiplier = 2.2f
                    };
                    
                default:
                    return CreateDefaultAirport(cityName);
            }
        }
        
        private static AirportData CreateDefaultAirport(string cityName)
        {
            return new AirportData
            {
                airportCode = "UNK",
                airportName = $"{cityName} Airport",
                cityName = cityName,
                terminalType = "Domestic",
                worldPosition = Vector3.zero,
                gpsCoordinates = Vector2.zero,
                isUnlocked = false,
                unlockCost = 100f,
                requiredReputation = 10,
                availableDestinations = new List<string>(),
                hasMissionBoard = true,
                hasCustomsNPC = true,
                hasLounge = false,
                hasCargoArea = false,
                architectureStyle = "Modern",
                ambientLighting = "Neutral",
                culturalDecor = new List<string>(),
                offersRentals = false,
                offersInsurance = false,
                offersCurrencyExchange = true,
                offersNFTMarketplace = false,
                baseLandingFee = 10f,
                economicActivityMultiplier = 1.0f
            };
        }
    }
}
