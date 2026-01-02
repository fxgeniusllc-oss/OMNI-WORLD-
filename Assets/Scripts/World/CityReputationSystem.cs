using UnityEngine;
using System.Collections.Generic;
using System;

namespace OmniWorld.World
{
    /// <summary>
    /// City Reputation System - Tracks cultural reputation per city
    /// Tied to local sound, style, quests, and progression
    /// </summary>
    public class CityReputationSystem : MonoBehaviour
    {
        private static CityReputationSystem _instance;
        public static CityReputationSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CityReputationSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CityReputationSystem");
                        _instance = go.AddComponent<CityReputationSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Reputation Tracking")]
        private Dictionary<string, CityReputationData> cityReputations = new Dictionary<string, CityReputationData>();
        
        [Header("Current City")]
        public string currentCity = "OmniLanta";
        
        [Header("Reputation Thresholds")]
        public int noviceThreshold = 0;
        public int localThreshold = 25;
        public int respectedThreshold = 50;
        public int influencerThreshold = 75;
        public int legendThreshold = 100;
        
        [Header("Events")]
        public event Action<string, int, ReputationLevel> OnReputationChanged;
        public event Action<string, ReputationLevel> OnReputationLevelUp;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeReputations();
        }

        /// <summary>
        /// Initialize reputation tracking for all cities
        /// </summary>
        private void InitializeReputations()
        {
            Debug.Log("Initializing City Reputation System...");
            
            string[] cities = { "OmniLanta", "OmniVegas", "OmniTokyo", "OmniNYC", "OmniDubai", "OmniLA", "OmniParis" };
            
            foreach (string city in cities)
            {
                cityReputations[city] = new CityReputationData
                {
                    cityName = city,
                    reputationPoints = city == "OmniLanta" ? 10 : 0, // Start with some rep in starting city
                    level = city == "OmniLanta" ? ReputationLevel.Novice : ReputationLevel.Unknown,
                    questsCompleted = 0,
                    eventsAttended = 0,
                    propertyOwned = 0,
                    mentorRelationship = 0,
                    culturalKnowledge = 0,
                    musicStyleMastery = 0,
                    lastVisitDate = city == "OmniLanta" ? DateTime.Now : DateTime.MinValue
                };
            }
            
            Debug.Log($"Reputation system initialized for {cityReputations.Count} cities");
        }

        /// <summary>
        /// Get reputation for a specific city
        /// </summary>
        public CityReputationData GetCityReputation(string cityName)
        {
            if (cityReputations.ContainsKey(cityName))
                return cityReputations[cityName];
            
            Debug.LogWarning($"No reputation data for city: {cityName}");
            return null;
        }

        /// <summary>
        /// Add reputation points to a city
        /// </summary>
        public void AddReputation(string cityName, int points, string reason = "")
        {
            if (!cityReputations.ContainsKey(cityName))
            {
                Debug.LogWarning($"City not found: {cityName}");
                return;
            }

            CityReputationData repData = cityReputations[cityName];
            int oldPoints = repData.reputationPoints;
            ReputationLevel oldLevel = repData.level;
            
            repData.reputationPoints += points;
            repData.reputationPoints = Mathf.Max(0, repData.reputationPoints); // Can't go below 0
            
            // Update level
            UpdateReputationLevel(repData);
            
            Debug.Log($"+{points} reputation in {cityName}");
            if (!string.IsNullOrEmpty(reason))
                Debug.Log($"Reason: {reason}");
            
            Debug.Log($"{cityName} Reputation: {oldPoints} → {repData.reputationPoints} ({repData.level})");
            
            // Fire events
            OnReputationChanged?.Invoke(cityName, repData.reputationPoints, repData.level);
            
            // Check for level up
            if (repData.level > oldLevel)
            {
                Debug.Log($"🎉 Level Up in {cityName}! Now {repData.level}");
                OnReputationLevelUp?.Invoke(cityName, repData.level);
                HandleReputationLevelUp(cityName, repData.level);
            }
        }

