using UnityEngine;
using System;
using System.Collections.Generic;
using OmniWorld.Economy;
using OmniWorld.World;

namespace OmniWorld.Combat
{
    /// <summary>
    /// Underground Gym Manager - Handles gym operations, memberships, and fight organization
    /// Supports 3 gym variations: Boxing Gym, MMA Training Center, Street Fighting Arena
    /// Optimized for performance and integrated with DominionEconomy
    /// </summary>
    public class UndergroundGymManager : MonoBehaviour
    {
        private static UndergroundGymManager _instance;
        public static UndergroundGymManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UndergroundGymManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UndergroundGymManager");
                        _instance = go.AddComponent<UndergroundGymManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Gym Configuration")]
        public GymType currentGymType = GymType.BoxingGym;
        public string gymName = "Iron Fist Underground";
        public string gymLocation = "OmniLanta - Industrial District";

        [Header("Membership Fees (in OMNI)")]
        [Tooltip("Daily gym access fee")]
        public float dailyMembershipFee = 5f;
        
        [Tooltip("Weekly gym access fee")]
        public float weeklyMembershipFee = 25f;
        
        [Tooltip("Monthly gym access fee")]
        public float monthlyMembershipFee = 75f;
        
        [Tooltip("Lifetime gym access fee")]
        public float lifetimeMembershipFee = 500f;

        [Header("Training Services (in OMNI per session)")]
        public float basicTrainingCost = 10f;
        public float advancedTrainingCost = 25f;
        public float eliteTrainingCost = 50f;
        public float privateLessonCost = 100f;

        [Header("Fight Organization")]
        [Tooltip("Entry fee for organized fights")]
        public float fightEntryFee = 20f;
        
        [Tooltip("Spectator entry fee")]
        public float spectatorFee = 5f;
        
        [Tooltip("Betting minimum (in OMNI)")]
        public float bettingMinimum = 10f;
        
        [Tooltip("Betting maximum (in OMNI)")]
        public float bettingMaximum = 1000f;

        [Header("Gym Economics")]
        [Tooltip("Gym revenue percentage from fights")]
        public float gymRevenueShare = 0.15f; // 15%
        
        [Tooltip("Trainer revenue percentage")]
        public float trainerRevenueShare = 0.10f; // 10%
        
        [Tooltip("Daily operational costs (in OMNI)")]
        public float dailyOperationalCost = 100f;

        [Header("Capacity & Facilities")]
        public int maxCapacity = 50;
        public int currentOccupancy = 0;
        public bool hasRing = true;
        public bool hasCage = false;
        public bool hasWeightRoom = true;
        public bool hasCardioArea = true;
        public bool hasLockerRooms = true;

        // Events
        public event Action<string, MembershipType> OnMembershipPurchased;
        public event Action<string, TrainingSession> OnTrainingStarted;
        public event Action<string, TrainingSession> OnTrainingCompleted;
        public event Action<string> OnGymEntered;
        public event Action<string> OnGymExited;
        public event Action<FightEvent> OnFightScheduled;

        private Dictionary<string, Membership> activeMemberships = new Dictionary<string, Membership>();
        private Dictionary<string, TrainingSession> activeTrainingSessions = new Dictionary<string, TrainingSession>();
        private List<FightEvent> scheduledFights = new List<FightEvent>();
        private List<string> currentOccupants = new List<string>();

