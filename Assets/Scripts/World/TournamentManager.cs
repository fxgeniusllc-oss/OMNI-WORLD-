using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OmniWorld.World
{
    /// <summary>
    /// TournamentManager - Manages tournament logic, matchmaking, and trophy awards
    /// </summary>
    public class TournamentManager : MonoBehaviour
    {
        private static TournamentManager _instance;
        public static TournamentManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TournamentManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TournamentManager");
                        _instance = go.AddComponent<TournamentManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Tournament Configuration")]
        public bool tournamentsEnabled = true;
        public int maxPlayersPerTournament = 32;
        public float minEntryFeeOMNI = 10f;
        
        [Header("Trophy Settings")]
        public bool enableTrophyNFTs = true;
        public bool enableSmartContractBots = true;
        public int botDurationMonths = 6;  // 6-12 months for bot lifetime
        
        [Header("Tournament Registry")]
        public List<Tournament> activeTournaments = new List<Tournament>();
        public List<Tournament> completedTournaments = new List<Tournament>();
        public List<TrophyNFT> awardedTrophies = new List<TrophyNFT>();

        // Events
        public event Action<Tournament> OnTournamentCreated;
        public event Action<Tournament> OnTournamentStarted;
        public event Action<Tournament> OnTournamentCompleted;
        public event Action<TrophyNFT, string> OnTrophyAwarded;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("Tournament Manager Initialized");
        }

        /// <summary>
        /// Create a new tournament
        /// </summary>
        public Tournament CreateTournament(
            string name,
            TrophyRank requiredRank,
            float entryFee,
            float prizePool,
            string tournamentType,
            TournamentDifficulty difficulty
        )
        {
            if (!tournamentsEnabled)
            {
                Debug.LogWarning("Tournaments are currently disabled");
                return null;
            }

            Tournament tournament = new Tournament
            {
                tournamentId = GenerateTournamentId(),
                name = name,
                requiredRank = requiredRank,
                entryFee = entryFee,
                prizePool = prizePool,
                tournamentType = tournamentType,
                difficulty = difficulty,
                status = TournamentStatus.Registration,
                createdDate = DateTime.UtcNow,
                registeredPlayers = new List<TournamentPlayer>()
            };

            activeTournaments.Add(tournament);
            OnTournamentCreated?.Invoke(tournament);

            Debug.Log($"Tournament created: {name} (ID: {tournament.tournamentId})");
            return tournament;
        }

        /// <summary>
        /// Register player for tournament
        /// </summary>
        public bool RegisterPlayer(string tournamentId, string playerAddress, TrophyNFT[] playerTrophies)
        {
            Tournament tournament = activeTournaments.FirstOrDefault(t => t.tournamentId == tournamentId);
            
            if (tournament == null)
            {
                Debug.LogWarning($"Tournament {tournamentId} not found");
                return false;
            }

            if (tournament.status != TournamentStatus.Registration)
            {
                Debug.LogWarning($"Tournament {tournamentId} is not accepting registrations");
                return false;
            }

            // Check VIP access requirement
            if (!HasRequiredVIPAccess(playerTrophies, tournament.requiredRank))
            {
                Debug.LogWarning($"Player does not have required VIP access for {tournament.name}");
                return false;
            }

            // Check if already registered
            if (tournament.registeredPlayers.Any(p => p.playerAddress == playerAddress))
            {
                Debug.LogWarning($"Player {playerAddress} is already registered");
                return false;
            }

            // Check max players
            if (tournament.registeredPlayers.Count >= maxPlayersPerTournament)
            {
                Debug.LogWarning($"Tournament {tournament.name} is full");
                return false;
            }

            // Register player
            TournamentPlayer player = new TournamentPlayer
            {
                playerAddress = playerAddress,
                registrationDate = DateTime.UtcNow,
                entryFeePaid = tournament.entryFee,
                score = 0,
                finalRanking = 0
            };

            tournament.registeredPlayers.Add(player);
            Debug.Log($"Player {playerAddress} registered for {tournament.name}");

            // Start tournament if enough players
            if (tournament.registeredPlayers.Count >= 8)
            {
                StartTournament(tournamentId);
            }

            return true;
        }

        /// <summary>
        /// Start tournament
        /// </summary>
        public bool StartTournament(string tournamentId)
        {
            Tournament tournament = activeTournaments.FirstOrDefault(t => t.tournamentId == tournamentId);
            
            if (tournament == null) return false;
            
            if (tournament.status != TournamentStatus.Registration)
            {
                Debug.LogWarning($"Tournament {tournamentId} cannot be started");
                return false;
            }

            tournament.status = TournamentStatus.InProgress;
            tournament.startDate = DateTime.UtcNow;

            OnTournamentStarted?.Invoke(tournament);
            Debug.Log($"Tournament {tournament.name} started with {tournament.registeredPlayers.Count} players");

            return true;
        }

        /// <summary>
        /// Complete tournament and award trophies
        /// </summary>
        public bool CompleteTournament(string tournamentId, List<TournamentPlayer> finalRankings)
        {
            Tournament tournament = activeTournaments.FirstOrDefault(t => t.tournamentId == tournamentId);
            
            if (tournament == null) return false;
            
            if (tournament.status != TournamentStatus.InProgress)
            {
                Debug.LogWarning($"Tournament {tournamentId} is not in progress");
                return false;
            }

            // Update tournament
            tournament.status = TournamentStatus.Completed;
            tournament.endDate = DateTime.UtcNow;
            tournament.registeredPlayers = finalRankings;

            // Award trophies to top players
            AwardTrophies(tournament);

            // Move to completed
            activeTournaments.Remove(tournament);
            completedTournaments.Add(tournament);

            OnTournamentCompleted?.Invoke(tournament);
            Debug.Log($"Tournament {tournament.name} completed");

            return true;
        }

        /// <summary>
        /// Award trophy NFTs to winners
        /// </summary>
        private void AwardTrophies(Tournament tournament)
        {
            if (!enableTrophyNFTs) return;

            // Determine trophy rank based on tournament prestige
            TrophyRank trophyRank = DetermineTrophyRank(tournament);

            // Award to top 3 players
            for (int i = 0; i < Mathf.Min(3, tournament.registeredPlayers.Count); i++)
            {
                TournamentPlayer player = tournament.registeredPlayers[i];
                
                string trophyName = GetTrophyName(trophyRank, i + 1);
                
                TrophyNFT trophy = new TrophyNFT(
                    trophyName,
                    trophyRank,
                    tournament.name,
                    tournament.tournamentType
                );

                trophy.currentOwner = player.playerAddress;
                trophy.originalWinner = player.playerAddress;
                trophy.participantCount = tournament.registeredPlayers.Count;
                trophy.prizePool = tournament.prizePool;
                trophy.difficulty = tournament.difficulty.ToString();
                trophy.playerRanking = i + 1;

                // Attach smart contract trading bot for Gold/Silver trophies
                if (enableSmartContractBots && (trophyRank == TrophyRank.Gold || trophyRank == TrophyRank.Silver))
                {
                    string botAddress = GenerateBotAddress();
                    TradingStrategy strategy = trophyRank == TrophyRank.Gold 
                        ? TradingStrategy.Aggressive 
                        : TradingStrategy.Balanced;
                    
                    trophy.AttachTradingBot(botAddress, strategy, botDurationMonths);
                }

                awardedTrophies.Add(trophy);
                OnTrophyAwarded?.Invoke(trophy, player.playerAddress);

                Debug.Log($"Trophy awarded: {trophyName} to {player.playerAddress}");
            }
        }

        /// <summary>
        /// Determine trophy rank based on tournament difficulty and prize pool
        /// </summary>
        private TrophyRank DetermineTrophyRank(Tournament tournament)
        {
            // Elite championship tournaments award Gold trophies
            if (tournament.difficulty == TournamentDifficulty.Championship && tournament.prizePool >= 10000f)
            {
                return TrophyRank.Gold;
            }
            
            // High-stakes tournaments award Silver trophies
            if (tournament.difficulty >= TournamentDifficulty.Elite && tournament.prizePool >= 2000f)
            {
                return TrophyRank.Silver;
            }
            
            // Entry-level tournaments award Bronze trophies
            return TrophyRank.Bronze;
        }

        /// <summary>
        /// Get trophy name based on rank and placement
        /// </summary>
        private string GetTrophyName(TrophyRank rank, int placement)
        {
            string rankName = rank switch
            {
                TrophyRank.Gold => "OmniWorld Legend Trophy",
                TrophyRank.Silver => "OmniWorld Master Medal",
                TrophyRank.Bronze => "OmniWorld Challenger Badge",
                _ => "OmniWorld Trophy"
            };

            string placementText = placement switch
            {
                1 => "Champion",
                2 => "Runner-Up",
                3 => "3rd Place",
                _ => ""
            };

            return $"{rankName} - {placementText}";
        }

        /// <summary>
        /// Check if player has required VIP access
        /// </summary>
        private bool HasRequiredVIPAccess(TrophyNFT[] trophies, TrophyRank requiredRank)
        {
            if (requiredRank == TrophyRank.Bronze) return true; // Bronze is open to all
            
            foreach (var trophy in trophies)
            {
                if (trophy.HasVIPAccessToTournament(requiredRank))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Get player's highest XP boost multiplier
        /// </summary>
        public float GetPlayerXPBoost(TrophyNFT[] trophies)
        {
            if (trophies == null || trophies.Length == 0) return 1.0f;
            
            float maxBoost = 1.0f;
            foreach (var trophy in trophies)
            {
                if (trophy.xpBoostMultiplier > maxBoost)
                {
                    maxBoost = trophy.xpBoostMultiplier;
                }
            }
            
            return maxBoost;
        }

        /// <summary>
        /// Get all active tournaments player can join
        /// </summary>
        public List<Tournament> GetAvailableTournaments(TrophyNFT[] playerTrophies)
        {
            return activeTournaments
                .Where(t => t.status == TournamentStatus.Registration)
                .Where(t => HasRequiredVIPAccess(playerTrophies, t.requiredRank))
                .ToList();
        }

        private string GenerateTournamentId()
        {
            return $"TOUR-{DateTime.UtcNow.Ticks}-{UnityEngine.Random.Range(1000, 9999)}";
        }

        private string GenerateBotAddress()
        {
            return $"0x{Guid.NewGuid().ToString("N").Substring(0, 40)}";
        }
    }

    /// <summary>
    /// Tournament data structure
    /// </summary>
    [System.Serializable]
    public class Tournament
    {
        public string tournamentId;
        public string name;
        public TrophyRank requiredRank;        // Minimum rank required to enter
        public float entryFee;                 // Entry fee in $OMNI
        public float prizePool;                // Total prize pool in $OMNI
        public string tournamentType;          // "PvP", "Race", "Combat", etc.
        public TournamentDifficulty difficulty;
        public TournamentStatus status;
        public DateTime createdDate;
        public DateTime startDate;
        public DateTime endDate;
        public List<TournamentPlayer> registeredPlayers;
    }

    /// <summary>
    /// Tournament player data
    /// </summary>
    [System.Serializable]
    public class TournamentPlayer
    {
        public string playerAddress;
        public DateTime registrationDate;
        public float entryFeePaid;
        public int score;
        public int finalRanking;
    }

    /// <summary>
    /// Tournament status
    /// </summary>
    public enum TournamentStatus
    {
        Registration,
        InProgress,
        Completed,
        Cancelled
    }

    /// <summary>
    /// Tournament difficulty levels
    /// </summary>
    public enum TournamentDifficulty
    {
        Beginner,
        Intermediate,
        Advanced,
        Elite,
        Championship
    }
}
