using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.Vehicles
{
    /// <summary>
    /// Manages vehicle modification shop operations
    /// NFT-compatible parts system with on-chain upgrade logging
    /// Located at OmniSpeedWorks garage
    /// </summary>
    public class VehicleModShopManager : MonoBehaviour
    {
        private static VehicleModShopManager _instance;
        public static VehicleModShopManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<VehicleModShopManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("VehicleModShopManager");
                        _instance = go.AddComponent<VehicleModShopManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Mod Shop Configuration")]
        public string shopName = "OmniSpeedWorks";
        public bool nftPartsEnabled = true;
        public bool onChainLogging = true;

        [Header("Available Upgrades")]
        public List<string> engineUpgrades = new List<string> { "Stage 1 Tune", "Stage 2 Tune", "Stage 3 Turbo" };
        public List<string> suspensionUpgrades = new List<string> { "Sport", "Race", "Drift" };
        public List<string> aeroUpgrades = new List<string> { "Front Splitter", "Rear Wing", "Full Body Kit" };

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log($"{shopName} initialized - Vehicle modification shop ready");
            Debug.Log($"NFT Parts: {(nftPartsEnabled ? "Enabled" : "Disabled")}");
            Debug.Log($"On-Chain Logging: {(onChainLogging ? "Enabled" : "Disabled")}");
        }

        public void InstallUpgrade(string vehicleId, string upgradeType, string upgradeName)
        {
            Debug.Log($"Installing {upgradeName} ({upgradeType}) on vehicle {vehicleId}");
            
            if (onChainLogging)
            {
                Debug.Log("Logging upgrade to blockchain...");
                // TODO: Implement blockchain logging
            }
            
            // TODO: Apply upgrade effects to vehicle
        }

        public void MintNFTPart(string partName, string ownerAddress)
        {
            if (!nftPartsEnabled)
            {
                Debug.LogWarning("NFT parts are not enabled");
                return;
            }

            Debug.Log($"Minting NFT part: {partName} for {ownerAddress}");
            // TODO: Implement NFT minting for parts
        }
    }
}