        // Gym variations with unique configurations
        public static Dictionary<GymType, GymConfiguration> GymConfigurations = new Dictionary<GymType, GymConfiguration>
        {
            {
                GymType.BoxingGym,
                new GymConfiguration
                {
                    name = "Iron Fist Boxing Gym",
                    theme = "Classic Boxing - Speed & Technique",
                    description = "Underground boxing gym focused on sweet science and technical prowess",
                    specialization = "Boxing techniques, footwork, combinations, defensive skills",
                    atmosphere = "Raw, gritty, traditional. Dim lighting, heavy bags, speed bags, ring",
                    allowedFightTypes = new[] { "Boxing", "Sparring" },
                    baseStatBonus = new StatBonus { speed = 15, technique = 20, defense = 10 },
                    membershipDiscount = 0.9f, // 10% cheaper than base
                    equipmentList = new[] { "Heavy Bag", "Speed Bag", "Double End Bag", "Boxing Ring", "Jump Ropes", "Mitts" }
                }
            },
            {
                GymType.MMATrainingCenter,
                new GymConfiguration
                {
                    name = "Omega Fight Lab",
                    theme = "MMA - Complete Combat System",
                    description = "State-of-the-art underground MMA facility with octagon and full training regimen",
                    specialization = "Mixed martial arts, grappling, striking, wrestling, submissions",
                    atmosphere = "Modern, intense, professional. LED lighting, octagon cage, mats, high-tech equipment",
                    allowedFightTypes = new[] { "MMA", "Grappling", "Kickboxing", "Wrestling" },
                    baseStatBonus = new StatBonus { strength = 12, speed = 12, defense = 12, technique = 12 },
                    membershipDiscount = 1.1f, // 10% more expensive than base
                    equipmentList = new[] { "Octagon Cage", "Grappling Mats", "Heavy Bags", "Thai Pads", "Wrestling Dummies", "Submission Trainers" }
                }
            },
            {
                GymType.StreetFightArena,
                new GymConfiguration
                {
                    name = "The Pit - Street Warriors Den",
                    theme = "Street Fighting - No Rules Survival",
                    description = "Underground street fighting arena where anything goes and legends are born",
                    specialization = "Street fighting, dirty tactics, survival skills, improvised weapons",
                    atmosphere = "Dark, dangerous, chaotic. Concrete floors, graffiti walls, makeshift equipment, fighting pit",
                    allowedFightTypes = new[] { "Street Fight", "No Holds Barred", "Survival Combat" },
                    baseStatBonus = new StatBonus { strength = 20, defense = 8, technique = 5 },
                    membershipDiscount = 0.8f, // 20% cheaper, higher risk
                    equipmentList = new[] { "Fighting Pit", "Concrete Bags", "Metal Pipes", "Chain Links", "Tire Stacks", "Improvised Weights" }
                }
            }
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
            
            InitializeGym();
        }

        private void InitializeGym()
        {
            // Apply gym-specific configuration
            if (GymConfigurations.ContainsKey(currentGymType))
            {
                GymConfiguration config = GymConfigurations[currentGymType];
                gymName = config.name;
                
                // Adjust fees based on gym type
                float discount = config.membershipDiscount;
                dailyMembershipFee *= discount;
                weeklyMembershipFee *= discount;
                monthlyMembershipFee *= discount;
                
                Debug.Log($"Underground Gym Initialized: {gymName}");
                Debug.Log($"Theme: {config.theme}");
                Debug.Log($"Specialization: {config.specialization}");
            }
        }

        /// <summary>
        /// Purchase gym membership
        /// </summary>
        public bool PurchaseMembership(string playerId, MembershipType membershipType)
        {
            float cost = GetMembershipCost(membershipType);
            
            // Check if player can afford (integrate with DominionEconomy)
            if (DominionEconomy.Instance != null)
            {
                // TODO: Actual token transaction
                Debug.Log($"Processing {cost} OMNI payment for {membershipType} membership");
            }

            Membership membership = new Membership
            {
                playerId = playerId,
                type = membershipType,
                startDate = DateTime.Now,
                expiryDate = GetExpiryDate(membershipType),
                isActive = true,
                remainingAccess = GetAccessCount(membershipType)
            };

            activeMemberships[playerId] = membership;
            
            OnMembershipPurchased?.Invoke(playerId, membershipType);
            Debug.Log($"Membership purchased: {playerId} - {membershipType} - Cost: {cost} OMNI");
            
            return true;
        }

        /// <summary>
        /// Enter the gym (checks membership)
        /// </summary>
        public bool EnterGym(string playerId)
        {
            if (!HasValidMembership(playerId))
            {
                Debug.LogWarning($"{playerId} does not have valid membership");
                return false;
            }

            if (currentOccupancy >= maxCapacity)
            {
                Debug.LogWarning($"Gym at max capacity ({maxCapacity})");
                return false;
            }

            if (!currentOccupants.Contains(playerId))
            {
                currentOccupants.Add(playerId);
                currentOccupancy++;
                OnGymEntered?.Invoke(playerId);
                Debug.Log($"{playerId} entered {gymName} - Occupancy: {currentOccupancy}/{maxCapacity}");
            }

            return true;
        }

        /// <summary>
        /// Exit the gym
        /// </summary>
        public void ExitGym(string playerId)
        {
            if (currentOccupants.Contains(playerId))
            {
                currentOccupants.Remove(playerId);
                currentOccupancy--;
                OnGymExited?.Invoke(playerId);
                Debug.Log($"{playerId} exited {gymName} - Occupancy: {currentOccupancy}/{maxCapacity}");
            }
        }

