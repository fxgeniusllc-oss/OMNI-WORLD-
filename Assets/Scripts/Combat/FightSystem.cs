using UnityEngine;
using System;
using System.Collections.Generic;

namespace OmniWorld.Combat
{
    /// <summary>
    /// Core fight system for OmniWorld - handles combat mechanics, health, stamina, and damage
    /// Optimized for performance with pooling and efficient calculations
    /// Integrates with DominionEconomy for fight rewards and betting
    /// </summary>
    public class FightSystem : MonoBehaviour
    {
        private static FightSystem _instance;
        public static FightSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<FightSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("FightSystem");
                        _instance = go.AddComponent<FightSystem>();
                    }
                }
                return _instance;
            }
        }

        [Header("Fighter Stats")]
        [Tooltip("Base health for all fighters")]
        public float baseHealth = 100f;
        
        [Tooltip("Base stamina for all fighters")]
        public float baseStamina = 100f;
        
        [Tooltip("Stamina regeneration per second")]
        public float staminaRegenRate = 10f;
        
        [Tooltip("Health regeneration per second (out of combat)")]
        public float healthRegenRate = 2f;

        [Header("Combat Mechanics")]
        [Tooltip("Base damage multiplier")]
        public float baseDamageMultiplier = 1.0f;
        
        [Tooltip("Critical hit chance (0-1)")]
        public float criticalHitChance = 0.15f;
        
        [Tooltip("Critical hit damage multiplier")]
        public float criticalDamageMultiplier = 2.0f;
        
        [Tooltip("Block damage reduction (0-1)")]
        public float blockDamageReduction = 0.5f;
        
        [Tooltip("Dodge success chance when attempting (0-1)")]
        public float dodgeSuccessChance = 0.3f;

        [Header("Combo System")]
        [Tooltip("Enable combo system")]
        public bool enableComboSystem = true;
        
        [Tooltip("Time window for combo continuation (seconds)")]
        public float comboWindow = 1.5f;
        
        [Tooltip("Combo damage multiplier per hit")]
        public float comboDamageIncrement = 0.1f;
        
        [Tooltip("Maximum combo multiplier")]
        public float maxComboMultiplier = 2.5f;

        [Header("Experience & Progression")]
        [Tooltip("Experience gained per hit landed")]
        public int expPerHit = 5;
        
        [Tooltip("Experience gained per knockout")]
        public int expPerKnockout = 100;
        
        [Tooltip("Experience gained per fight won")]
        public int expPerWin = 250;

        [Header("Economy Integration")]
        [Tooltip("Base reward for winning a fight (in OMNI)")]
        public float baseWinReward = 50f;
        
        [Tooltip("Betting multiplier for winner")]
        public float winnerBettingMultiplier = 2.0f;
        
        [Tooltip("House cut percentage for the gym")]
        public float gymHouseCut = 0.15f; // 15% to gym

        [Header("Fight Types")]
        public FightType[] availableFightTypes = new FightType[]
        {
            new FightType { name = "Boxing", allowedMoves = MoveType.Punch | MoveType.Block | MoveType.Dodge },
            new FightType { name = "MMA", allowedMoves = MoveType.Punch | MoveType.Kick | MoveType.Grapple | MoveType.Block | MoveType.Dodge },
            new FightType { name = "Street Fight", allowedMoves = MoveType.All }
        };

        // Events
        public event Action<Fighter, Fighter, float> OnDamageDealt;
        public event Action<Fighter, int> OnComboIncreased;
        public event Action<Fighter> OnFighterKnockedOut;
        public event Action<Fighter, Fighter> OnFightStarted;
        public event Action<Fighter, Fighter, FightResult> OnFightEnded;

        private Dictionary<string, Fighter> activeFighters = new Dictionary<string, Fighter>();
        private Dictionary<string, FightSession> activeSessions = new Dictionary<string, FightSession>();
        private System.Random random;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeFightSystem();
        }

        private void InitializeFightSystem()
        {
            random = new System.Random();
            Debug.Log("Fight System Initialized - Ready for Combat!");
        }

        private void Update()
        {
            // Update all active fighters (stamina regen, health regen, combo timers)
            UpdateActiveFighters(Time.deltaTime);
        }

        /// <summary>
        /// Register a fighter in the system
        /// </summary>
        public Fighter RegisterFighter(string fighterId, string fighterName, FighterStats stats = null)
        {
            if (activeFighters.ContainsKey(fighterId))
            {
                Debug.LogWarning($"Fighter {fighterId} already registered");
                return activeFighters[fighterId];
            }

            Fighter fighter = new Fighter
            {
                id = fighterId,
                name = fighterName,
                stats = stats ?? new FighterStats
                {
                    health = baseHealth,
                    maxHealth = baseHealth,
                    stamina = baseStamina,
                    maxStamina = baseStamina,
                    level = 1,
                    experience = 0,
                    strength = 10,
                    speed = 10,
                    defense = 10,
                    technique = 10
                },
                state = FighterState.Idle,
                lastActionTime = Time.time
            };

            activeFighters[fighterId] = fighter;
            Debug.Log($"Fighter registered: {fighterName} (ID: {fighterId})");
            
            return fighter;
        }

        /// <summary>
        /// Start a fight between two fighters
        /// </summary>
        public FightSession StartFight(string fighter1Id, string fighter2Id, FightType fightType, string gymId = "")
        {
            if (!activeFighters.ContainsKey(fighter1Id) || !activeFighters.ContainsKey(fighter2Id))
            {
                Debug.LogError("One or both fighters not registered");
                return null;
            }

            Fighter fighter1 = activeFighters[fighter1Id];
            Fighter fighter2 = activeFighters[fighter2Id];

            // Reset fighters to full health/stamina for the fight
            ResetFighterForFight(fighter1);
            ResetFighterForFight(fighter2);

            string sessionId = $"{fighter1Id}_vs_{fighter2Id}_{Time.time}";
            FightSession session = new FightSession
            {
                sessionId = sessionId,
                fighter1 = fighter1,
                fighter2 = fighter2,
                fightType = fightType,
                gymId = gymId,
                startTime = Time.time,
                isActive = true
            };

            activeSessions[sessionId] = session;

            fighter1.state = FighterState.Fighting;
            fighter2.state = FighterState.Fighting;

            OnFightStarted?.Invoke(fighter1, fighter2);
            Debug.Log($"Fight started: {fighter1.name} vs {fighter2.name} ({fightType.name})");

            return session;
        }

        /// <summary>
        /// Execute an attack from one fighter to another
        /// </summary>
        public AttackResult ExecuteAttack(string attackerId, string targetId, MoveType moveType, float attackPower = 1.0f)
        {
            if (!activeFighters.ContainsKey(attackerId) || !activeFighters.ContainsKey(targetId))
            {
                Debug.LogError("Invalid fighter IDs");
                return null;
            }

            Fighter attacker = activeFighters[attackerId];
            Fighter target = activeFighters[targetId];

            // Check if move is allowed in current fight type
            FightSession session = GetFighterSession(attackerId);
            if (session != null && !IsMoveAllowed(moveType, session.fightType))
            {
                Debug.LogWarning($"Move {moveType} not allowed in {session.fightType.name}");
                return null;
            }

            // Check stamina
            float staminaCost = GetStaminaCost(moveType);
            if (attacker.stats.stamina < staminaCost)
            {
                Debug.Log($"{attacker.name} insufficient stamina for {moveType}");
                return new AttackResult { success = false, reason = "Insufficient stamina" };
            }

            // Consume stamina
            attacker.stats.stamina -= staminaCost;

            // Calculate if attack hits
            bool isHit = CalculateHitChance(attacker, target, moveType);
            if (!isHit)
            {
                Debug.Log($"{attacker.name}'s attack missed!");
                return new AttackResult { success = false, reason = "Attack missed", staminaUsed = staminaCost };
            }

            // Calculate damage
            float baseDamage = CalculateBaseDamage(attacker, moveType, attackPower);
            
            // Check for critical hit
            bool isCritical = random.NextDouble() < criticalHitChance;
            if (isCritical)
            {
                baseDamage *= criticalDamageMultiplier;
            }

            // Apply combo multiplier
            float comboMultiplier = 1.0f;
            if (enableComboSystem && attacker.comboCount > 0)
            {
                comboMultiplier = Mathf.Min(1.0f + (attacker.comboCount * comboDamageIncrement), maxComboMultiplier);
                baseDamage *= comboMultiplier;
            }

            // Check if target is blocking
            float finalDamage = baseDamage;
            if (target.state == FighterState.Blocking)
            {
                finalDamage *= (1f - blockDamageReduction);
                Debug.Log($"{target.name} blocked! Damage reduced by {blockDamageReduction * 100}%");
            }

            // Apply damage
            target.stats.health -= finalDamage;
            target.stats.health = Mathf.Max(0, target.stats.health);

            // Update combo
            attacker.comboCount++;
            attacker.lastActionTime = Time.time;
            OnComboIncreased?.Invoke(attacker, attacker.comboCount);

            // Award experience
            attacker.stats.experience += expPerHit;

            // Trigger damage event
            OnDamageDealt?.Invoke(attacker, target, finalDamage);

            AttackResult result = new AttackResult
            {
                success = true,
                damage = finalDamage,
                isCritical = isCritical,
                comboMultiplier = comboMultiplier,
                staminaUsed = staminaCost,
                targetHealth = target.stats.health
            };

            Debug.Log($"{attacker.name} hits {target.name} for {finalDamage:F1} damage! " +
                     $"(Critical: {isCritical}, Combo: {attacker.comboCount}x, Health: {target.stats.health:F1}/{target.stats.maxHealth})");

            // Check for knockout
            if (target.stats.health <= 0)
            {
                HandleKnockout(attacker, target);
            }

            return result;
        }

        /// <summary>
        /// Set fighter to blocking stance
        /// </summary>
        public void SetFighterBlocking(string fighterId, bool isBlocking)
        {
            if (activeFighters.ContainsKey(fighterId))
            {
                Fighter fighter = activeFighters[fighterId];
                fighter.state = isBlocking ? FighterState.Blocking : FighterState.Fighting;
                Debug.Log($"{fighter.name} is {(isBlocking ? "blocking" : "ready")}");
            }
        }

        /// <summary>
        /// Attempt to dodge an attack
        /// </summary>
        public bool AttemptDodge(string fighterId)
        {
            if (!activeFighters.ContainsKey(fighterId))
                return false;

            Fighter fighter = activeFighters[fighterId];
            
            float dodgeCost = 15f;
            if (fighter.stats.stamina < dodgeCost)
                return false;

            fighter.stats.stamina -= dodgeCost;
            
            bool dodgeSuccess = random.NextDouble() < dodgeSuccessChance;
            if (dodgeSuccess)
            {
                Debug.Log($"{fighter.name} successfully dodged!");
            }
            
            return dodgeSuccess;
        }

        private void HandleKnockout(Fighter winner, Fighter loser)
        {
            loser.state = FighterState.KnockedOut;
            winner.stats.experience += expPerKnockout;
            
            OnFighterKnockedOut?.Invoke(loser);
            Debug.Log($"{loser.name} has been knocked out!");

            // End the fight
            FightSession session = GetFighterSession(winner.id);
            if (session != null)
            {
                EndFight(session.sessionId, winner, loser);
            }
        }

        private void EndFight(string sessionId, Fighter winner, Fighter loser)
        {
            if (!activeSessions.ContainsKey(sessionId))
                return;

            FightSession session = activeSessions[sessionId];
            session.isActive = false;
            session.endTime = Time.time;

            winner.state = FighterState.Idle;
            loser.state = FighterState.Idle;

            // Award experience and rewards
            winner.stats.experience += expPerWin;
            
            // Calculate OMNI rewards
            float totalReward = baseWinReward;
            float gymCut = totalReward * gymHouseCut;
            float winnerReward = totalReward - gymCut;

            FightResult result = new FightResult
            {
                winner = winner,
                loser = loser,
                duration = session.endTime - session.startTime,
                totalDamageDealt = 0, // TODO: Track this
                winnerReward = winnerReward,
                gymReward = gymCut
            };

            OnFightEnded?.Invoke(winner, loser, result);
            
            // Integrate with DominionEconomy if available
            if (Economy.DominionEconomy.Instance != null)
            {
                Debug.Log($"Awarding {winnerReward} OMNI to {winner.name}");
                // TODO: Actual token transfer through DominionEconomy
            }

            Debug.Log($"Fight ended! Winner: {winner.name}, Duration: {result.duration:F1}s, Reward: {winnerReward} OMNI");

            activeSessions.Remove(sessionId);
        }

        private void UpdateActiveFighters(float deltaTime)
        {
            foreach (var fighter in activeFighters.Values)
            {
                // Stamina regeneration
                if (fighter.stats.stamina < fighter.stats.maxStamina)
                {
                    fighter.stats.stamina += staminaRegenRate * deltaTime;
                    fighter.stats.stamina = Mathf.Min(fighter.stats.stamina, fighter.stats.maxStamina);
                }

                // Health regeneration (only when not fighting)
                if (fighter.state == FighterState.Idle && fighter.stats.health < fighter.stats.maxHealth)
                {
                    fighter.stats.health += healthRegenRate * deltaTime;
                    fighter.stats.health = Mathf.Min(fighter.stats.health, fighter.stats.maxHealth);
                }

                // Combo timeout
                if (enableComboSystem && fighter.comboCount > 0)
                {
                    if (Time.time - fighter.lastActionTime > comboWindow)
                    {
                        fighter.comboCount = 0;
                    }
                }
            }
        }

        private void ResetFighterForFight(Fighter fighter)
        {
            fighter.stats.health = fighter.stats.maxHealth;
            fighter.stats.stamina = fighter.stats.maxStamina;
            fighter.comboCount = 0;
            fighter.lastActionTime = Time.time;
        }

        private float CalculateBaseDamage(Fighter attacker, MoveType moveType, float attackPower)
        {
            float damage = 0f;

            switch (moveType)
            {
                case MoveType.Punch:
                    damage = attacker.stats.strength * 0.8f * attackPower;
                    break;
                case MoveType.Kick:
                    damage = attacker.stats.strength * 1.2f * attackPower;
                    break;
                case MoveType.Grapple:
                    damage = attacker.stats.strength * 1.5f * attackPower;
                    break;
                case MoveType.Special:
                    damage = attacker.stats.strength * 2.0f * attackPower;
                    break;
            }

            return damage * baseDamageMultiplier;
        }

        private bool CalculateHitChance(Fighter attacker, Fighter target, MoveType moveType)
        {
            // Base hit chance is 80%
            float hitChance = 0.8f;
            
            // Modify by attacker technique and speed
            hitChance += (attacker.stats.technique / 100f) * 0.1f;
            hitChance += (attacker.stats.speed / 100f) * 0.05f;
            
            // Reduce by defender speed and defense
            hitChance -= (target.stats.speed / 100f) * 0.05f;
            hitChance -= (target.stats.defense / 100f) * 0.05f;

            return random.NextDouble() < hitChance;
        }

        private float GetStaminaCost(MoveType moveType)
        {
            return moveType switch
            {
                MoveType.Punch => 5f,
                MoveType.Kick => 8f,
                MoveType.Grapple => 12f,
                MoveType.Special => 25f,
                _ => 5f
            };
        }

        private bool IsMoveAllowed(MoveType move, FightType fightType)
        {
            return (fightType.allowedMoves & move) == move;
        }

        private FightSession GetFighterSession(string fighterId)
        {
            foreach (var session in activeSessions.Values)
            {
                if (session.fighter1.id == fighterId || session.fighter2.id == fighterId)
                {
                    return session;
                }
            }
            return null;
        }

        public Fighter GetFighter(string fighterId)
        {
            return activeFighters.ContainsKey(fighterId) ? activeFighters[fighterId] : null;
        }

        public FightSession GetSession(string sessionId)
        {
            return activeSessions.ContainsKey(sessionId) ? activeSessions[sessionId] : null;
        }
    }

    // Data structures
    [Serializable]
    public class Fighter
    {
        public string id;
        public string name;
        public FighterStats stats;
        public FighterState state;
        public int comboCount;
        public float lastActionTime;
    }

    [Serializable]
    public class FighterStats
    {
        public float health;
        public float maxHealth;
        public float stamina;
        public float maxStamina;
        public int level;
        public int experience;
        public float strength;
        public float speed;
        public float defense;
        public float technique;
    }

    public enum FighterState
    {
        Idle,
        Fighting,
        Blocking,
        Dodging,
        KnockedOut
    }

    [Serializable]
    public class FightType
    {
        public string name;
        public MoveType allowedMoves;
    }

    [Flags]
    public enum MoveType
    {
        None = 0,
        Punch = 1 << 0,
        Kick = 1 << 1,
        Grapple = 1 << 2,
        Block = 1 << 3,
        Dodge = 1 << 4,
        Special = 1 << 5,
        All = ~0
    }

    public class AttackResult
    {
        public bool success;
        public string reason;
        public float damage;
        public bool isCritical;
        public float comboMultiplier;
        public float staminaUsed;
        public float targetHealth;
    }

    public class FightSession
    {
        public string sessionId;
        public Fighter fighter1;
        public Fighter fighter2;
        public FightType fightType;
        public string gymId;
        public float startTime;
        public float endTime;
        public bool isActive;
    }

    public class FightResult
    {
        public Fighter winner;
        public Fighter loser;
        public float duration;
        public float totalDamageDealt;
        public float winnerReward;
        public float gymReward;
    }
}