        /// <summary>
        /// Update reputation level based on points
        /// </summary>
        private void UpdateReputationLevel(CityReputationData repData)
        {
            if (repData.reputationPoints >= legendThreshold)
                repData.level = ReputationLevel.Legend;
            else if (repData.reputationPoints >= influencerThreshold)
                repData.level = ReputationLevel.Influencer;
            else if (repData.reputationPoints >= respectedThreshold)
                repData.level = ReputationLevel.Respected;
            else if (repData.reputationPoints >= localThreshold)
                repData.level = ReputationLevel.Local;
            else if (repData.reputationPoints > 0)
                repData.level = ReputationLevel.Novice;
            else
                repData.level = ReputationLevel.Unknown;
        }

        /// <summary>
        /// Handle rewards and unlocks on reputation level up
        /// </summary>
        private void HandleReputationLevelUp(string cityName, ReputationLevel newLevel)
        {
            switch (newLevel)
            {
                case ReputationLevel.Local:
                    Debug.Log($"✨ {cityName} Local Status: Access to local-only missions and discounts");
                    break;
                    
                case ReputationLevel.Respected:
                    Debug.Log($"✨ {cityName} Respected Status: Mentor system unlocked, special gear available");
                    UnlockMentorSystem(cityName);
                    break;
                    
                case ReputationLevel.Influencer:
                    Debug.Log($"✨ {cityName} Influencer Status: Exclusive events, property discounts, signature items");
                    break;
                    
                case ReputationLevel.Legend:
                    Debug.Log($"✨ {cityName} Legend Status: Maximum reputation benefits, legendary items, city-wide recognition");
                    break;
            }
        }

        /// <summary>
        /// Track quest completion
        /// </summary>
        public void OnQuestCompleted(string cityName, string questId, int reputationReward)
        {
            if (!cityReputations.ContainsKey(cityName))
                return;

            CityReputationData repData = cityReputations[cityName];
            repData.questsCompleted++;
            
            AddReputation(cityName, reputationReward, $"Completed quest: {questId}");
        }

        /// <summary>
        /// Track event attendance
        /// </summary>
        public void OnEventAttended(string cityName, string eventName)
        {
            if (!cityReputations.ContainsKey(cityName))
                return;

            CityReputationData repData = cityReputations[cityName];
            repData.eventsAttended++;
            
            AddReputation(cityName, 2, $"Attended event: {eventName}");
        }

        /// <summary>
        /// Track property ownership
        /// </summary>
        public void OnPropertyAcquired(string cityName, string propertyType)
        {
            if (!cityReputations.ContainsKey(cityName))
                return;

            CityReputationData repData = cityReputations[cityName];
            repData.propertyOwned++;
            
            AddReputation(cityName, 5, $"Acquired property: {propertyType}");
        }

        /// <summary>
        /// Update mentor relationship
        /// </summary>
        public void UpdateMentorRelationship(string cityName, int progress)
        {
            if (!cityReputations.ContainsKey(cityName))
                return;

            CityReputationData repData = cityReputations[cityName];
            repData.mentorRelationship = Mathf.Clamp(repData.mentorRelationship + progress, 0, 100);
            
            Debug.Log($"{cityName} Mentor Relationship: {repData.mentorRelationship}%");
            
            if (repData.mentorRelationship >= 100)
            {
                Debug.Log("🎓 Mentor relationship maxed out! Unlocking master-level content");
            }
        }

        /// <summary>
        /// Update cultural knowledge
        /// </summary>
        public void LearnCulturalKnowledge(string cityName, int knowledge)
        {
            if (!cityReputations.ContainsKey(cityName))
                return;

            CityReputationData repData = cityReputations[cityName];
            repData.culturalKnowledge = Mathf.Clamp(repData.culturalKnowledge + knowledge, 0, 100);
            
            AddReputation(cityName, knowledge / 10, "Cultural knowledge gained");
        }

        /// <summary>
        /// Update music style mastery
        /// </summary>
        public void IncreaseMusicMastery(string cityName, int mastery)
        {
            if (!cityReputations.ContainsKey(cityName))
                return;

            CityReputationData repData = cityReputations[cityName];
            repData.musicStyleMastery = Mathf.Clamp(repData.musicStyleMastery + mastery, 0, 100);
            
            AddReputation(cityName, mastery / 5, "Music style mastery increased");
            
            if (repData.musicStyleMastery >= 100)
            {
                Debug.Log($"🎵 Music Style Mastered in {cityName}! Signature moves unlocked");
            }
        }

