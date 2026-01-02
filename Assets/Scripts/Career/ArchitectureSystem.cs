using UnityEngine;
using System.Collections.Generic;
using OmniWorld.Web3;
using OmniWorld.Economy;

namespace OmniWorld.Career
{
    /// <summary>
    /// Architecture Career Path System
    /// Enables creation of 3D blueprints that can be minted as NFTs
    /// and potentially built in the real world
    /// </summary>
    public class ArchitectureSystem : MonoBehaviour
    {
        public static ArchitectureSystem Instance { get; private set; }

        [Header("Blueprint Configuration")]
        [Tooltip("Maximum number of blueprints per architect")]
        public int maxBlueprintsPerArchitect = 100;

        [Tooltip("Base cost to create a blueprint (in $OMNI)")]
        public float blueprintCreationCost = 50f;

        [Tooltip("NFT minting fee (in $OMNI)")]
        public float nftMintingFee = 100f;

        [Header("Real World Integration")]
        [Tooltip("Enable real-world build potential tracking")]
        public bool enableRealWorldBuilds = true;

        [Tooltip("Minimum reputation required for real-world certified blueprints")]
        public float minReputationForCertification = 0.8f;

        private Dictionary<string, List<BlueprintData>> architectBlueprints = new Dictionary<string, List<BlueprintData>>();
        private List<BlueprintData> allBlueprints = new List<BlueprintData>();
        private int nextBlueprintId = 1000;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Create a new architectural blueprint
        /// </summary>
        public BlueprintData CreateBlueprint(string architectAddress, BlueprintType type, string structureName, Vector3 dimensions)
        {
            if (architectBlueprints.ContainsKey(architectAddress) && 
                architectBlueprints[architectAddress].Count >= maxBlueprintsPerArchitect)
            {
                Debug.LogWarning($"Architect {architectAddress} has reached maximum blueprint limit");
                return null;
            }

            if (DominionEconomy.Instance != null)
            {
                float currentPrice = DominionEconomy.Instance.CalculateTokenPrice();
                Debug.Log($"Blueprint creation cost: {blueprintCreationCost} $OMNI (${blueprintCreationCost * currentPrice} USD)");
            }

            BlueprintData blueprint = new BlueprintData
            {
                id = nextBlueprintId++,
                architectAddress = architectAddress,
                structureName = structureName,
                blueprintType = type,
                dimensions = dimensions,
                creationDate = System.DateTime.Now,
                isNFTMinted = false,
                isCertifiedForRealWorld = false,
                estimatedBuildCost = CalculateEstimatedBuildCost(dimensions, type),
                valueInOMNI = CalculateBlueprintValue(dimensions, type)
            };

            if (!architectBlueprints.ContainsKey(architectAddress))
            {
                architectBlueprints[architectAddress] = new List<BlueprintData>();
            }
            architectBlueprints[architectAddress].Add(blueprint);
            allBlueprints.Add(blueprint);

            Debug.Log($"Blueprint created: {structureName} (ID: {blueprint.id}) by {architectAddress}");
            return blueprint;
        }