        /// <summary>
        /// Start a training session
        /// </summary>
        public TrainingSession StartTraining(string playerId, TrainingType trainingType, float durationMinutes = 30f)
        {
            if (!HasValidMembership(playerId))
            {
                Debug.LogWarning($"{playerId} needs membership to train");
                return null;
            }

            float cost = GetTrainingCost(trainingType);
            
            // TODO: Process payment through DominionEconomy
            
            TrainingSession session = new TrainingSession
            {
                sessionId = $"{playerId}_{Time.time}",
                playerId = playerId,
                trainingType = trainingType,
                gymType = currentGymType,
                startTime = Time.time,
                durationMinutes = durationMinutes,
                isActive = true,
                cost = cost
            };

            activeTrainingSessions[session.sessionId] = session;
            
            OnTrainingStarted?.Invoke(playerId, session);
            Debug.Log($"Training started: {playerId} - {trainingType} - Duration: {durationMinutes}min - Cost: {cost} OMNI");
            
            return session;
        }

        /// <summary>
        /// Complete a training session and award stat bonuses
        /// </summary>
        public TrainingResult CompleteTraining(string sessionId)
        {
            if (!activeTrainingSessions.ContainsKey(sessionId))
            {
                Debug.LogError($"Training session {sessionId} not found");
                return null;
            }

            TrainingSession session = activeTrainingSessions[sessionId];
            session.isActive = false;
            session.endTime = Time.time;

            // Calculate stat gains based on training type and gym type
            StatBonus statGains = CalculateStatGains(session);
            int experienceGained = CalculateExperienceGain(session);

            TrainingResult result = new TrainingResult
            {
                session = session,
                statGains = statGains,
                experienceGained = experienceGained,
                success = true
            };

            OnTrainingCompleted?.Invoke(session.playerId, session);
            Debug.Log($"Training completed: {session.playerId} - Stats: +{statGains.strength}STR +{statGains.speed}SPD +{statGains.defense}DEF +{statGains.technique}TEC - XP: +{experienceGained}");

            activeTrainingSessions.Remove(sessionId);
            
            return result;
        }

        /// <summary>
        /// Schedule a fight event at the gym
        /// </summary>
        public FightEvent ScheduleFight(string fighter1Id, string fighter2Id, string fightType, DateTime scheduledTime, float prizePurse = 0f)
        {
            FightEvent fightEvent = new FightEvent
            {
                eventId = $"fight_{Time.time}",
                fighter1Id = fighter1Id,
                fighter2Id = fighter2Id,
                fightType = fightType,
                scheduledTime = scheduledTime,
                gymId = gymName,
                gymType = currentGymType,
                entryFee = fightEntryFee,
                prizePurse = prizePurse > 0 ? prizePurse : baseWinReward,
                isPublic = true,
                allowBetting = true
            };

            scheduledFights.Add(fightEvent);
            
            OnFightScheduled?.Invoke(fightEvent);
            Debug.Log($"Fight scheduled: {fighter1Id} vs {fighter2Id} at {scheduledTime} - Prize: {fightEvent.prizePurse} OMNI");
            
            return fightEvent;
        }

        private bool HasValidMembership(string playerId)
        {
            if (!activeMemberships.ContainsKey(playerId))
                return false;

            Membership membership = activeMemberships[playerId];
            
            // Check if membership is expired
            if (membership.expiryDate.HasValue && DateTime.Now > membership.expiryDate.Value)
            {
                membership.isActive = false;
                return false;
            }

            // Check if membership has remaining access
            if (membership.remainingAccess.HasValue && membership.remainingAccess.Value <= 0)
            {
                membership.isActive = false;
                return false;
            }

            return membership.isActive;
        }

        private float GetMembershipCost(MembershipType type)
        {
            return type switch
            {
                MembershipType.Daily => dailyMembershipFee,
                MembershipType.Weekly => weeklyMembershipFee,
                MembershipType.Monthly => monthlyMembershipFee,
                MembershipType.Lifetime => lifetimeMembershipFee,
                _ => dailyMembershipFee
            };
        }

        private DateTime? GetExpiryDate(MembershipType type)
        {
            return type switch
            {
                MembershipType.Daily => DateTime.Now.AddDays(1),
                MembershipType.Weekly => DateTime.Now.AddDays(7),
                MembershipType.Monthly => DateTime.Now.AddDays(30),
                MembershipType.Lifetime => null,
                _ => DateTime.Now.AddDays(1)
            };
        }

