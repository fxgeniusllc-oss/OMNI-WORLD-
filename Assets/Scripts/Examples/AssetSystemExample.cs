using UnityEngine;
using OmniWorld.Core;

/// <summary>
/// Example script demonstrating AssetDefinitionManager usage
/// Shows how to load and filter assets at runtime
/// </summary>
public class AssetSystemExample : MonoBehaviour
{
    [Header("Example Settings")]
    [Tooltip("Run examples on Start")]
    public bool runOnStart = true;

    [Tooltip("Show detailed logs")]
    public bool verboseLogging = true;

    private void Start()
    {
        if (runOnStart)
        {
            RunExamples();
        }
    }

    /// <summary>
    /// Run all example queries
    /// </summary>
    public void RunExamples()
    {
        Debug.Log("=== Asset Definition System Examples ===");

        // Example 1: Get registry statistics
        Example1_GetStatistics();

        // Example 2: Load assets by category
        Example2_LoadByCategory();

        // Example 3: Filter by economic tier
        Example3_FilterByTier();

        // Example 4: Filter by price range
        Example4_FilterByPrice();

        // Example 5: Get NFT assets
        Example5_GetNFTAssets();

        // Example 6: Load specific asset
        Example6_LoadSpecificAsset();

        Debug.Log("=== Examples Complete ===");
    }

    /// <summary>
    /// Example 1: Get overall statistics
    /// </summary>
    private void Example1_GetStatistics()
    {
        Debug.Log("\n--- Example 1: Registry Statistics ---");

        var assetManager = AssetDefinitionManager.Instance;
        var stats = assetManager.GetStatistics();

        if (stats != null)
        {
            Debug.Log($"Total Categories: {stats.totalCategories}");
            Debug.Log($"Total Subcategories: {stats.totalSubcategories}");
            Debug.Log($"Total Assets: {stats.totalAssets}");
            Debug.Log($"Housing: {stats.breakdown.housing}");
            Debug.Log($"Vehicles: {stats.breakdown.vehicles}");
            Debug.Log($"Avatars: {stats.breakdown.avatars}");
            Debug.Log($"Gyms: {stats.breakdown.gyms}");
            Debug.Log($"Buildings: {stats.breakdown.buildings}");
        }
        else
        {
            Debug.LogWarning("Failed to load statistics");
        }
    }

    /// <summary>
    /// Example 2: Load all assets in a category
    /// </summary>
    private void Example2_LoadByCategory()
    {
        Debug.Log("\n--- Example 2: Load Housing Category ---");

        var assetManager = AssetDefinitionManager.Instance;
        var housingAssets = assetManager.GetAssetsByCategory("housing");

        Debug.Log($"Found {housingAssets.Count} housing assets");

        if (verboseLogging)
        {
            foreach (var asset in housingAssets)
            {
                if (asset != null)
                {
                    Debug.Log($"  - {asset.prefabName}: {asset.price?.purchasePrice ?? 0} OMNI");
                }
            }
        }
    }

    /// <summary>
    /// Example 3: Filter assets by economic tier
    /// </summary>
    private void Example3_FilterByTier()
    {
        Debug.Log("\n--- Example 3: Filter by Luxury Tier ---");

        var assetManager = AssetDefinitionManager.Instance;
        var luxuryAssets = assetManager.GetAssetsByEconomicTier("Luxury");

        Debug.Log($"Found {luxuryAssets.Count} luxury tier assets");

        if (verboseLogging)
        {
            foreach (var asset in luxuryAssets)
            {
                if (asset != null)
                {
                    Debug.Log($"  - {asset.prefabName} ({asset.category}): {asset.price?.purchasePrice ?? 0} OMNI");
                }
            }
        }
    }

    /// <summary>
    /// Example 4: Filter assets by price range
    /// </summary>
    private void Example4_FilterByPrice()
    {
        Debug.Log("\n--- Example 4: Filter by Price Range (50K-200K) ---");

        var assetManager = AssetDefinitionManager.Instance;
        var affordableAssets = assetManager.GetAssetsByPriceRange(50000, 200000);

        Debug.Log($"Found {affordableAssets.Count} assets in price range");

        if (verboseLogging)
        {
            foreach (var asset in affordableAssets)
            {
                if (asset != null)
                {
                    Debug.Log($"  - {asset.prefabName}: {asset.price?.purchasePrice ?? 0} OMNI");
                }
            }
        }
    }

    /// <summary>
    /// Example 5: Get all NFT-compatible assets
    /// </summary>
    private void Example5_GetNFTAssets()
    {
        Debug.Log("\n--- Example 5: Get NFT-Compatible Assets ---");

        var assetManager = AssetDefinitionManager.Instance;
        var nftAssets = assetManager.GetNFTAssets();

        Debug.Log($"Found {nftAssets.Count} NFT-compatible assets");

        if (verboseLogging)
        {
            // Show first 10 as sample
            int count = 0;
            foreach (var asset in nftAssets)
            {
                if (asset != null && count < 10)
                {
                    Debug.Log($"  - {asset.prefabName} ({asset.metadata?.economicTier})");
                    count++;
                }
            }
            if (nftAssets.Count > 10)
            {
                Debug.Log($"  ... and {nftAssets.Count - 10} more");
            }
        }
    }

    /// <summary>
    /// Example 6: Load a specific asset definition
    /// </summary>
    private void Example6_LoadSpecificAsset()
    {
        Debug.Log("\n--- Example 6: Load Specific Asset ---");

        var assetManager = AssetDefinitionManager.Instance;
        
        // Try to load the studio apartment definition
        string assetPath = "Assets/Prefabs/Housing/Apartments/StudioApartment.json";
        var asset = assetManager.LoadAssetDefinition(assetPath);

        if (asset != null)
        {
            Debug.Log($"Loaded: {asset.prefabName}");
            Debug.Log($"  Category: {asset.category}");
            Debug.Log($"  Type: {asset.type}");
            Debug.Log($"  Economic Tier: {asset.metadata?.economicTier}");
            Debug.Log($"  Purchase Price: {asset.price?.purchasePrice ?? 0} OMNI");
            Debug.Log($"  Monthly Rent: {asset.price?.monthlyRent ?? 0} OMNI");
            Debug.Log($"  NFT Compatible: {asset.metadata?.nftCompatible ?? false}");
            
            if (asset.graphics != null)
            {
                Debug.Log($"  LOD Levels: {asset.graphics.lodLevels}");
                Debug.Log($"  Render Pipeline: {asset.graphics.renderPipeline}");
            }
            
            if (asset.components != null)
            {
                Debug.Log($"  Components: {string.Join(", ", asset.components)}");
            }
        }
        else
        {
            Debug.LogWarning($"Failed to load asset: {assetPath}");
        }
    }

    /// <summary>
    /// Example: Clear cache (useful for development/testing)
    /// </summary>
    public void ClearCache()
    {
        Debug.Log("\n--- Clearing Asset Cache ---");
        var assetManager = AssetDefinitionManager.Instance;
        assetManager.ClearCache();
        Debug.Log("Cache cleared successfully");
    }

    /// <summary>
    /// Example: Reload registry (useful for development/testing)
    /// </summary>
    public void ReloadRegistry()
    {
        Debug.Log("\n--- Reloading Asset Registry ---");
        var assetManager = AssetDefinitionManager.Instance;
        assetManager.ReloadRegistry();
        Debug.Log("Registry reloaded successfully");
    }
}
