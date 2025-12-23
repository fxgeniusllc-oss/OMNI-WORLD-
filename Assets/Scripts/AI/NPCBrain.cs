using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.AI
{
    /// <summary>
    /// NPC Brain with AI-powered behavior
    /// Integrates with GPT for dialogue and decision-making
    /// </summary>
    public class NPCBrain : MonoBehaviour
    {
        [Header("NPC Identity")]
        public string npcName = "Citizen";
        public NPCRole role = NPCRole.Citizen;
        public string personality = "friendly";

        [Header("Behavior")]
        public float interactionRange = 3f;
        public bool canTrade = true;
        public bool hasQuests = true;

        [Header("Memory")]
        public List<string> conversationHistory = new List<string>();
        public Dictionary<string, int> playerRelationships = new Dictionary<string, int>();
        
        [Header("Economic Activity")]
        public float walletBalance = 1000f;
        public List<string> ownedAssets = new List<string>();

        private bool isInteracting = false;
        private Transform currentPlayer;

        private void Start()
        {
            InitializeNPC();
        }

        private void InitializeNPC()
        {
            Debug.Log($"NPC Initialized: {npcName} ({role})");
            
            // Set initial behavior based on role
            switch (role)
            {
                case NPCRole.Merchant:
                    canTrade = true;
                    hasQuests = false;
                    walletBalance = 5000f;
                    break;
                
                case NPCRole.QuestGiver:
                    canTrade = false;
                    hasQuests = true;
                    break;
                
                case NPCRole.Banker:
                    canTrade = true;
                    hasQuests = true;
                    walletBalance = 100000f;
                    break;
                
                case NPCRole.Educator:
                    canTrade = false;
                    hasQuests = true;
                    break;
                
                case NPCRole.FashionDesigner:
                    canTrade = true;
                    hasQuests = true;
                    walletBalance = 10000f;
                    break;
                
                case NPCRole.InteriorDesigner:
                    canTrade = true;
                    hasQuests = true;
                    walletBalance = 15000f;
                    break;
                
                case NPCRole.Architect:
                    canTrade = true;
                    hasQuests = true;
                    walletBalance = 50000f;
                    break;
            }
        }

        private void Update()
        {
            if (!isInteracting)
            {
                // Basic idle behavior
                PerformIdleBehavior();
            }
        }

        /// <summary>
        /// Perform idle NPC behavior
        /// </summary>
        private void PerformIdleBehavior()
        {
            // TODO: Implement pathfinding and idle animations
            // For now, just occasional random movement or rotation
            if (Time.frameCount % 300 == 0) // Every 5 seconds at 60 FPS
            {
                // Random idle action
                float rand = Random.value;
                if (rand < 0.3f)
                {
                    // Look around
                    transform.Rotate(0, Random.Range(-45f, 45f), 0);
                }
            }
        }

        /// <summary>
        /// Start interaction with player
        /// </summary>
        public void StartInteraction(Transform player)
        {
            if (isInteracting)
                return;

            float distance = Vector3.Distance(transform.position, player.position);
            
            if (distance > interactionRange)
            {
                Debug.Log($"{npcName}: You're too far away!");
                return;
            }

            currentPlayer = player;
            isInteracting = true;

            // Face the player
            Vector3 direction = (player.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            string greeting = GetGreeting();
            Debug.Log($"{npcName}: {greeting}");
            
            conversationHistory.Add($"[GREETING] {greeting}");
        }

        /// <summary>
        /// End interaction with player
        /// </summary>
        public void EndInteraction()
        {
            isInteracting = false;
            currentPlayer = null;
            
            Debug.Log($"{npcName}: Goodbye!");
            conversationHistory.Add("[GOODBYE]");
        }

        /// <summary>
        /// Generate contextual greeting
        /// </summary>
        private string GetGreeting()
        {
            string playerAddress = WalletConnect.Instance?.connectedAddress ?? "stranger";
            int relationship = GetRelationship(playerAddress);

            if (relationship >= 50)
                return "Great to see you again, friend!";
            else if (relationship >= 20)
                return "Hello! Good to see you.";
            else if (relationship < 0)
                return "What do you want?";
            else
                return "Hello there! How can I help you?";
        }

        /// <summary>
        /// Get relationship level with player
        /// </summary>
        private int GetRelationship(string playerAddress)
        {
            if (!playerRelationships.ContainsKey(playerAddress))
            {
                playerRelationships[playerAddress] = 0;
            }
            
            return playerRelationships[playerAddress];
        }

        /// <summary>
        /// Update relationship with player
        /// </summary>
        public void UpdateRelationship(string playerAddress, int change)
        {
            int current = GetRelationship(playerAddress);
            playerRelationships[playerAddress] = Mathf.Clamp(current + change, -100, 100);
            
            Debug.Log($"Relationship with {playerAddress}: {playerRelationships[playerAddress]}");
        }

        /// <summary>
        /// Process player dialogue input (AI-powered)
        /// </summary>
        public string ProcessDialogue(string playerInput)
        {
            conversationHistory.Add($"[PLAYER] {playerInput}");

            // TODO: Integrate with GPT API for dynamic responses
            // For now, use rule-based responses
            
            string response = GenerateResponse(playerInput);
            conversationHistory.Add($"[NPC] {response}");

            return response;
        }

        /// <summary>
        /// Generate response based on input (simplified)
        /// </summary>
        private string GenerateResponse(string input)
        {
            input = input.ToLower();

            // Quest-related
            if (input.Contains("quest") || input.Contains("mission"))
            {
                if (hasQuests)
                    return "I have a task that might interest you. Are you up for a challenge?";
                else
                    return "I don't have any quests right now, sorry!";
            }

            // Trade-related
            if (input.Contains("buy") || input.Contains("sell") || input.Contains("trade"))
            {
                if (canTrade)
                    return "I'm always interested in good deals. What do you have?";
                else
                    return "I'm not a merchant, try the marketplace!";
            }

            // Information about city
            if (input.Contains("city") || input.Contains("location"))
            {
                string city = Core.GameManager.Instance?.currentCity ?? "OmniLanta";
                return $"Welcome to {city}! It's a great place with lots of opportunities.";
            }

            // Economy information
            if (input.Contains("economy") || input.Contains("money") || input.Contains("omni"))
            {
                return "The Dominion Economy keeps everything balanced. Circulation is key to success here!";
            }

            // Default response
            return "That's interesting. Tell me more!";
        }

        /// <summary>
        /// Offer a quest to the player
        /// </summary>
        public Quest OfferQuest()
        {
            if (!hasQuests)
                return null;

            // Generate a simple quest
            Quest quest = new Quest
            {
                id = Random.Range(1000, 9999),
                title = GetRandomQuestTitle(),
                description = "Help out the community and earn rewards!",
                reward = Random.Range(10f, 100f),
                experienceReward = Random.Range(50, 200),
                questType = (QuestType)Random.Range(0, 4)
            };

            Debug.Log($"Quest offered: {quest.title} - Reward: {quest.reward} $OMNI");

            return quest;
        }

        /// <summary>
        /// Get random quest title based on role
        /// </summary>
        private string GetRandomQuestTitle()
        {
            string[] questTitles = role switch
            {
                NPCRole.Merchant => new[] { "Delivery Run", "Find Rare Items", "Market Survey" },
                NPCRole.QuestGiver => new[] { "Community Service", "Exploration Mission", "Help a Neighbor" },
                NPCRole.Banker => new[] { "Execute Flash Loan", "Arbitrage Opportunity", "DeFi Yield Farming" },
                NPCRole.Educator => new[] { "Learn the Basics", "Economic Tutorial", "Property Investment Guide" },
                NPCRole.FashionDesigner => new[] { "Design Fashion Collection", "Source Fabric Materials", "Organize Fashion Show" },
                NPCRole.InteriorDesigner => new[] { "Design Room Layout", "Source Furniture", "Complete Interior Makeover" },
                NPCRole.Architect => new[] { "Create Building Blueprint", "Design Dream Home", "Plan City Structure" },
                _ => new[] { "Daily Task", "Help Needed", "Community Quest" }
            };

            return questTitles[Random.Range(0, questTitles.Length)];
        }

        /// <summary>
        /// Make economic decision (AI-driven)
        /// </summary>
        public void MakeEconomicDecision()
        {
            // NPCs participate in the economy
            float rand = Random.value;

            if (rand < 0.2f && walletBalance > 100f)
            {
                // Purchase property or items
                float purchaseAmount = Random.Range(50f, walletBalance * 0.5f);
                walletBalance -= purchaseAmount;
                Debug.Log($"{npcName} purchased something for {purchaseAmount} $OMNI");
            }
            else if (rand < 0.4f && ownedAssets.Count > 0)
            {
                // Sell an asset
                float saleAmount = Random.Range(100f, 500f);
                walletBalance += saleAmount;
                Debug.Log($"{npcName} sold an asset for {saleAmount} $OMNI");
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }

    [System.Serializable]
    public class Quest
    {
        public int id;
        public string title;
        public string description;
        public float reward;
        public int experienceReward;
        public QuestType questType;
        public bool isCompleted = false;
    }

    public enum NPCRole
    {
        Citizen,
        Merchant,
        QuestGiver,
        Banker,
        Educator,
        Security,
        Entertainer,
        FashionDesigner,
        InteriorDesigner,
        Architect
    }

    public enum QuestType
    {
        Delivery,
        Collection,
        Social,
        Economic
    }
}
