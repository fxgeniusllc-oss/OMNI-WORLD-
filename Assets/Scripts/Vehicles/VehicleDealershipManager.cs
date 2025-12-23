using UnityEngine;
using System;
using System.Collections.Generic;
using OmniWorld.Economy;
using OmniWorld.World;

namespace OmniWorld.Vehicles
{
    /// <summary>
    /// Manages vehicle dealership operations including inventory, sales, and NFT minting
    /// Handles OmniLux Auto dealership in Vegas Strip Zone
    /// </summary>
    public class VehicleDealershipManager : MonoBehaviour
    {
        private static VehicleDealershipManager _instance;
        public static VehicleDealershipManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<VehicleDealershipManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("VehicleDealershipManager");
                        _instance = go.AddComponent<VehicleDealershipManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Dealership Configuration")]
        [Tooltip("Name of the dealership")]
        public string dealershipName = "OmniLux Auto";
        
        [Tooltip("Location of the dealership")]
        public string location = "Vegas Strip Zone";
        
        [Tooltip("City where dealership is located")]
        public string city = "OmniVegas";
        
        [Tooltip("Operating hours")]
        public string operatingHours = "24/7";

        [Header("Economic Settings")]
        [Tooltip("Transaction fee percentage (1.5%)")]
        public float transactionFee = 0.015f;
        
        [Tooltip("Dealer commission percentage (5%)")]
        public float dealerCommission = 0.05f;
        
        [Tooltip("Accepts ETH payments")]
        public bool acceptsEth = true;

        [Header("NFT Configuration")]
        [Tooltip("Enables NFT vehicle minting")]
        public bool nftMintingEnabled = true;
        
        [Tooltip("NFT royalty percentage (20%)")]
        public float nftRoyaltyPercent = 20f;
        
        [Tooltip("NFT vehicle parts compatible")]
        public bool nftVehicleParts = true;
        
        [Tooltip("Upgrade logs stored on-chain")]
        public bool upgradeLogsOnChain = true;

        [Header("Services")]
        [Tooltip("Vehicle inspection service cost")]
        public float inspectionCost = 50f;
        
        [Tooltip("Test drive service cost")]
        public float testDriveCost = 100f;
        
        [Tooltip("Financing available")]
        public bool financingEnabled = true;
        
        [Tooltip("Down payment percentage for financing")]
        public float downPaymentPercent = 20f;
        
        [Tooltip("Maximum financing term in months")]
        public int maxFinancingTermMonths = 36;
        
        [Tooltip("Interest rate for financing")]
        public float interestRate = 5.5f;

        [Header("Inventory")]
        public List<VehicleData> exclusiveVehicles = new List<VehicleData>();
        public List<string> standardVehicles = new List<string>();

        public event Action<string, float> OnVehicleSold;
        public event Action<string, string> OnVehicleInspected;
        public event Action<string, bool> OnNFTMinted;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeDealership();
        }

        private void InitializeDealership()
        {
            Debug.Log($"=== {dealershipName} Initialized ===");
            Debug.Log($"Location: {location}, {city}");
            Debug.Log($"Operating Hours: {operatingHours}");
            Debug.Log($"NFT Minting: {(nftMintingEnabled ? "Enabled" : "Disabled")}");
            Debug.Log($"Financing: {(financingEnabled ? "Enabled" : "Disabled")}");
            
            // Initialize exclusive vehicles inventory
            InitializeExclusiveInventory();
            
            Debug.Log($"Inventory: {exclusiveVehicles.Count} exclusive vehicles, {standardVehicles.Count} standard models");
        }

        private void InitializeExclusiveInventory()
        {
            // Aether Phantom GT (1-of-1)
            exclusiveVehicles.Add(new VehicleData
            {
                id = "001",
                name = "Aether Phantom GT",
                rarity = "1-of-1",
                priceOmni = 3750000f,
                priceEth = 12.5f,
                available = true,
                isNFT = true,
                tier = "Ultra-Legendary",
                horsepower = 1800,
                topSpeed = 280,
                vehicleClass = "Hypercar"
            });

            // Stratos Lynx V (10-of-10)
            exclusiveVehicles.Add(new VehicleData
            {
                id = "002",
                name = "Stratos Lynx V",
                rarity = "10-of-10",
                priceOmni = 1200000f,
                priceEth = 4.0f,
                available = true,
                isNFT = true,
                tier = "Legendary",
                horsepower = 950,
                topSpeed = 235,
                vehicleClass = "Supercar"
            });

            // Initialize standard vehicle references
            standardVehicles.AddRange(new[]
            {
                "Hypercar", "Supercar", "ElectricRoadster", "RacingPedigreeCoupe",
                "UltraLuxCoupe", "PrestigeSedan", "LuxuryElectricSUV",
                "LuxuryLimoSUV", "GrandTourer"
            });
        }

