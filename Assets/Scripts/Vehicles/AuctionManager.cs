using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using OmniWorld.Economy;

namespace OmniWorld.Vehicles
{
    /// <summary>
    /// Manages monthly vehicle auctions for ultra-rare vehicles
    /// VIP tier wallets only with global livestream visibility
    /// </summary>
    public class AuctionManager : MonoBehaviour
    {
        private static AuctionManager _instance;
        public static AuctionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AuctionManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AuctionManager");
                        _instance = go.AddComponent<AuctionManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Auction Configuration")]
        [Tooltip("Auction schedule (Monthly)")]
        public string schedule = "Monthly";
        
        [Tooltip("Day of month for auction (1st)")]
        public int dayOfMonth = 1;
        
        [Tooltip("Auction duration in hours")]
        public int durationHours = 72;
        
        [Tooltip("Minimum user prestige for VIP eligibility")]
        public float minimumPrestige = 0.8f;
        
        [Tooltip("Minimum bid increment percentage (5%)")]
        public float minimumBidIncrementPercent = 5f;
        
        [Tooltip("Global livestream enabled")]
        public bool livestreamEnabled = true;

        [Header("VIP Tiers")]
        public List<string> vipTiers = new List<string> { "Platinum", "Diamond", "Elite" };

        [Header("Current Auction")]
        public bool auctionActive = false;
        public DateTime auctionStartTime;
        public DateTime auctionEndTime;
        public string currentVehicleId;
        
        private int lastAuctionMonth = -1; // Track last auction month to prevent duplicate triggers
        
        private List<AuctionData> activeAuctions = new List<AuctionData>();
        private List<BidData> bidHistory = new List<BidData>();

        public event Action<string, float> OnNewBid;
        public event Action<string, string, float> OnAuctionWon;
        public event Action<string> OnAuctionStarted;
        public event Action<string> OnAuctionEnded;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAuctionSystem();
        }

        private void InitializeAuctionSystem()
        {
            Debug.Log("=== Auction Manager Initialized ===");
            Debug.Log($"Schedule: {schedule}");
            Debug.Log($"Day of Month: {dayOfMonth}");
            Debug.Log($"Duration: {durationHours} hours");
            Debug.Log($"Minimum Prestige: {minimumPrestige}");
            Debug.Log($"VIP Tiers: {string.Join(", ", vipTiers)}");
            Debug.Log($"Livestream: {(livestreamEnabled ? "Enabled" : "Disabled")}");
        }

        /// <summary>
        /// Start an auction for a vehicle
        /// </summary>
        public bool StartAuction(string vehicleId, string vehicleName, float startingBid)
        {
            if (auctionActive)
            {
                Debug.LogWarning("An auction is already active");
                return false;
            }

            auctionStartTime = DateTime.UtcNow;
            auctionEndTime = auctionStartTime.AddHours(durationHours);
            currentVehicleId = vehicleId;
            auctionActive = true;

            AuctionData auction = new AuctionData
            {
                vehicleId = vehicleId,
                vehicleName = vehicleName,
                startingBid = startingBid,
                currentBid = startingBid,
                startTime = auctionStartTime,
                endTime = auctionEndTime,
                isActive = true
            };

            activeAuctions.Add(auction);

            Debug.Log($"=== Auction Started ===");
            Debug.Log($"Vehicle: {vehicleName}");
            Debug.Log($"Starting Bid: {startingBid:N0} OMNI");
            Debug.Log($"Start Time: {auctionStartTime}");
            Debug.Log($"End Time: {auctionEndTime}");
            Debug.Log($"Duration: {durationHours} hours");
            Debug.Log($"Eligibility: VIP tier wallets only");
            Debug.Log($"Visibility: Global livestream");

            OnAuctionStarted?.Invoke(vehicleName);

            return true;
        }

