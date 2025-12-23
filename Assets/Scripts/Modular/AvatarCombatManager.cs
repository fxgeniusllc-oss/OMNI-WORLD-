using UnityEngine;

namespace OmniWorld.Combat
{
    /// <summary>
    /// Manages avatar combat mechanics including melee, ranged, and special abilities
    /// Integrates with GymTrainingSystem for stat progression
    /// </summary>
    public class AvatarCombatManager : MonoBehaviour
    {
        private static AvatarCombatManager _instance;
        public static AvatarCombatManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AvatarCombatManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AvatarCombatManager");
                        _instance = go.AddComponent<AvatarCombatManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Combat Stats")]
        public float health = 100f;
        public float stamina = 100f;
        public float damage = 10f;
        public float defense = 5f;

        [Header("Combat Configuration")]
        public bool combatEnabled = true;
        public float healthRegenRate = 1f;
        public float staminaRegenRate = 2f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("AvatarCombatManager initialized - Combat system ready");
        }

        public void InitiateCombat(string targetId)
        {
            Debug.Log($"Combat initiated with target: {targetId}");
            // TODO: Implement combat logic
        }

        public void TrainCombatStats(string statType, float amount)
        {
            Debug.Log($"Training {statType} by {amount}");
            // TODO: Integrate with GymTrainingSystem
        }
    }
}