        /// <summary>
        /// Purchase a vehicle with $OMNI tokens
        /// </summary>
        public bool PurchaseVehicle(string vehicleId, string walletAddress, bool payWithEth = false)
        {
            VehicleData vehicle = exclusiveVehicles.Find(v => v.id == vehicleId);
            
            if (vehicle == null)
            {
                Debug.LogWarning($"Vehicle {vehicleId} not found in inventory");
                return false;
            }

            if (!vehicle.available)
            {
                Debug.LogWarning($"Vehicle {vehicle.name} is not available for purchase");
                return false;
            }

            float price = payWithEth ? vehicle.priceEth : vehicle.priceOmni;
            string currency = payWithEth ? "ETH" : "OMNI";

            // Calculate total cost with fees
            float transactionFeeAmount = price * transactionFee;
            float commissionAmount = price * dealerCommission;
            float totalCost = price + transactionFeeAmount + commissionAmount;

            Debug.Log($"=== Vehicle Purchase ===");
            Debug.Log($"Vehicle: {vehicle.name} ({vehicle.rarity})");
            Debug.Log($"Base Price: {price:F2} {currency}");
            Debug.Log($"Transaction Fee: {transactionFeeAmount:F2} {currency} ({transactionFee * 100:F1}%)");
            Debug.Log($"Dealer Commission: {commissionAmount:F2} {currency} ({dealerCommission * 100:F1}%)");
            Debug.Log($"Total Cost: {totalCost:F2} {currency}");
            Debug.Log($"Wallet: {walletAddress}");

            // Process transaction through DominionEconomy if paying with OMNI
            if (!payWithEth && DominionEconomy.Instance != null)
            {
                bool transactionSuccess = DominionEconomy.Instance.ProcessTransaction(
                    walletAddress, 
                    vehicle.priceOmni, 
                    "Vehicle Purchase"
                );

                if (!transactionSuccess)
                {
                    Debug.LogError("Transaction failed through DominionEconomy");
                    return false;
                }
            }

            // Mark vehicle as sold
            vehicle.available = false;
            vehicle.owner = walletAddress;
            vehicle.purchaseDate = DateTime.UtcNow;

            // Mint NFT if enabled
            if (vehicle.isNFT && nftMintingEnabled)
            {
                MintVehicleNFT(vehicle, walletAddress);
            }

            OnVehicleSold?.Invoke(vehicle.name, totalCost);
            
            Debug.Log($"✓ Purchase successful! {vehicle.name} now owned by {walletAddress}");
            
            return true;
        }

        /// <summary>
        /// Mint an NFT for the purchased vehicle
        /// </summary>
        private void MintVehicleNFT(VehicleData vehicle, string ownerAddress)
        {
            Debug.Log($"=== Minting NFT ===");
            Debug.Log($"Vehicle: {vehicle.name}");
            Debug.Log($"Owner: {ownerAddress}");
            Debug.Log($"Rarity: {vehicle.rarity}");
            Debug.Log($"Royalty: {nftRoyaltyPercent}%");
            
            // In production, this would call smart contract minting
            // For now, we log the mint operation
            vehicle.nftMinted = true;
            vehicle.nftTokenId = Guid.NewGuid().ToString();
            
            OnNFTMinted?.Invoke(vehicle.name, true);
            
            Debug.Log($"✓ NFT Minted! Token ID: {vehicle.nftTokenId}");
        }

        /// <summary>
        /// Perform vehicle inspection service
        /// </summary>
        public bool InspectVehicle(string vehicleId, string walletAddress)
        {
            VehicleData vehicle = exclusiveVehicles.Find(v => v.id == vehicleId);
            
            if (vehicle == null)
            {
                Debug.LogWarning($"Vehicle {vehicleId} not found");
                return false;
            }

            Debug.Log($"=== Vehicle Inspection ===");
            Debug.Log($"Vehicle: {vehicle.name}");
            Debug.Log($"Cost: {inspectionCost} OMNI");
            Debug.Log($"Features: Full 360° walkaround, Interior view, Engine inspection, Performance metrics");

            // Process inspection payment
            if (DominionEconomy.Instance != null)
            {
                DominionEconomy.Instance.ProcessTransaction(
                    walletAddress,
                    inspectionCost,
                    "Vehicle Inspection"
                );
            }

            OnVehicleInspected?.Invoke(vehicle.name, walletAddress);

            Debug.Log($"✓ Inspection completed for {vehicle.name}");
            return true;
        }