        /// <summary>
        /// Unlock mentor system for a city
        /// </summary>
        private void UnlockMentorSystem(string cityName)
        {
            Debug.Log($"🎓 Mentor system unlocked in {cityName}");
            
            // Assign city-specific mentor based on music biome
            string mentorName = GetCityMentor(cityName);
            Debug.Log($"Your mentor: {mentorName}");
            
            // TODO: Create mentor NPC and add to city
        }

        /// <summary>
        /// Get city-specific mentor based on cultural identity
        /// </summary>
        private string GetCityMentor(string cityName)
        {
            switch (cityName)
            {
                case "OmniNYC":
                    return "DJ Premier (Boom Bap Master)";
                case "Berlin":
                    return "Richie Hawtin (Techno Pioneer)";
                case "Lagos":
                    return "Fela Kuti Legacy (Afrobeats Legend)";
                case "OmniTokyo":
                    return "Yoko Kanno (J-Pop Innovator)";
                case "OmniLanta":
                    return "Metro Boomin (Trap Architect)";
                case "OmniVegas":
                    return "Calvin Harris (EDM Kingpin)";
                case "OmniDubai":
                    return "Amr Diab (Arabic Pop Icon)";
                case "OmniLA":
                    return "Dr. Dre (West Coast Legend)";
                case "OmniParis":
                    return "Daft Punk Legacy (French House Masters)";
                default:
                    return "Local Guide";
            }
        }

        /// <summary>
        /// Get reputation bonus multiplier for economic activities
        /// </summary>
        public float GetReputationMultiplier(string cityName)
        {
            CityReputationData repData = GetCityReputation(cityName);
            if (repData == null)
                return 1.0f;

            switch (repData.level)
            {
                case ReputationLevel.Legend:
                    return 2.0f;
                case ReputationLevel.Influencer:
                    return 1.5f;
                case ReputationLevel.Respected:
                    return 1.25f;
                case ReputationLevel.Local:
                    return 1.1f;
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Check if player can access certain content based on reputation
        /// </summary>
        public bool CanAccessContent(string cityName, ReputationLevel requiredLevel)
        {
            CityReputationData repData = GetCityReputation(cityName);
            if (repData == null)
                return false;

            return repData.level >= requiredLevel;
        }

        /// <summary>
        /// Get formatted reputation summary for UI
        /// </summary>
        public string GetReputationSummary(string cityName)
        {
            CityReputationData repData = GetCityReputation(cityName);
            if (repData == null)
                return "No data available";

            string summary = $"=== {cityName} Reputation ===\n";
            summary += $"Level: {repData.level}\n";
            summary += $"Points: {repData.reputationPoints}\n";
            summary += $"Quests Completed: {repData.questsCompleted}\n";
            summary += $"Events Attended: {repData.eventsAttended}\n";
            summary += $"Properties Owned: {repData.propertyOwned}\n";
            summary += $"Mentor Relationship: {repData.mentorRelationship}%\n";
            summary += $"Cultural Knowledge: {repData.culturalKnowledge}%\n";
            summary += $"Music Mastery: {repData.musicStyleMastery}%\n";

            return summary;
        }
    }

    /// <summary>
    /// City reputation data structure
    /// </summary>
    [System.Serializable]
    public class CityReputationData
    {
        public string cityName;
        public int reputationPoints;
        public ReputationLevel level;
        
        // Progression metrics
        public int questsCompleted;
        public int eventsAttended;
        public int propertyOwned;
        public int mentorRelationship; // 0-100
        public int culturalKnowledge; // 0-100
        public int musicStyleMastery; // 0-100
        
        // Timestamps
        public DateTime firstVisitDate;
        public DateTime lastVisitDate;
        public float totalTimeSpent; // In hours
    }

    /// <summary>
    /// Reputation levels per city
    /// </summary>
    public enum ReputationLevel
    {
        Unknown = 0,        // Never visited or 0 reputation
        Novice = 1,         // 1-24 points: Tourist
        Local = 2,          // 25-49 points: Known in the community
        Respected = 3,      // 50-74 points: Mentor system unlocked
        Influencer = 4,     // 75-99 points: City-wide recognition
        Legend = 5          // 100+ points: Maximum status, legendary items
    }
}