        /// <summary>
        /// Place a bid on the current auction
        /// </summary>
        public bool PlaceBid(string walletAddress, float bidAmount, string vipTier, float userPrestige)
        {
            if (!auctionActive)
            {
                Debug.LogWarning("No active auction");
                return false;
            }

            // Check VIP eligibility
            if (!IsVIPEligible(vipTier, userPrestige))
            {
                Debug.LogWarning($"Wallet {walletAddress} is not VIP eligible");
                Debug.LogWarning($"Required: VIP tier ({string.Join(", ", vipTiers)}) and Prestige >= {minimumPrestige}");
                Debug.LogWarning($"Current: Tier={vipTier}, Prestige={userPrestige}");
                return false;
            }

            // Check if auction has ended
            if (DateTime.UtcNow >= auctionEndTime)
            {
                Debug.LogWarning("Auction has ended");
                EndAuction();
                return false;
            }

            AuctionData auction = activeAuctions.Find(a => a.vehicleId == currentVehicleId && a.isActive);
            
            if (auction == null)
            {
                Debug.LogWarning("Active auction not found");
                return false;
            }

            // Check if bid is higher than current bid
            float minimumBid = auction.currentBid * (1f + minimumBidIncrementPercent / 100f);
            if (bidAmount < minimumBid)
            {
                Debug.LogWarning($"Bid too low. Minimum bid: {minimumBid:N0} OMNI (must be {minimumBidIncrementPercent}% higher)");
                return false;
            }

            // Process bid
            BidData bid = new BidData
            {
                auctionId = auction.vehicleId,
                walletAddress = walletAddress,
                bidAmount = bidAmount,
                bidTime = DateTime.UtcNow,
                vipTier = vipTier,
                userPrestige = userPrestige
            };

            bidHistory.Add(bid);
            
            // Update current bid and leader
            string previousLeader = auction.currentLeader;
            float previousBid = auction.currentBid;
            
            auction.currentBid = bidAmount;
            auction.currentLeader = walletAddress;
            auction.bidCount++;

            Debug.Log($"=== New Bid Placed ===");
            Debug.Log($"Auction: {auction.vehicleName}");
            Debug.Log($"Bidder: {walletAddress}");
            Debug.Log($"VIP Tier: {vipTier}");
            Debug.Log($"Prestige: {userPrestige:F2}");
            Debug.Log($"Bid Amount: {bidAmount:N0} OMNI");
            Debug.Log($"Previous Bid: {previousBid:N0} OMNI");
            Debug.Log($"Bid Count: {auction.bidCount}");
            Debug.Log($"Time Remaining: {(auctionEndTime - DateTime.UtcNow).TotalHours:F2} hours");

            OnNewBid?.Invoke(walletAddress, bidAmount);

            // Notify previous leader they were outbid
            if (!string.IsNullOrEmpty(previousLeader) && previousLeader != walletAddress)
            {
                Debug.Log($"! {previousLeader} has been outbid!");
            }

            return true;
        }

        /// <summary>
        /// Check if wallet is eligible for VIP auction
        /// </summary>
        private bool IsVIPEligible(string vipTier, float userPrestige)
        {
            bool hasTier = vipTiers.Contains(vipTier);
            bool hasPrestige = userPrestige >= minimumPrestige;
            
            return hasTier && hasPrestige;
        }

