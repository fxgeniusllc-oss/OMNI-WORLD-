using UnityEngine;
using System;
using System.Collections.Generic;

namespace OmniWorld.World
{
    /// <summary>
    /// AuctionSystem - Manages monthly auctions for ultra-rare vehicles
    /// Elite-status players can bid on 1-of-1 and exclusive vehicles
    /// </summary>
    public class AuctionSystem : MonoBehaviour
    {
        private static AuctionSystem _instance;
        public static AuctionSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AuctionSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AuctionSystem");
                        _instance = go.AddComponent<AuctionSystem>();
                    }
                }
                return _instance;
            }
        }
        
        [Header("Auction Configuration")]
        [Tooltip("Minimum prestige points required to participate")]
        public float minimumPrestigeScore = 0.8f;
        
        [Tooltip("Auction duration in hours")]
        public int auctionDurationHours = 48;
        
        [Tooltip("Minimum bid increment percentage")]
        public float minimumBidIncrementPercent = 0.05f; // 5%
        
        [Tooltip("Platform auction fee percentage")]
        public float auctionFeePercent = 0.10f; // 10%
        
        [Header("Current Auctions")]
        public List<VehicleAuction> activeAuctions = new List<VehicleAuction>();
        public List<VehicleAuction> completedAuctions = new List<VehicleAuction>();
        
        [Header("Monthly Schedule")]
        public DateTime nextAuctionDate;
        public int monthlyAuctionCount = 0;
        
        public event Action<VehicleAuction> OnAuctionStarted;
        public event Action<VehicleAuction, string, float> OnBidPlaced;
        public event Action<VehicleAuction> OnAuctionEnded;
        
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
            Debug.Log("Auction System Initialized");
            
            // Set next auction to first day of next month
            DateTime now = DateTime.UtcNow;
            nextAuctionDate = new DateTime(now.Year, now.Month, 1).AddMonths(1);
            
            Debug.Log($"Next monthly auction scheduled for: {nextAuctionDate}");
        }
        
        /// <summary>
        /// Create new auction for an ultra-rare vehicle
        /// </summary>
        public VehicleAuction CreateAuction(VehicleNFT vehicle, float startingBid, bool eliteOnly = true)
        {
            if (!vehicle.IsEliteStatus() && eliteOnly)
            {
                Debug.LogWarning($"Vehicle {vehicle.vehicleName} does not qualify for elite auction");
                return null;
            }
            
            VehicleAuction auction = new VehicleAuction
            {
                auctionId = GenerateAuctionId(),
                vehicle = vehicle,
                startingBid = startingBid,
                currentBid = startingBid,
                minimumBidIncrement = startingBid * minimumBidIncrementPercent,
                startTime = DateTime.UtcNow,
                endTime = DateTime.UtcNow.AddHours(auctionDurationHours),
                isActive = true,
                eliteOnly = eliteOnly,
                bids = new List<AuctionBid>()
            };
            
            activeAuctions.Add(auction);
            vehicle.isInAuction = true;
            
            OnAuctionStarted?.Invoke(auction);
            Debug.Log($"Auction created for {vehicle.vehicleName} - Starting bid: {startingBid} OMNI");
            
            return auction;
        }
        
        /// <summary>
        /// Place bid on active auction
        /// </summary>
        public bool PlaceBid(string auctionId, string bidderAddress, float bidAmount, float bidderPrestige)
        {
            VehicleAuction auction = activeAuctions.Find(a => a.auctionId == auctionId);
            
            if (auction == null)
            {
                Debug.LogWarning($"Auction {auctionId} not found");
                return false;
            }
            
            if (!auction.isActive)
            {
                Debug.LogWarning($"Auction {auctionId} is no longer active");
                return false;
            }
            
            if (auction.eliteOnly && bidderPrestige < minimumPrestigeScore)
            {
                Debug.LogWarning($"Bidder prestige {bidderPrestige} below minimum {minimumPrestigeScore}");
                return false;
            }
            
            if (DateTime.UtcNow > auction.endTime)
            {
                EndAuction(auction);
                return false;
            }
            
            float minimumRequiredBid = auction.currentBid + auction.minimumBidIncrement;
            if (bidAmount < minimumRequiredBid)
            {
                Debug.LogWarning($"Bid {bidAmount} below minimum required {minimumRequiredBid}");
                return false;
            }
            
            // Create bid record
            AuctionBid bid = new AuctionBid
            {
                bidder = bidderAddress,
                amount = bidAmount,
                timestamp = DateTime.UtcNow,
                prestigeScore = bidderPrestige
            };
            
            auction.bids.Add(bid);
            auction.currentBid = bidAmount;
            auction.leadingBidder = bidderAddress;
            auction.bidCount++;
            
            OnBidPlaced?.Invoke(auction, bidderAddress, bidAmount);
            Debug.Log($"Bid placed: {bidAmount} OMNI by {bidderAddress} on {auction.vehicle.vehicleName}");
            
            return true;
        }
        
        /// <summary>
        /// End auction and transfer vehicle to winner
        /// </summary>
        public void EndAuction(VehicleAuction auction)
        {
            if (!auction.isActive)
                return;
            
            auction.isActive = false;
            auction.vehicle.isInAuction = false;
            
            if (!string.IsNullOrEmpty(auction.leadingBidder) && auction.currentBid > auction.startingBid)
            {
                // Calculate fees
                float auctionFee = auction.currentBid * auctionFeePercent;
                float royalty = auction.vehicle.CalculateRoyalty(auction.currentBid);
                float sellerProceeds = auction.currentBid - auctionFee - royalty;
                
                // Transfer ownership
                auction.vehicle.TransferOwnership(auction.leadingBidder, auction.currentBid);
                
                auction.winningBid = auction.currentBid;
                auction.winner = auction.leadingBidder;
                
                Debug.Log($"Auction won by {auction.winner} for {auction.winningBid} OMNI");
                Debug.Log($"Fees: Auction={auctionFee}, Royalty={royalty}, Seller={sellerProceeds}");
            }
            else
            {
                Debug.Log($"Auction for {auction.vehicle.vehicleName} ended with no valid bids");
            }
            
            activeAuctions.Remove(auction);
            completedAuctions.Add(auction);
            
            OnAuctionEnded?.Invoke(auction);
        }
        
        /// <summary>
        /// Start monthly auction event
        /// </summary>
        public void StartMonthlyAuction(List<VehicleNFT> featuredVehicles)
        {
            monthlyAuctionCount++;
            
            Debug.Log($"=== MONTHLY AUCTION #{monthlyAuctionCount} STARTING ===");
            Debug.Log($"Featuring {featuredVehicles.Count} ultra-rare vehicles");
            
            foreach (var vehicle in featuredVehicles)
            {
                if (vehicle.IsEliteStatus())
                {
                    float startingBid = vehicle.currentValue * 0.8f; // Start at 80% of value
                    CreateAuction(vehicle, startingBid, eliteOnly: true);
                }
            }
            
            // Schedule next auction
            nextAuctionDate = nextAuctionDate.AddMonths(1);
            Debug.Log($"Next auction scheduled: {nextAuctionDate}");
        }
        
        /// <summary>
        /// Get active auctions for elite players
        /// </summary>
        public List<VehicleAuction> GetActiveEliteAuctions()
        {
            return activeAuctions.FindAll(a => a.eliteOnly && a.isActive);
        }
        
        /// <summary>
        /// Get auction history for a vehicle
        /// </summary>
        public List<VehicleAuction> GetVehicleAuctionHistory(string nftId)
        {
            return completedAuctions.FindAll(a => a.vehicle.nftId == nftId);
        }
        
        private string GenerateAuctionId()
        {
            return $"AUCTION-{DateTime.UtcNow.Ticks}-{UnityEngine.Random.Range(1000, 9999)}";
        }
        
        private void Update()
        {
            // Check for expired auctions
            for (int i = activeAuctions.Count - 1; i >= 0; i--)
            {
                if (activeAuctions[i].isActive && DateTime.UtcNow > activeAuctions[i].endTime)
                {
                    EndAuction(activeAuctions[i]);
                }
            }
        }
    }
    
    [System.Serializable]
    public class VehicleAuction
    {
        public string auctionId;
        public VehicleNFT vehicle;
        
        public float startingBid;
        public float currentBid;
        public float minimumBidIncrement;
        public float winningBid;
        
        public string leadingBidder;
        public string winner;
        
        public DateTime startTime;
        public DateTime endTime;
        
        public bool isActive;
        public bool eliteOnly;
        public int bidCount;
        
        public List<AuctionBid> bids;
    }
    
    [System.Serializable]
    public class AuctionBid
    {
        public string bidder;
        public float amount;
        public DateTime timestamp;
        public float prestigeScore;
    }
}