        /// <summary>
        /// Mint blueprint as NFT
        /// </summary>
        public bool MintBlueprintNFT(int blueprintId, string architectAddress)
        {
            BlueprintData blueprint = GetBlueprintById(blueprintId);
            
            if (blueprint == null || blueprint.architectAddress != architectAddress || blueprint.isNFTMinted)
            {
                Debug.LogError("Cannot mint blueprint");
                return false;
            }

            if (DominionEconomy.Instance != null)
            {
                float currentPrice = DominionEconomy.Instance.CalculateTokenPrice();
                Debug.Log($"NFT minting fee: {nftMintingFee} $OMNI (${nftMintingFee * currentPrice} USD)");
            }

            if (ContractBridge.Instance != null)
            {
                string metadata = GenerateBlueprintMetadata(blueprint);
                Debug.Log($"Minting blueprint NFT with metadata: {metadata}");
                
                blueprint.isNFTMinted = true;
                blueprint.nftTokenId = $"ARCH-{blueprint.id}-{System.DateTime.Now.Ticks}";
                blueprint.mintDate = System.DateTime.Now;

                Debug.Log($"Blueprint {blueprintId} minted as NFT: {blueprint.nftTokenId}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Certify blueprint for real-world construction
        /// </summary>
        public bool CertifyForRealWorld(int blueprintId, float architectReputation)
        {
            if (!enableRealWorldBuilds)
                return false;

            BlueprintData blueprint = GetBlueprintById(blueprintId);
            
            if (blueprint == null || architectReputation < minReputationForCertification || !blueprint.isNFTMinted)
            {
                Debug.LogWarning("Cannot certify blueprint");
                return false;
            }

            bool meetsStandards = ValidateBuildingStandards(blueprint);
            
            if (meetsStandards)
            {
                blueprint.isCertifiedForRealWorld = true;
                blueprint.certificationDate = System.DateTime.Now;
                blueprint.realWorldBuildingCode = $"RW-{blueprint.id}-{System.DateTime.Now.Year}";
                blueprint.valueInOMNI *= 1.5f;
                
                Debug.Log($"Blueprint {blueprintId} certified for real-world construction!");
                return true;
            }

            return false;
        }

        private float CalculateEstimatedBuildCost(Vector3 dimensions, BlueprintType type)
        {
            float volume = dimensions.x * dimensions.y * dimensions.z;
            float baseCostPerCubicMeter = type switch
            {
                BlueprintType.Residential => 1500f,
                BlueprintType.Commercial => 2000f,
                BlueprintType.Industrial => 1200f,
                BlueprintType.Luxury => 5000f,
                BlueprintType.Infrastructure => 3000f,
                _ => 1500f
            };
            return volume * baseCostPerCubicMeter;
        }

        private float CalculateBlueprintValue(Vector3 dimensions, BlueprintType type)
        {
            float baseValue = (dimensions.x * dimensions.y * dimensions.z) / 10f;
            float typeMultiplier = type switch
            {
                BlueprintType.Residential => 1.0f,
                BlueprintType.Commercial => 1.5f,
                BlueprintType.Industrial => 1.2f,
                BlueprintType.Luxury => 3.0f,
                BlueprintType.Infrastructure => 2.0f,
                _ => 1.0f
            };
            return baseValue * typeMultiplier;
        }

        private bool ValidateBuildingStandards(BlueprintData blueprint)
        {
            if (blueprint.dimensions.x <= 0 || blueprint.dimensions.y <= 0 || blueprint.dimensions.z <= 0)
                return false;
            if (blueprint.dimensions.y > 300f)
                return false;
            return true;
        }

        private string GenerateBlueprintMetadata(BlueprintData blueprint)
        {
            return $"{{\"name\":\"{blueprint.structureName}\",\"type\":\"{blueprint.blueprintType}\"}}";
        }

        public BlueprintData GetBlueprintById(int id)
        {
            return allBlueprints.Find(b => b.id == id);
        }

        public List<BlueprintData> GetCertifiedBlueprints()
        {
            return allBlueprints.FindAll(b => b.isCertifiedForRealWorld);
        }
    }

    [System.Serializable]
    public class BlueprintData
    {
        public int id;
        public string architectAddress;
        public string structureName;
        public BlueprintType blueprintType;
        public Vector3 dimensions;
        public System.DateTime creationDate;
        public bool isNFTMinted;
        public string nftTokenId;
        public System.DateTime mintDate;
        public bool isCertifiedForRealWorld;
        public string realWorldBuildingCode;
        public System.DateTime certificationDate;
        public float estimatedBuildCost;
        public float valueInOMNI;
    }

    public enum BlueprintType
    {
        Residential,
        Commercial,
        Industrial,
        Luxury,
        Infrastructure
    }
}
