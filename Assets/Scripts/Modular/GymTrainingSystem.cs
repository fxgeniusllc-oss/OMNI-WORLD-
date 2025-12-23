using UnityEngine;

namespace OmniWorld.Training
{
    /// <summary>
    /// Manages gym training mechanics for stat progression
    /// Integrates with AvatarCombatManager for combat stat improvements
    /// Located in OmniSouthside underground gym
    /// </summary>
    public class GymTrainingSystem : MonoBehaviour
    {
        private static GymTrainingSystem _instance;
        public static GymTrainingSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GymTrainingSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GymTrainingSystem");
                        _instance = go.AddComponent<GymTrainingSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Training Configuration")]
        public float strengthGainPerSession = 2f;
        public float enduranceGainPerSession = 1.5f;
        public float agilityGainPerSession = 1f;
        public float trainingCost = 50f;

        [Header("Player Stats")]
        public float strength = 10f;
        public float endurance = 10f;
        public float agility = 10f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("GymTrainingSystem initialized - Training facilities ready");
        }

        public void TrainStrength(string walletAddress)
        {
            strength += strengthGainPerSession;
            Debug.Log($"Strength training completed. New strength: {strength}");
            // TODO: Process payment and update combat stats
        }

        public void TrainEndurance(string walletAddress)
        {
            endurance += enduranceGainPerSession;
            Debug.Log($"Endurance training completed. New endurance: {endurance}");
            // TODO: Process payment and update combat stats
        }

        public void TrainAgility(string walletAddress)
        {
            agility += agilityGainPerSession;
            Debug.Log($"Agility training completed. New agility: {agility}");
            // TODO: Process payment and update combat stats
        }
    }
}
