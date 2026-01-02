using UnityEngine;
using System.Collections;

namespace OmniWorld.Combat
{
    /// <summary>
    /// Combat Controller - Handles player combat input and interactions
    /// Connects player input to FightSystem and animation systems
    /// Optimized for responsive gameplay
    /// </summary>
    public class CombatController : MonoBehaviour
    {
        [Header("Fighter Configuration")]
        public string fighterId;
        public string fighterName = "Player";
        
        [Header("Combat Settings")]
        [Tooltip("Enable input processing")]
        public bool enableInput = true;
        
        [Tooltip("Input buffer time in seconds")]
        public float inputBufferTime = 0.2f;
        
        [Tooltip("Animation transition speed")]
        public float animationTransitionSpeed = 0.1f;

        [Header("Key Bindings")]
        public KeyCode lightPunchKey = KeyCode.Q;
        public KeyCode heavyPunchKey = KeyCode.E;
        public KeyCode lightKickKey = KeyCode.Z;
        public KeyCode heavyKickKey = KeyCode.C;
        public KeyCode blockKey = KeyCode.LeftShift;
        public KeyCode dodgeKey = KeyCode.Space;
        public KeyCode specialAttackKey = KeyCode.R;

        [Header("References")]
        public Animator animator;
        public Transform targetIndicator;

        private Fighter fighter;
        private string currentTargetId;
        private bool isBlocking;
        private float lastInputTime;
        private MoveType bufferedMove;

        // Animation parameter hashes (optimized)
        private int animIsBlocking;
        private int animPunch;
        private int animKick;
        private int animSpecial;
        private int animHit;
        private int animDodge;

        private void Awake()
        {
            // Generate unique fighter ID if not set
            if (string.IsNullOrEmpty(fighterId))
            {
                fighterId = $"player_{GetInstanceID()}";
            }

            // Cache animator parameter hashes for performance
            if (animator != null)
            {
                animIsBlocking = Animator.StringToHash("IsBlocking");
                animPunch = Animator.StringToHash("Punch");
                animKick = Animator.StringToHash("Kick");
                animSpecial = Animator.StringToHash("Special");
                animHit = Animator.StringToHash("Hit");
                animDodge = Animator.StringToHash("Dodge");
            }
        }

        private void Start()
        {
            // Register fighter with FightSystem
            if (FightSystem.Instance != null)
            {
                fighter = FightSystem.Instance.RegisterFighter(fighterId, fighterName);
                Debug.Log($"Combat Controller initialized for {fighterName}");
            }
            else
            {
                Debug.LogError("FightSystem not found!");
            }

            // Subscribe to events
            if (FightSystem.Instance != null)
            {
                FightSystem.Instance.OnDamageDealt += OnDamageDealt;
                FightSystem.Instance.OnFighterKnockedOut += OnFighterKnockedOut;
            }
        }

        private void Update()
        {
            if (!enableInput || fighter == null)
                return;

            // Process combat input
            ProcessCombatInput();

            // Update animation states
            UpdateAnimations();

            // Process buffered input
            ProcessInputBuffer();
        }

        private void ProcessCombatInput()
        {
            // Block (hold)
            if (Input.GetKey(blockKey))
            {
                if (!isBlocking)
                {
                    StartBlocking();
                }
            }
            else if (isBlocking)
            {
                StopBlocking();
            }

            // Don't allow attacks while blocking
            if (isBlocking)
                return;

            // Light Punch
            if (Input.GetKeyDown(lightPunchKey))
            {
                ExecuteMove(MoveType.Punch, 1.0f);
            }

            // Heavy Punch
            if (Input.GetKeyDown(heavyPunchKey))
            {
                ExecuteMove(MoveType.Punch, 1.5f);
            }

            // Light Kick
            if (Input.GetKeyDown(lightKickKey))
            {
                ExecuteMove(MoveType.Kick, 1.0f);
            }

            // Heavy Kick
            if (Input.GetKeyDown(heavyKickKey))
            {
                ExecuteMove(MoveType.Kick, 1.5f);
            }

            // Dodge
            if (Input.GetKeyDown(dodgeKey))
            {
                ExecuteDodge();
            }

            // Special Attack
            if (Input.GetKeyDown(specialAttackKey))
            {
                ExecuteMove(MoveType.Special, 2.0f);
            }
        }

        private void ExecuteMove(MoveType moveType, float power)
        {
            if (string.IsNullOrEmpty(currentTargetId))
            {
                Debug.LogWarning("No target selected");
                BufferInput(moveType);
                return;
            }

            if (FightSystem.Instance != null)
            {
                AttackResult result = FightSystem.Instance.ExecuteAttack(fighterId, currentTargetId, moveType, power);
                
                if (result != null && result.success)
                {
                    // Trigger attack animation
                    TriggerAttackAnimation(moveType);
                    
                    // Visual feedback
                    OnAttackSuccess(result);
                }
                else
                {
                    // Trigger miss animation or feedback
                    OnAttackFailed(result?.reason ?? "Unknown");
                }
            }
        }

        private void ExecuteDodge()
        {
            if (FightSystem.Instance != null)
            {
                bool success = FightSystem.Instance.AttemptDodge(fighterId);
                
                if (success && animator != null)
                {
                    animator.SetTrigger(animDodge);
                }
            }
        }

