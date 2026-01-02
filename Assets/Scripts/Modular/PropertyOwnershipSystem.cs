using UnityEngine;
using System.Collections.Generic;
using OmniWorld.Economy;

namespace OmniWorld.Property
{
    /// <summary>
    /// Manages property ownership, rentals, and real estate transactions
    /// NFT-based property system with smart contract integration
    /// </summary>
    public class PropertyOwnershipSystem : MonoBehaviour
    {
        private static PropertyOwnershipSystem _instance;
        public static PropertyOwnershipSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PropertyOwnershipSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PropertyOwnershipSystem");
                        _instance = go.AddComponent<PropertyOwnershipSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Property Configuration")]
        public bool nftPropertiesEnabled = true;
        public float propertyTaxRate = 0.01f;
        public int taxCollectionDays = 30;

        [Header("Property Types")]
        public List<string> propertyTypes = new List<string> {
            "Apartment",
            "Single-family home",
            "Penthouse",
            "Smart tower unit",
            "Commercial space"
        };

        private Dictionary<string, PropertyData> properties = new Dictionary<string, PropertyData>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("PropertyOwnershipSystem initialized - Real estate system ready");
            Debug.Log($"NFT Properties: {(nftPropertiesEnabled ? "Enabled" : "Disabled")}");
            Debug.Log($"Property Tax Rate: {propertyTaxRate * 100}%");
        }

        public void PurchaseProperty(string propertyId, string propertyType, float price, string buyerAddress)
        {
            Debug.Log($"=== Property Purchase ===");
            Debug.Log($"Type: {propertyType}");
            Debug.Log($"Price: {price:N0} OMNI");
            Debug.Log($"Buyer: {buyerAddress}");

            PropertyData property = new PropertyData
            {
                id = propertyId,
                propertyType = propertyType,
                owner = buyerAddress,
                purchasePrice = price,
                isNFT = nftPropertiesEnabled
            };

            properties[propertyId] = property;

            if (DominionEconomy.Instance != null)
            {
                DominionEconomy.Instance.ProcessTransaction(buyerAddress, price, "Property Purchase");
            }

            Debug.Log("✓ Property purchased successfully");
            // TODO: Mint property NFT
        }

        public void RentProperty(string propertyId, string tenantAddress, float monthlyRent)
        {
            Debug.Log($"Property {propertyId} rented to {tenantAddress} for {monthlyRent} OMNI/month");
            // TODO: Implement rental system with smart contracts
        }
    }

    [System.Serializable]
    public class PropertyData
    {
        public string id;
        public string propertyType;
        public string owner;
        public float purchasePrice;
        public bool isNFT;
    }
}