        /// <summary>
        /// Schedule a test drive
        /// </summary>
        public bool ScheduleTestDrive(string vehicleId, string walletAddress)
        {
            VehicleData vehicle = exclusiveVehicles.Find(v => v.id == vehicleId);
            
            if (vehicle == null || !vehicle.available)
            {
                Debug.LogWarning($"Vehicle not available for test drive");
                return false;
            }

            Debug.Log($"=== Test Drive Scheduled ===");
            Debug.Log($"Vehicle: {vehicle.name}");
            Debug.Log($"Cost: {testDriveCost} OMNI");
            Debug.Log($"Duration: 5 minutes");
            Debug.Log($"Track Available: Yes");

            // Process test drive payment
            if (DominionEconomy.Instance != null)
            {
                DominionEconomy.Instance.ProcessTransaction(
                    walletAddress,
                    testDriveCost,
                    "Test Drive"
                );
            }

            Debug.Log($"✓ Test drive scheduled for {vehicle.name}");
            return true;
        }

        /// <summary>
        /// Calculate financing terms for a vehicle
        /// </summary>
        public FinancingTerms CalculateFinancing(string vehicleId, int termMonths)
        {
            if (!financingEnabled)
            {
                Debug.LogWarning("Financing is not enabled");
                return null;
            }

            VehicleData vehicle = exclusiveVehicles.Find(v => v.id == vehicleId);
            
            if (vehicle == null)
            {
                Debug.LogWarning($"Vehicle {vehicleId} not found");
                return null;
            }

            if (termMonths > maxFinancingTermMonths)
            {
                Debug.LogWarning($"Term exceeds maximum of {maxFinancingTermMonths} months");
                return null;
            }

            float downPayment = vehicle.priceOmni * (downPaymentPercent / 100f);
            float financeAmount = vehicle.priceOmni - downPayment;
            float monthlyRate = (interestRate / 100f) / 12f;
            float monthlyPayment = financeAmount * (monthlyRate * Mathf.Pow(1 + monthlyRate, termMonths)) / 
                                   (Mathf.Pow(1 + monthlyRate, termMonths) - 1);
            float totalPayment = downPayment + (monthlyPayment * termMonths);
            float totalInterest = totalPayment - vehicle.priceOmni;

            FinancingTerms terms = new FinancingTerms
            {
                vehicleName = vehicle.name,
                basePrice = vehicle.priceOmni,
                downPayment = downPayment,
                financeAmount = financeAmount,
                termMonths = termMonths,
                interestRate = interestRate,
                monthlyPayment = monthlyPayment,
                totalPayment = totalPayment,
                totalInterest = totalInterest
            };

            Debug.Log($"=== Financing Terms ===");
            Debug.Log($"Vehicle: {terms.vehicleName}");
            Debug.Log($"Base Price: {terms.basePrice:N0} OMNI");
            Debug.Log($"Down Payment: {terms.downPayment:N0} OMNI ({downPaymentPercent}%)");
            Debug.Log($"Finance Amount: {terms.financeAmount:N0} OMNI");
            Debug.Log($"Term: {terms.termMonths} months");
            Debug.Log($"Interest Rate: {terms.interestRate:F2}%");
            Debug.Log($"Monthly Payment: {terms.monthlyPayment:N0} OMNI");
            Debug.Log($"Total Payment: {terms.totalPayment:N0} OMNI");
            Debug.Log($"Total Interest: {terms.totalInterest:N0} OMNI");

            return terms;
        }

        /// <summary>
        /// Get available exclusive vehicles
        /// </summary>
        public List<VehicleData> GetAvailableExclusiveVehicles()
        {
            return exclusiveVehicles.FindAll(v => v.available);
        }

        /// <summary>
        /// Get vehicle by ID
        /// </summary>
        public VehicleData GetVehicleById(string vehicleId)
        {
            return exclusiveVehicles.Find(v => v.id == vehicleId);
        }
    }

    [System.Serializable]
    public class VehicleData
    {
        public string id;
        public string name;
        public string rarity;
        public float priceOmni;
        public float priceEth;
        public bool available;
        public bool isNFT;
        public string tier;
        public int horsepower;
        public int topSpeed;
        public string vehicleClass;
        public string owner;
        public DateTime purchaseDate;
        public bool nftMinted;
        public string nftTokenId;
    }

    [System.Serializable]
    public class FinancingTerms
    {
        public string vehicleName;
        public float basePrice;
        public float downPayment;
        public float financeAmount;
        public int termMonths;
        public float interestRate;
        public float monthlyPayment;
        public float totalPayment;
        public float totalInterest;
    }
}
