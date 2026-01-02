using UnityEngine;
using System;
using System.Collections.Generic;

namespace OmniWorld.Core
{
    /// <summary>
    /// Citizenship System - Manages sovereign economic units with rights and responsibilities
    /// Every player is a sovereign economic unit with voting power, reputation, and roles
    /// </summary>
    public class CitizenshipSystem : MonoBehaviour
    {
        private static CitizenshipSystem _instance;
        private static readonly object _lock = new object();
        
        public static CitizenshipSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindObjectOfType<CitizenshipSystem>();
                            if (_instance == null)
                            {
                                GameObject go = new GameObject("CitizenshipSystem");
                                _instance = go.AddComponent<CitizenshipSystem>();
                                DontDestroyOnLoad(go);
                            }
                        }
                    }
                }
                return _instance;
            }
        }

        // Citizen data storage
        private Dictionary<string, Citizen> citizens = new Dictionary<string, Citizen>();
        
        public event Action<string, CitizenRole> OnRoleGranted;
        public event Action<string, int> OnPrestigeChanged;
        public event Action<string, string> OnPropertyAcquired;

        [Serializable]
        public class Citizen
        {
            public string walletAddress;
            public string citizenId;
            public DateTime registrationDate;
            public int prestigeLevel; // 0-100, affects governance weight
            public List<CitizenRole> roles;
            public List<string> ownedProperties;
            public List<string> businesses;
            public float totalWealth;
            public int governanceVotes;
            public string primaryCity;
            public bool isActive;
        }

        [Serializable]
        public enum CitizenRole
        {
            None,
            Landlord,          // Rent-to-own property manager
            Banker,            // Loan provider with OmniCredit
            Educator,          // Learn-to-Earn CP credentials
            Mogul,             // Owns Prestige Zones
            TaxEntity,         // Runs DAO-governed district
            Creator,           // Content creator
            Entrepreneur,      // Business owner
            Investor,          // Property/asset investor
            Curator,           // Cultural curator
            Enforcer           // Community moderator
        }

        [Header("Prestige Configuration")]
        [Tooltip("Base prestige for new citizens")]
        public int basePrestige = 50;
        
        [Tooltip("Prestige gain per property owned")]
        public int prestigePerProperty = 2;
        
        [Tooltip("Prestige gain per business operated")]
        public int prestigePerBusiness = 5;
        
        [Tooltip("Prestige gain per $1000 in wealth")]
        public float prestigePerThousand = 0.5f;

        [Header("Governance Configuration")]
        [Tooltip("Voting weight multiplier for prestige levels")]
        public Dictionary<int, float> votingWeightByPrestige = new Dictionary<int, float>
        {
            { 90, 10f },  // Top 1%: 10x voting power
            { 80, 5f },   // Top 5%: 5x voting power
            { 70, 2f },   // Top 20%: 2x voting power
            { 50, 1f },   // Average: 1x voting power
            { 0, 0.5f }   // Below average: 0.5x voting power
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
            
            LogManager.Info("=== Citizenship System Initialized ===", new {
                message = "Every player is a sovereign economic unit"
            });
        }

        /// <summary>
        /// Register a new citizen
        /// </summary>
        public bool RegisterCitizen(string walletAddress, string primaryCity)
        {
            if (citizens.ContainsKey(walletAddress))
            {
                LogManager.Warn("Citizen already registered", new { walletAddress });
                return false;
            }

            Citizen citizen = new Citizen
            {
                walletAddress = walletAddress,
                citizenId = GenerateCitizenId(walletAddress),
                registrationDate = DateTime.UtcNow,
                prestigeLevel = basePrestige,
                roles = new List<CitizenRole> { CitizenRole.None },
                ownedProperties = new List<string>(),
                businesses = new List<string>(),
                totalWealth = 0f,
                governanceVotes = 0,
                primaryCity = primaryCity,
                isActive = true
            };

            citizens[walletAddress] = citizen;

            LogManager.Info("New Citizen Registered", new {
                walletAddress,
                citizenId = citizen.citizenId,
                primaryCity,
                prestigeLevel = basePrestige
            });

            return true;
        }

        /// <summary>
        /// Grant a role to a citizen
        /// </summary>
        public bool GrantRole(string walletAddress, CitizenRole role)
        {
            if (!citizens.ContainsKey(walletAddress))
            {
                LogManager.Warn("Citizen not found", new { walletAddress });
                return false;
            }

            Citizen citizen = citizens[walletAddress];
            
            if (citizen.roles.Contains(role))
            {
                LogManager.Debug("Citizen already has role", new { walletAddress, role });
                return false;
            }

            citizen.roles.Add(role);
            
            // Update prestige based on role
            UpdatePrestige(walletAddress, GetRolePrestigeBonus(role), $"Role granted: {role}");

            LogManager.Info("Role Granted", new {
                walletAddress,
                role,
                totalRoles = citizen.roles.Count
            });

            OnRoleGranted?.Invoke(walletAddress, role);
            return true;
        }

        /// <summary>
        /// Add property ownership
        /// </summary>
        public void AddProperty(string walletAddress, string propertyId, float propertyValue)
        {
            if (!citizens.ContainsKey(walletAddress))
            {
                LogManager.Warn("Citizen not found", new { walletAddress });
                return;
            }

            Citizen citizen = citizens[walletAddress];
            
            if (!citizen.ownedProperties.Contains(propertyId))
            {
                citizen.ownedProperties.Add(propertyId);
                citizen.totalWealth += propertyValue;
                
                // Grant Landlord role if this is first property
                if (citizen.ownedProperties.Count == 1 && !citizen.roles.Contains(CitizenRole.Landlord))
                {
                    GrantRole(walletAddress, CitizenRole.Landlord);
                }
                
                // Update prestige
                RecalculatePrestige(walletAddress);

                LogManager.Info("Property Acquired", new {
                    walletAddress,
                    propertyId,
                    propertyValue,
                    totalProperties = citizen.ownedProperties.Count
                });

                OnPropertyAcquired?.Invoke(walletAddress, propertyId);
            }
        }

        /// <summary>
        /// Add business ownership
        /// </summary>
        public void AddBusiness(string walletAddress, string businessId, float businessValue)
        {
            if (!citizens.ContainsKey(walletAddress))
            {
                LogManager.Warn("Citizen not found", new { walletAddress });
                return;
            }

            Citizen citizen = citizens[walletAddress];
            
            if (!citizen.businesses.Contains(businessId))
            {
                citizen.businesses.Add(businessId);
                citizen.totalWealth += businessValue;
                
                // Grant Entrepreneur role if first business
                if (citizen.businesses.Count == 1 && !citizen.roles.Contains(CitizenRole.Entrepreneur))
                {
                    GrantRole(walletAddress, CitizenRole.Entrepreneur);
                }
                
                // Update prestige
                RecalculatePrestige(walletAddress);

                LogManager.Info("Business Acquired", new {
                    walletAddress,
                    businessId,
                    businessValue,
                    totalBusinesses = citizen.businesses.Count
                });
            }
        }

        /// <summary>
        /// Update citizen wealth
        /// </summary>
        public void UpdateWealth(string walletAddress, float newWealth)
        {
            if (!citizens.ContainsKey(walletAddress))
                return;

            citizens[walletAddress].totalWealth = newWealth;
            RecalculatePrestige(walletAddress);
        }

        /// <summary>
        /// Recalculate prestige based on all factors
        /// </summary>
        private void RecalculatePrestige(string walletAddress)
        {
            if (!citizens.ContainsKey(walletAddress))
                return;

            Citizen citizen = citizens[walletAddress];
            
            int oldPrestige = citizen.prestigeLevel;
            
            // Base prestige
            int newPrestige = basePrestige;
            
            // Property bonus
            newPrestige += citizen.ownedProperties.Count * prestigePerProperty;
            
            // Business bonus
            newPrestige += citizen.businesses.Count * prestigePerBusiness;
            
            // Wealth bonus
            newPrestige += (int)(citizen.totalWealth / 1000f * prestigePerThousand);
            
            // Role bonuses
            foreach (var role in citizen.roles)
            {
                newPrestige += GetRolePrestigeBonus(role);
            }
            
            // Clamp to 0-100
            citizen.prestigeLevel = Mathf.Clamp(newPrestige, 0, 100);
            
            if (citizen.prestigeLevel != oldPrestige)
            {
                LogManager.Info("Prestige Updated", new {
                    walletAddress,
                    oldPrestige,
                    newPrestige = citizen.prestigeLevel,
                    properties = citizen.ownedProperties.Count,
                    businesses = citizen.businesses.Count,
                    wealth = citizen.totalWealth
                });

                OnPrestigeChanged?.Invoke(walletAddress, citizen.prestigeLevel);
            }
        }

        /// <summary>
        /// Update prestige by a specific amount
        /// </summary>
        private void UpdatePrestige(string walletAddress, int change, string reason)
        {
            if (!citizens.ContainsKey(walletAddress))
                return;

            Citizen citizen = citizens[walletAddress];
            int oldPrestige = citizen.prestigeLevel;
            
            citizen.prestigeLevel = Mathf.Clamp(citizen.prestigeLevel + change, 0, 100);

            LogManager.Info("Prestige Changed", new {
                walletAddress,
                oldPrestige,
                newPrestige = citizen.prestigeLevel,
                change,
                reason
            });

            OnPrestigeChanged?.Invoke(walletAddress, citizen.prestigeLevel);
        }

        /// <summary>
        /// Get prestige bonus for a role
        /// </summary>
        private int GetRolePrestigeBonus(CitizenRole role)
        {
            switch (role)
            {
                case CitizenRole.Mogul: return 15;
                case CitizenRole.TaxEntity: return 12;
                case CitizenRole.Banker: return 10;
                case CitizenRole.Entrepreneur: return 8;
                case CitizenRole.Landlord: return 5;
                case CitizenRole.Creator: return 5;
                case CitizenRole.Educator: return 5;
                case CitizenRole.Investor: return 3;
                case CitizenRole.Curator: return 3;
                case CitizenRole.Enforcer: return 3;
                default: return 0;
            }
        }

        /// <summary>
        /// Calculate voting weight based on prestige
        /// </summary>
        public float CalculateVotingWeight(string walletAddress)
        {
            if (!citizens.ContainsKey(walletAddress))
                return 0f;

            int prestige = citizens[walletAddress].prestigeLevel;
            
            // Find appropriate multiplier
            if (prestige >= 90) return 10f;  // Top 1%
            if (prestige >= 80) return 5f;   // Top 5%
            if (prestige >= 70) return 2f;   // Top 20%
            if (prestige >= 50) return 1f;   // Average
            return 0.5f;                      // Below average
        }

        /// <summary>
        /// Cast a governance vote
        /// </summary>
        public bool CastVote(string walletAddress, string proposalId, bool support)
        {
            if (!citizens.ContainsKey(walletAddress))
                return false;

            Citizen citizen = citizens[walletAddress];
            float votingWeight = CalculateVotingWeight(walletAddress);
            
            citizen.governanceVotes++;

            LogManager.Info("Vote Cast", new {
                walletAddress,
                proposalId,
                support,
                votingWeight,
                prestigeLevel = citizen.prestigeLevel
            });

            return true;
        }

        /// <summary>
        /// Get citizen information
        /// </summary>
        public Citizen GetCitizen(string walletAddress)
        {
            return citizens.ContainsKey(walletAddress) ? citizens[walletAddress] : null;
        }

        /// <summary>
        /// Check if citizen has specific role
        /// </summary>
        public bool HasRole(string walletAddress, CitizenRole role)
        {
            return citizens.ContainsKey(walletAddress) && citizens[walletAddress].roles.Contains(role);
        }

        /// <summary>
        /// Get citizen statistics
        /// </summary>
        public Dictionary<string, object> GetCitizenStats(string walletAddress)
        {
            if (!citizens.ContainsKey(walletAddress))
                return null;

            Citizen citizen = citizens[walletAddress];
            
            return new Dictionary<string, object>
            {
                { "citizenId", citizen.citizenId },
                { "prestigeLevel", citizen.prestigeLevel },
                { "votingWeight", CalculateVotingWeight(walletAddress) },
                { "roles", citizen.roles },
                { "properties", citizen.ownedProperties.Count },
                { "businesses", citizen.businesses.Count },
                { "totalWealth", citizen.totalWealth },
                { "governanceVotes", citizen.governanceVotes },
                { "primaryCity", citizen.primaryCity },
                { "memberSince", citizen.registrationDate }
            };
        }

        /// <summary>
        /// Get global citizenship statistics
        /// </summary>
        public Dictionary<string, object> GetGlobalStats()
        {
            int totalCitizens = citizens.Count;
            int activeCitizens = 0;
            int landlords = 0;
            int entrepreneurs = 0;
            int moguls = 0;
            float totalWealth = 0f;
            int totalProperties = 0;
            int totalBusinesses = 0;

            foreach (var citizen in citizens.Values)
            {
                if (citizen.isActive) activeCitizens++;
                if (citizen.roles.Contains(CitizenRole.Landlord)) landlords++;
                if (citizen.roles.Contains(CitizenRole.Entrepreneur)) entrepreneurs++;
                if (citizen.roles.Contains(CitizenRole.Mogul)) moguls++;
                
                totalWealth += citizen.totalWealth;
                totalProperties += citizen.ownedProperties.Count;
                totalBusinesses += citizen.businesses.Count;
            }

            return new Dictionary<string, object>
            {
                { "totalCitizens", totalCitizens },
                { "activeCitizens", activeCitizens },
                { "landlords", landlords },
                { "entrepreneurs", entrepreneurs },
                { "moguls", moguls },
                { "totalWealth", totalWealth },
                { "totalProperties", totalProperties },
                { "totalBusinesses", totalBusinesses },
                { "averageWealth", totalCitizens > 0 ? totalWealth / totalCitizens : 0f }
            };
        }

        /// <summary>
        /// Generate unique citizen ID
        /// </summary>
        private string GenerateCitizenId(string walletAddress)
        {
            // Use first 8 chars of wallet + timestamp
            string shortAddress = walletAddress.Substring(0, Math.Min(8, walletAddress.Length));
            string timestamp = DateTime.UtcNow.Ticks.ToString().Substring(0, 8);
            return $"C-{shortAddress}-{timestamp}";
        }
    }
}