        private int? GetAccessCount(MembershipType type)
        {
            // Some membership types might have limited access count
            return null; // Unlimited for now
        }

        private float GetTrainingCost(TrainingType type)
        {
            return type switch
            {
                TrainingType.Basic => basicTrainingCost,
                TrainingType.Advanced => advancedTrainingCost,
                TrainingType.Elite => eliteTrainingCost,
                TrainingType.PrivateLesson => privateLessonCost,
                _ => basicTrainingCost
            };
        }

        private StatBonus CalculateStatGains(TrainingSession session)
        {
            StatBonus baseGains = new StatBonus();
            
            // Base gains from training type
            switch (session.trainingType)
            {
                case TrainingType.Basic:
                    baseGains = new StatBonus { strength = 1, speed = 1, defense = 1, technique = 1 };
                    break;
                case TrainingType.Advanced:
                    baseGains = new StatBonus { strength = 2, speed = 2, defense = 2, technique = 2 };
                    break;
                case TrainingType.Elite:
                    baseGains = new StatBonus { strength = 3, speed = 3, defense = 3, technique = 3 };
                    break;
                case TrainingType.PrivateLesson:
                    baseGains = new StatBonus { strength = 4, speed = 4, defense = 4, technique = 4 };
                    break;
            }

            // Apply gym-specific bonuses
            if (GymConfigurations.ContainsKey(session.gymType))
            {
                StatBonus gymBonus = GymConfigurations[session.gymType].baseStatBonus;
                baseGains.strength += (int)(gymBonus.strength * 0.1f);
                baseGains.speed += (int)(gymBonus.speed * 0.1f);
                baseGains.defense += (int)(gymBonus.defense * 0.1f);
                baseGains.technique += (int)(gymBonus.technique * 0.1f);
            }

            return baseGains;
        }

        private int CalculateExperienceGain(TrainingSession session)
        {
            float durationMultiplier = session.durationMinutes / 30f; // Base 30 minutes
            
            int baseXP = session.trainingType switch
            {
                TrainingType.Basic => 25,
                TrainingType.Advanced => 50,
                TrainingType.Elite => 100,
                TrainingType.PrivateLesson => 200,
                _ => 25
            };

            return (int)(baseXP * durationMultiplier);
        }

        public GymConfiguration GetCurrentGymConfig()
        {
            return GymConfigurations.ContainsKey(currentGymType) ? GymConfigurations[currentGymType] : null;
        }

        public List<FightEvent> GetScheduledFights()
        {
            return scheduledFights;
        }

        public int GetCurrentOccupancy()
        {
            return currentOccupancy;
        }
    }

    // Enums
    public enum GymType
    {
        BoxingGym,
        MMATrainingCenter,
        StreetFightArena
    }

    public enum MembershipType
    {
        Daily,
        Weekly,
        Monthly,
        Lifetime
    }

    public enum TrainingType
    {
        Basic,
        Advanced,
        Elite,
        PrivateLesson
    }

    // Data structures
    [Serializable]
    public class GymConfiguration
    {
        public string name;
        public string theme;
        public string description;
        public string specialization;
        public string atmosphere;
        public string[] allowedFightTypes;
        public StatBonus baseStatBonus;
        public float membershipDiscount;
        public string[] equipmentList;
    }

    [Serializable]
    public class StatBonus
    {
        public int strength;
        public int speed;
        public int defense;
        public int technique;
    }

    public class Membership
    {
        public string playerId;
        public MembershipType type;
        public DateTime startDate;
        public DateTime? expiryDate;
        public bool isActive;
        public int? remainingAccess;
    }

    public class TrainingSession
    {
        public string sessionId;
        public string playerId;
        public TrainingType trainingType;
        public GymType gymType;
        public float startTime;
        public float endTime;
        public float durationMinutes;
        public bool isActive;
        public float cost;
    }

    public class TrainingResult
    {
        public TrainingSession session;
        public StatBonus statGains;
        public int experienceGained;
        public bool success;
    }

    public class FightEvent
    {
        public string eventId;
        public string fighter1Id;
        public string fighter2Id;
        public string fightType;
        public DateTime scheduledTime;
        public string gymId;
        public GymType gymType;
        public float entryFee;
        public float prizePurse;
        public bool isPublic;
        public bool allowBetting;
        public List<string> spectators = new List<string>();
    }
}
