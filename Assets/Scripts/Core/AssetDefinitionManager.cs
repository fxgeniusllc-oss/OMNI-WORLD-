using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OmniWorld.Core
{
    /// <summary>
    /// Manager for loading and managing asset definitions from JSON files
    /// Provides centralized access to all asset registry and definition data
    /// Integrates with Dominion Economy and NFT systems
    /// </summary>
    public class AssetDefinitionManager : MonoBehaviour
    {
        private static AssetDefinitionManager _instance;
        private static readonly object _lock = new object();

        public static AssetDefinitionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindObjectOfType<AssetDefinitionManager>();
                            if (_instance == null)
                            {
                                GameObject go = new GameObject("AssetDefinitionManager");
                                _instance = go.AddComponent<AssetDefinitionManager>();
                                DontDestroyOnLoad(go);
                            }
                        }
                    }
                }
                return _instance;
            }
        }

        [Header("Registry Settings")]
        [Tooltip("Path to the master asset registry JSON file")]
        public string registryPath = "Assets/AssetRegistry.json";

        [Header("Cache Settings")]
        [Tooltip("Enable caching of loaded definitions")]
        public bool enableCaching = true;

        [Tooltip("Maximum cache size in MB")]
        public int maxCacheSizeMB = 50;

        // Cached data
        private AssetRegistry _registry;
        private Dictionary<string, AssetDefinition> _definitionCache;
        private Dictionary<string, List<AssetDefinition>> _categoryCache;
        private bool _isInitialized = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize caches
            if (enableCaching)
            {
                _definitionCache = new Dictionary<string, AssetDefinition>();
                _categoryCache = new Dictionary<string, List<AssetDefinition>>();
            }

            // Load registry on startup
            LoadRegistry();
        }

        /// <summary>
        /// Load the master asset registry
        /// </summary>
        public bool LoadRegistry()
        {
            try
            {
                if (!File.Exists(registryPath))
                {
                    Debug.LogError($"Asset registry not found at: {registryPath}");
                    return false;
                }

                string jsonData = File.ReadAllText(registryPath);
                _registry = JsonUtility.FromJson<AssetRegistry>(jsonData);

                if (_registry == null)
                {
                    Debug.LogError("Failed to parse asset registry JSON");
                    return false;
                }

                _isInitialized = true;
                Debug.Log($"Asset registry loaded successfully. Total assets: {_registry.statistics.totalAssets}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading asset registry: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load a specific asset definition by path
        /// </summary>
        /// <param name="assetPath">Path to the asset JSON file</param>
        /// <returns>AssetDefinition or null if not found</returns>
        public AssetDefinition LoadAssetDefinition(string assetPath)
        {
            // Check cache first
            if (enableCaching && _definitionCache.ContainsKey(assetPath))
            {
                return _definitionCache[assetPath];
            }

            try
            {
                if (!File.Exists(assetPath))
                {
                    Debug.LogWarning($"Asset definition not found: {assetPath}");
                    return null;
                }

                string jsonData = File.ReadAllText(assetPath);
                AssetDefinition definition = JsonUtility.FromJson<AssetDefinition>(jsonData);

                // Cache the definition
                if (enableCaching && definition != null)
                {
                    _definitionCache[assetPath] = definition;
                }

                return definition;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading asset definition from {assetPath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get all assets in a specific category
        /// </summary>
        /// <param name="category">Category name (e.g., "housing", "vehicles")</param>
        /// <returns>List of asset definitions in the category</returns>
        public List<AssetDefinition> GetAssetsByCategory(string category)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("Asset registry not initialized");
                return new List<AssetDefinition>();
            }

            // Check cache
            string cacheKey = $"category_{category}";
            if (enableCaching && _categoryCache.ContainsKey(cacheKey))
            {
                return _categoryCache[cacheKey];
            }

            List<AssetDefinition> assets = new List<AssetDefinition>();

            // Find category in registry
            var categoryData = GetCategoryData(category);
            if (categoryData == null)
            {
                Debug.LogWarning($"Category not found: {category}");
                return assets;
            }

            // Load all assets in subcategories
            foreach (var subcategory in categoryData.subcategories)
            {
                foreach (var assetFile in subcategory.assets)
                {
                    string fullPath = Path.Combine(subcategory.path, assetFile);
                    var definition = LoadAssetDefinition(fullPath);
                    if (definition != null)
                    {
                        assets.Add(definition);
                    }
                }
            }

            // Cache the results
            if (enableCaching)
            {
                _categoryCache[cacheKey] = assets;
            }

            return assets;
        }

        /// <summary>
        /// Get assets filtered by economic tier
        /// </summary>
        /// <param name="tier">Economic tier (Entry, Standard, Premium, Luxury, Ultra-Luxury)</param>
        /// <returns>List of assets in the specified tier</returns>
        public List<AssetDefinition> GetAssetsByEconomicTier(string tier)
        {
            List<AssetDefinition> filteredAssets = new List<AssetDefinition>();

            if (!_isInitialized)
            {
                Debug.LogWarning("Asset registry not initialized");
                return filteredAssets;
            }

            // Search through all categories
            foreach (var category in new[] { "housing", "vehicles", "avatars", "gyms", "buildings" })
            {
                var assets = GetAssetsByCategory(category);
                filteredAssets.AddRange(assets.Where(a => 
                    a.metadata != null && 
                    a.metadata.economicTier.Equals(tier, StringComparison.OrdinalIgnoreCase)));
            }

            return filteredAssets;
        }

        /// <summary>
        /// Get assets filtered by price range
        /// </summary>
        /// <param name="minPrice">Minimum price</param>
        /// <param name="maxPrice">Maximum price</param>
        /// <returns>List of assets within the price range</returns>
        public List<AssetDefinition> GetAssetsByPriceRange(float minPrice, float maxPrice)
        {
            List<AssetDefinition> filteredAssets = new List<AssetDefinition>();

            if (!_isInitialized)
            {
                Debug.LogWarning("Asset registry not initialized");
                return filteredAssets;
            }

            // Search through all categories
            foreach (var category in new[] { "housing", "vehicles", "avatars", "gyms", "buildings" })
            {
                var assets = GetAssetsByCategory(category);
                filteredAssets.AddRange(assets.Where(a => 
                    a.price != null && 
                    a.price.purchasePrice >= minPrice && 
                    a.price.purchasePrice <= maxPrice));
            }

            return filteredAssets;
        }

        /// <summary>
        /// Get NFT-compatible assets only
        /// </summary>
        /// <returns>List of NFT-compatible assets</returns>
        public List<AssetDefinition> GetNFTAssets()
        {
            List<AssetDefinition> nftAssets = new List<AssetDefinition>();

            if (!_isInitialized)
            {
                Debug.LogWarning("Asset registry not initialized");
                return nftAssets;
            }

            // Search through all categories
            foreach (var category in new[] { "housing", "vehicles", "avatars", "gyms", "buildings" })
            {
                var assets = GetAssetsByCategory(category);
                nftAssets.AddRange(assets.Where(a => 
                    a.metadata != null && 
                    a.metadata.nftCompatible));
            }

            return nftAssets;
        }

        /// <summary>
        /// Get registry statistics
        /// </summary>
        /// <returns>Registry statistics object</returns>
        public RegistryStatistics GetStatistics()
        {
            return _registry?.statistics;
        }

        /// <summary>
        /// Clear all cached definitions
        /// </summary>
        public void ClearCache()
        {
            if (_definitionCache != null)
            {
                _definitionCache.Clear();
                Debug.Log("Asset definition cache cleared");
            }

            if (_categoryCache != null)
            {
                _categoryCache.Clear();
                Debug.Log("Category cache cleared");
            }
        }

        /// <summary>
        /// Reload the registry from disk
        /// </summary>
        public void ReloadRegistry()
        {
            ClearCache();
            LoadRegistry();
        }

        // Helper method to get category data from registry
        private CategoryData GetCategoryData(string category)
        {
            if (_registry?.categories == null)
            {
                return null;
            }

            switch (category.ToLower())
            {
                case "housing":
                    return _registry.categories.housing;
                case "vehicles":
                    return _registry.categories.vehicles;
                case "avatars":
                    return _registry.categories.avatars;
                case "gyms":
                    return _registry.categories.gyms;
                case "buildings":
                    return _registry.categories.buildings;
                default:
                    return null;
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ClearCache();
            }
        }
    }

    // Data classes for JSON deserialization
    [Serializable]
    public class AssetRegistry
    {
        public string version;
        public string lastUpdated;
        public string description;
        public Categories categories;
        public RegistryStatistics statistics;
        public EconomicTier[] economicTiers;
        public NFTCategories nftCategories;
        public Integrations integrations;
    }

    [Serializable]
    public class Categories
    {
        public CategoryData housing;
        public CategoryData vehicles;
        public CategoryData avatars;
        public CategoryData gyms;
        public CategoryData buildings;
    }

    [Serializable]
    public class CategoryData
    {
        public string name;
        public string description;
        public string path;
        public Subcategory[] subcategories;
    }

    [Serializable]
    public class Subcategory
    {
        public string name;
        public string path;
        public int count;
        public string economicTier;
        public PriceRange priceRange;
        public string[] assets;
        public string[] nftVehicles;
    }

    [Serializable]
    public class PriceRange
    {
        public float min;
        public float max;
        public string currency;
    }

    [Serializable]
    public class RegistryStatistics
    {
        public int totalCategories;
        public int totalSubcategories;
        public int totalAssets;
        public AssetBreakdown breakdown;
    }

    [Serializable]
    public class AssetBreakdown
    {
        public int housing;
        public int vehicles;
        public int avatars;
        public int gyms;
        public int buildings;
    }

    [Serializable]
    public class EconomicTier
    {
        public string name;
        public PriceRange priceRange;
        public string description;
    }

    [Serializable]
    public class NFTCategories
    {
        public NFTCategory standard;
        public NFTCategory limited;
        public NFTCategory oneOfOne;
    }

    [Serializable]
    public class NFTCategory
    {
        public string description;
        public float royalty;
        public float creatorShare;
    }

    [Serializable]
    public class Integrations
    {
        public bool dominionEconomy;
        public bool web3;
        public string blockchain;
        public string[] standards;
    }

    [Serializable]
    public class AssetDefinition
    {
        public string prefabName;
        public string category;
        public string subCategory;
        public string type;
        public AssetMetadata metadata;
        public AssetSpecifications specifications;
        public GraphicsSettings graphics;
        public string[] components;
        public PriceData price;
    }

    [Serializable]
    public class AssetMetadata
    {
        public bool nftCompatible;
        public string ownershipType;
        public string economicTier;
        public string dominionZone;
    }

    [Serializable]
    public class AssetSpecifications
    {
        // Housing
        public int squareFootage;
        public int bedrooms;
        public int bathrooms;
        public int floors;
        public int parkingSpaces;

        // Vehicles
        public string vehicleClass;
        public int seats;
        public string engine;
        public int horsepower;
        public int topSpeed;
        public float acceleration;
        public float handling;
        public string fuelType;
    }

    [Serializable]
    public class GraphicsSettings
    {
        public int lodLevels;
        public LODPolyCount polyCount;
        public TextureResolution textureResolution;
        public string[] materials;
        public LightingSettings lighting;
        public string renderPipeline;
        public string[] realisticFeatures;
    }

    [Serializable]
    public class LODPolyCount
    {
        public int lod0;
        public int lod1;
        public int lod2;
        public int lod3;
    }

    [Serializable]
    public class TextureResolution
    {
        public int diffuse;
        public int normal;
        public int roughness;
        public int metallic;
    }

    [Serializable]
    public class LightingSettings
    {
        public int realtimeLights;
        public bool lightProbes;
        public bool reflectionProbes;
        public bool ambientOcclusion;
    }

    [Serializable]
    public class PriceData
    {
        public float purchasePrice;
        public float monthlyRent;
        public float dailyRent;
        public string currency;
    }
}