        /// <summary>
        /// End the current auction and determine winner
        /// </summary>
        public void EndAuction()
        {
            if (!auctionActive)
            {
                Debug.LogWarning("No active auction to end");
                return;
            }

            AuctionData auction = activeAuctions.Find(a => a.vehicleId == currentVehicleId && a.isActive);
            
            if (auction == null)
            {
                Debug.LogWarning("Active auction not found");
                return;
            }

            auction.isActive = false;
            auctionActive = false;

            Debug.Log($"=== Auction Ended ===");
            Debug.Log($"Vehicle: {auction.vehicleName}");
            Debug.Log($"Total Bids: {auction.bidCount}");
            
            if (!string.IsNullOrEmpty(auction.currentLeader))
            {
                Debug.Log($"Winner: {auction.currentLeader}");
                Debug.Log($"Winning Bid: {auction.currentBid:N0} OMNI");
                
                // Process winning transaction
                if (DominionEconomy.Instance != null)
                {
                    DominionEconomy.Instance.ProcessTransaction(
                        auction.currentLeader,
                        auction.currentBid,
                        "Auction Win"
                    );
                }

                // Mint NFT for winner
                if (VehicleDealershipManager.Instance != null)
                {
                    VehicleDealershipManager.Instance.PurchaseVehicle(
                        auction.vehicleId,
                        auction.currentLeader,
                        false
                    );
                }

                OnAuctionWon?.Invoke(auction.vehicleName, auction.currentLeader, auction.currentBid);
                
                Debug.Log($"✓ Vehicle transferred to winner");
            }
            else
            {
                Debug.Log("No bids received - auction ended without winner");
            }

            OnAuctionEnded?.Invoke(auction.vehicleName);
        }

        /// <summary>
        /// Get current auction status
        /// </summary>
        public AuctionStatus GetAuctionStatus()
        {
            if (!auctionActive)
            {
                return new AuctionStatus
                {
                    isActive = false,
                    message = "No active auction"
                };
            }

            AuctionData auction = activeAuctions.Find(a => a.vehicleId == currentVehicleId && a.isActive);
            
            if (auction == null)
            {
                return new AuctionStatus
                {
                    isActive = false,
                    message = "Auction not found"
                };
            }

            TimeSpan timeRemaining = auctionEndTime - DateTime.UtcNow;
            
            return new AuctionStatus
            {
                isActive = true,
                vehicleName = auction.vehicleName,
                currentBid = auction.currentBid,
                currentLeader = auction.currentLeader,
                bidCount = auction.bidCount,
                timeRemainingHours = timeRemaining.TotalHours,
                message = $"{auction.vehicleName} - Current bid: {auction.currentBid:N0} OMNI"
            };
        }

        /// <summary>
        /// Get bid history for current auction
        /// </summary>
        public List<BidData> GetBidHistory(string auctionId = null)
        {
            if (string.IsNullOrEmpty(auctionId))
            {
                auctionId = currentVehicleId;
            }

            return bidHistory.FindAll(b => b.auctionId == auctionId)
                            .OrderByDescending(b => b.bidTime)
                            .ToList();
        }

        /// <summary>
        /// Check if next monthly auction should start
        /// </summary>
        private void Update()
        {
            // Check if it's the first day of the month and no auction is active
            DateTime now = DateTime.UtcNow;
            if (!auctionActive && now.Day == dayOfMonth && now.Month != lastAuctionMonth)
            {
                // Auto-start monthly auction logic could go here
                // For now, auctions must be started manually
                lastAuctionMonth = now.Month;
                Debug.Log($"Auction trigger day reached (Month: {now.Month}, Day: {now.Day}). Ready to start monthly auction.");
            }

            // Auto-end auction if time is up
            if (auctionActive && DateTime.UtcNow >= auctionEndTime)
            {
                EndAuction();
            }
        }
    }

    [System.Serializable]
    public class AuctionData
    {
        public string vehicleId;
        public string vehicleName;
        public float startingBid;
        public float currentBid;
        public string currentLeader;
        public int bidCount;
        public DateTime startTime;
        public DateTime endTime;
        public bool isActive;
    }

    [System.Serializable]
    public class BidData
    {
        public string auctionId;
        public string walletAddress;
        public float bidAmount;
        public DateTime bidTime;
        public string vipTier;
        public float userPrestige;
    }

    [System.Serializable]
    public class AuctionStatus
    {
        public bool isActive;
        public string vehicleName;
        public float currentBid;
        public string currentLeader;
        public int bidCount;
        public double timeRemainingHours;
        public string message;
    }
}