        private void StartBlocking()
        {
            isBlocking = true;
            
            if (FightSystem.Instance != null)
            {
                FightSystem.Instance.SetFighterBlocking(fighterId, true);
            }

            if (animator != null)
            {
                animator.SetBool(animIsBlocking, true);
            }

            Debug.Log($"{fighterName} is blocking");
        }

        private void StopBlocking()
        {
            isBlocking = false;
            
            if (FightSystem.Instance != null)
            {
                FightSystem.Instance.SetFighterBlocking(fighterId, false);
            }

            if (animator != null)
            {
                animator.SetBool(animIsBlocking, false);
            }
        }

        private void TriggerAttackAnimation(MoveType moveType)
        {
            if (animator == null)
                return;

            switch (moveType)
            {
                case MoveType.Punch:
                    animator.SetTrigger(animPunch);
                    break;
                case MoveType.Kick:
                    animator.SetTrigger(animKick);
                    break;
                case MoveType.Special:
                    animator.SetTrigger(animSpecial);
                    break;
            }
        }

        private void UpdateAnimations()
        {
            if (animator == null || fighter == null)
                return;

            // Update animator parameters based on fighter state
            // Add custom animation logic here
        }

        private void BufferInput(MoveType moveType)
        {
            bufferedMove = moveType;
            lastInputTime = Time.time;
        }

        private void ProcessInputBuffer()
        {
            if (bufferedMove == MoveType.None)
                return;

            // Check if buffer has expired
            if (Time.time - lastInputTime > inputBufferTime)
            {
                bufferedMove = MoveType.None;
                return;
            }

            // Try to execute buffered move if we now have a target
            if (!string.IsNullOrEmpty(currentTargetId))
            {
                ExecuteMove(bufferedMove, 1.0f);
                bufferedMove = MoveType.None;
            }
        }

        private void OnAttackSuccess(AttackResult result)
        {
            // Visual feedback for successful attack
            if (result.isCritical)
            {
                Debug.Log($"CRITICAL HIT! {result.damage} damage!");
                // Add screen shake, particle effects, etc.
            }

            if (result.comboMultiplier > 1.0f)
            {
                Debug.Log($"COMBO x{fighter.comboCount}!");
                // Add combo UI feedback
            }
        }

        private void OnAttackFailed(string reason)
        {
            Debug.Log($"Attack failed: {reason}");
            // Add miss animation or feedback
        }

        private void OnDamageDealt(Fighter attacker, Fighter target, float damage)
        {
            // Handle when this fighter deals or receives damage
            if (attacker.id == fighterId)
            {
                // We dealt damage
                Debug.Log($"Dealt {damage:F1} damage to {target.name}");
            }
            else if (target.id == fighterId)
            {
                // We received damage
                Debug.Log($"Received {damage:F1} damage from {attacker.name}");
                
                if (animator != null && !isBlocking)
                {
                    animator.SetTrigger(animHit);
                }

                // Add screen effect, camera shake, etc.
                OnTakeDamage(damage);
            }
        }

        private void OnFighterKnockedOut(Fighter knockedOutFighter)
        {
            if (knockedOutFighter.id == fighterId)
            {
                Debug.Log($"{fighterName} has been knocked out!");
                enableInput = false;
                
                // Play knockout animation
                StartCoroutine(HandleKnockout());
            }
        }

        private IEnumerator HandleKnockout()
        {
            // Play knockout animation
            yield return new WaitForSeconds(3f);
            
            // Reset or show game over screen
            Debug.Log("Knockout sequence complete");
        }

        private void OnTakeDamage(float damage)
        {
            // Add visual effects for taking damage
            // - Screen flash
            // - Camera shake
            // - Health bar update
            // - Damage numbers
        }

        /// <summary>
        /// Set combat target
        /// </summary>
        public void SetTarget(string targetId)
        {
            currentTargetId = targetId;
            Debug.Log($"Target set to: {targetId}");

            // Update target indicator if available
            if (targetIndicator != null)
            {
                // Position indicator above target
            }
        }

        /// <summary>
        /// Clear combat target
        /// </summary>
        public void ClearTarget()
        {
            currentTargetId = null;
            
            if (targetIndicator != null)
            {
                targetIndicator.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Get current fighter stats
        /// </summary>
        public FighterStats GetStats()
        {
            return fighter?.stats;
        }

        /// <summary>
        /// Enable/disable combat input
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            enableInput = enabled;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (FightSystem.Instance != null)
            {
                FightSystem.Instance.OnDamageDealt -= OnDamageDealt;
                FightSystem.Instance.OnFighterKnockedOut -= OnFighterKnockedOut;
            }
        }

        // Public methods for UI and external systems
        public float GetHealthPercentage()
        {
            if (fighter == null || fighter.stats == null)
                return 0f;
            
            return fighter.stats.health / fighter.stats.maxHealth;
        }

        public float GetStaminaPercentage()
        {
            if (fighter == null || fighter.stats == null)
                return 0f;
            
            return fighter.stats.stamina / fighter.stats.maxStamina;
        }

        public int GetComboCount()
        {
            return fighter?.comboCount ?? 0;
        }

        public bool IsInCombat()
        {
            return fighter != null && fighter.state == FighterState.Fighting;
        }

        public bool IsBlocking()
        {
            return isBlocking;
        }
    }
}
