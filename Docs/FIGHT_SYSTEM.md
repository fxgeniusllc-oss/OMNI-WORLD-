# 🥊 Fight System Documentation

## Overview

The **OmniWorld Fight System** is a comprehensive combat mechanics framework designed to power underground gyms, street fighting arenas, and competitive combat across all seven metropolises. Built with performance optimization and economic integration in mind, it delivers realistic, engaging combat experiences.

## Table of Contents

- [Core Components](#core-components)
- [Fight Mechanics](#fight-mechanics)
- [Combat Stats & Progression](#combat-stats--progression)
- [Gym Integration](#gym-integration)
- [Economic System](#economic-system)
- [API Reference](#api-reference)
- [Usage Examples](#usage-examples)

---

## Core Components

### 1. FightSystem.cs

The central combat engine managing all fight interactions.

**Key Features:**
- ✅ Health and stamina management
- ✅ Damage calculation with critical hits
- ✅ Combo system with multipliers
- ✅ Multiple fight types (Boxing, MMA, Street Fight)
- ✅ Block and dodge mechanics
- ✅ Experience and progression tracking
- ✅ Event system for UI integration
- ✅ DominionEconomy integration for rewards

**Fighter Stats:**
```csharp
public class FighterStats
{
    public float health;          // Current health (0-100)
    public float maxHealth;       // Maximum health
    public float stamina;         // Current stamina (0-100)
    public float maxStamina;      // Maximum stamina
    public int level;             // Fighter level
    public int experience;        // Total XP earned
    public float strength;        // Damage multiplier (0-100)
    public float speed;           // Hit chance & dodge (0-100)
    public float defense;         // Damage reduction (0-100)
    public float technique;       // Accuracy & critical (0-100)
}
```

### 2. UndergroundGymManager.cs

Manages gym operations, memberships, and training.

**Three Gym Variations:**

#### 🥊 Boxing Gym - "Iron Fist"
- **Focus:** Speed, technique, traditional boxing
- **Stat Bonuses:** +20 Technique, +15 Speed, +10 Defense
- **Cost:** 10% cheaper than base
- **Equipment:** Boxing ring, heavy bags, speed bags, mitts

#### 🛡️ MMA Training Center - "Omega Fight Lab"
- **Focus:** Complete martial arts training
- **Stat Bonuses:** +12 All Stats (balanced)
- **Cost:** 10% more expensive than base
- **Equipment:** Octagon cage, grappling mats, comprehensive training

#### ⚔️ Street Fight Arena - "The Pit"
- **Focus:** Raw survival, no-rules combat
- **Stat Bonuses:** +20 Strength, +8 Defense, +5 Technique
- **Cost:** 20% cheaper, higher risk
- **Equipment:** Fighting pit, improvised weapons, brutal training

### 3. CombatController.cs

Player-facing combat input handler with optimized animation integration.

**Features:**
- Keyboard-based combat controls
- Input buffering system (0.2s buffer)
- Target management
- Animation integration
- Health/stamina UI helpers

---

## Fight Mechanics

### Combat Flow

```
1. Register Fighters
   ↓
2. Start Fight
   ↓
3. Combat Loop:
   - Input Processing
   - Attack Execution
   - Damage Calculation
   - Health/Stamina Update
   - Combo Tracking
   ↓
4. Victory Condition:
   - Knockout (health = 0)
   - Submission (future)
   - Decision (future)
   ↓
5. Rewards Distribution
```

### Damage Calculation

```csharp
Base Damage = Fighter Strength × Move Multiplier × Attack Power

Move Multipliers:
- Punch: 0.8x
- Kick: 1.2x
- Grapple: 1.5x
- Special: 2.0x

Modifiers:
- Critical Hit: 2.0x damage (15% chance)
- Combo: +10% per hit (max 2.5x)
- Block: -50% damage
- Defender Defense Stat: Reduces hit chance
```

### Hit Chance System

```csharp
Base Hit Chance: 80%

Attacker Bonuses:
+ (Technique / 100) × 10%
+ (Speed / 100) × 5%

Defender Penalties:
- (Speed / 100) × 5%
- (Defense / 100) × 5%

Final Hit Roll: Random(0-1) < Hit Chance
```

### Stamina System

**Costs:**
- Punch: 5 stamina
- Kick: 8 stamina
- Grapple: 12 stamina
- Special: 25 stamina
- Dodge Attempt: 15 stamina
- Block: 0 stamina (passive)

**Regeneration:**
- 10 stamina per second (configurable)
- Slower during active combat
- Faster when idle

### Combo System

```
Combo Window: 1.5 seconds between hits

Combo Damage Scaling:
Hit 1: 1.0x
Hit 2: 1.1x
Hit 3: 1.2x
Hit 4: 1.3x
...
Max: 2.5x at 15+ hits

Combo Break Conditions:
- 1.5s timeout
- Attacker hit
- Knockdown
```

---

## Combat Stats & Progression

### Experience Gains

```
Per Hit Landed: 5 XP
Per Knockout: 100 XP
Per Fight Won: 250 XP
```

### Level Progression

```javascript
// Level calculation (example)
Level = floor(√(Total XP / 100))

Level Milestones:
Level 5: Unlock Special Moves
Level 10: Unlock Advanced Training
Level 20: Unlock Elite Training
Level 50: Legendary Fighter Status
```

### Training Benefits

**Basic Training** (10 OMNI)
- Duration: 30 min
- Gains: +1 all stats
- XP: 25

**Advanced Training** (25 OMNI)
- Duration: 30 min
- Gains: +2 all stats
- XP: 50

**Elite Training** (50 OMNI)
- Duration: 30 min
- Gains: +3 all stats
- XP: 100

**Private Lesson** (100 OMNI)
- Duration: 30 min
- Gains: +4 all stats
- XP: 200

**Gym-Specific Bonuses Applied:**
Each gym type adds 10% of their stat bonuses to training gains.

---

## Gym Integration

### Membership System

```csharp
public enum MembershipType
{
    Daily,      // 5 OMNI - 1 day access
    Weekly,     // 25 OMNI - 7 days
    Monthly,    // 75 OMNI - 30 days
    Lifetime    // 500 OMNI - permanent
}
```

### Gym Economics

**Revenue Streams:**
1. Memberships
2. Training sessions
3. Fight entry fees (20 OMNI)
4. Spectator fees (5 OMNI)
5. Betting commission (15% house cut)

**Operating Costs:**
- Boxing Gym: 8,000 OMNI/month
- MMA Center: 18,000 OMNI/month
- Street Arena: 5,000 OMNI/month

**Projected Revenue:**
- Boxing: 12k-25k OMNI/month
- MMA: 25k-60k OMNI/month
- Street: 15k-60k OMNI/month (variable, betting-driven)

### Fight Organization

```csharp
// Schedule a fight
FightEvent fight = UndergroundGymManager.Instance.ScheduleFight(
    fighter1Id: "player_123",
    fighter2Id: "npc_456",
    fightType: "MMA",
    scheduledTime: DateTime.Now.AddHours(2),
    prizePurse: 500f
);

// Start the scheduled fight
FightSession session = FightSystem.Instance.StartFight(
    fighter1Id,
    fighter2Id,
    fightType,
    gymId: "omega_fight_lab"
);
```

---

## Economic System

### Fight Rewards

```
Base Win Reward: 50 OMNI

Reward Distribution:
- Winner: 85% (42.5 OMNI)
- Gym: 15% (7.5 OMNI)

Bonus Multipliers:
- Perfect Victory (no damage): 1.5x
- Knockout Victory: 1.2x
- Combo King (10+ combo): 1.1x
```

### Betting System (Street Arena)

```
Minimum Bet: 10 OMNI
Maximum Bet: 1,000 OMNI (5,000 for VIP)

House Edge: 15%

Payout Formula:
Winner Payout = (Total Bet Pool × 0.85) × (Your Bet / Winner Total Bets)
```

### NFT Integration

**Ownable Assets:**
- Gym Properties (250k-500k OMNI)
- Signature Equipment (80-15,000 OMNI)
- Fight Records NFT
- Championship Belts

**Royalties:**
- Primary Sale: 85% to creator, 15% to treasury
- Secondary Sale: 20% to original owner

---

## API Reference

### FightSystem Methods

```csharp
// Fighter Management
Fighter RegisterFighter(string fighterId, string fighterName, FighterStats stats = null)
Fighter GetFighter(string fighterId)

// Fight Control
FightSession StartFight(string fighter1Id, string fighter2Id, FightType fightType, string gymId = "")
AttackResult ExecuteAttack(string attackerId, string targetId, MoveType moveType, float attackPower = 1.0f)
void SetFighterBlocking(string fighterId, bool isBlocking)
bool AttemptDodge(string fighterId)

// Session Management
FightSession GetSession(string sessionId)
```

### UndergroundGymManager Methods

```csharp
// Membership
bool PurchaseMembership(string playerId, MembershipType membershipType)
bool EnterGym(string playerId)
void ExitGym(string playerId)

// Training
TrainingSession StartTraining(string playerId, TrainingType trainingType, float durationMinutes = 30f)
TrainingResult CompleteTraining(string sessionId)

// Events
FightEvent ScheduleFight(string fighter1Id, string fighter2Id, string fightType, DateTime scheduledTime, float prizePurse = 0f)

// Queries
GymConfiguration GetCurrentGymConfig()
List<FightEvent> GetScheduledFights()
int GetCurrentOccupancy()
```

### CombatController Methods

```csharp
// Target Management
void SetTarget(string targetId)
void ClearTarget()

// Combat State
void SetInputEnabled(bool enabled)
float GetHealthPercentage()
float GetStaminaPercentage()
int GetComboCount()
bool IsInCombat()
bool IsBlocking()

// Stats
FighterStats GetStats()
```

### Events

```csharp
// FightSystem Events
event Action<Fighter, Fighter, float> OnDamageDealt;
event Action<Fighter, int> OnComboIncreased;
event Action<Fighter> OnFighterKnockedOut;
event Action<Fighter, Fighter> OnFightStarted;
event Action<Fighter, Fighter, FightResult> OnFightEnded;

// UndergroundGymManager Events
event Action<string, MembershipType> OnMembershipPurchased;
event Action<string, TrainingSession> OnTrainingStarted;
event Action<string, TrainingSession> OnTrainingCompleted;
event Action<string> OnGymEntered;
event Action<string> OnGymExited;
event Action<FightEvent> OnFightScheduled;
```

---

## Usage Examples

### Example 1: Start a Boxing Match

```csharp
// Register fighters
Fighter player = FightSystem.Instance.RegisterFighter("player_1", "Rocky");
Fighter opponent = FightSystem.Instance.RegisterFighter("npc_1", "Apollo");

// Get boxing fight type
FightType boxing = FightSystem.Instance.availableFightTypes[0]; // Boxing

// Start fight
FightSession session = FightSystem.Instance.StartFight(
    "player_1",
    "npc_1",
    boxing,
    "iron_fist_gym"
);

// Execute attacks
AttackResult result = FightSystem.Instance.ExecuteAttack(
    "player_1",
    "npc_1",
    MoveType.Punch,
    1.5f // Heavy punch
);

if (result.success)
{
    Debug.Log($"Hit! Damage: {result.damage}, Critical: {result.isCritical}");
}
```

### Example 2: Join Gym and Train

```csharp
// Purchase membership
bool success = UndergroundGymManager.Instance.PurchaseMembership(
    "player_1",
    MembershipType.Monthly
);

// Enter gym
if (UndergroundGymManager.Instance.EnterGym("player_1"))
{
    // Start training
    TrainingSession session = UndergroundGymManager.Instance.StartTraining(
        "player_1",
        TrainingType.Advanced,
        60f // 60 minutes
    );
    
    // Simulate training time...
    yield return new WaitForSeconds(5f); // Demo: 5 seconds = 60 min
    
    // Complete training
    TrainingResult result = UndergroundGymManager.Instance.CompleteTraining(session.sessionId);
    Debug.Log($"Training complete! Gains: +{result.statGains.strength} STR, XP: +{result.experienceGained}");
}
```

### Example 3: Setup Combat Controller

```csharp
// Attach to player GameObject
CombatController controller = player.AddComponent<CombatController>();
controller.fighterName = "Rocky";
controller.animator = player.GetComponent<Animator>();

// Set target
controller.SetTarget("npc_1");

// Enable combat
controller.SetInputEnabled(true);

// Subscribe to UI updates
void Update()
{
    healthBar.fillAmount = controller.GetHealthPercentage();
    staminaBar.fillAmount = controller.GetStaminaPercentage();
    comboText.text = $"Combo: {controller.GetComboCount()}x";
}
```

### Example 4: Organize Fight Event

```csharp
// Schedule fight at MMA gym
FightEvent fightEvent = UndergroundGymManager.Instance.ScheduleFight(
    fighter1Id: "player_1",
    fighter2Id: "champion_npc",
    fightType: "MMA",
    scheduledTime: DateTime.Now.AddDays(1),
    prizePurse: 1000f
);

// Add spectators
fightEvent.spectators.Add("spectator_1");
fightEvent.spectators.Add("spectator_2");

// Start fight when time comes
FightSystem.Instance.StartFight(
    fightEvent.fighter1Id,
    fightEvent.fighter2Id,
    fightEvent.fightType,
    fightEvent.gymId
);
```

---

## Performance Optimization

### Implemented Optimizations

1. **Cached Animator Parameters:** String hashes cached for instant lookups
2. **Dictionary Lookups:** O(1) fighter/session access
3. **Event System:** Decoupled architecture for UI updates
4. **Efficient Updates:** Only update active fighters in Update()
5. **Input Buffering:** 0.2s buffer reduces missed inputs

### Recommended Settings

```csharp
// For optimal performance with 50+ fighters:
FightSystem.Instance.staminaRegenRate = 10f;
FightSystem.Instance.healthRegenRate = 2f;
FightSystem.Instance.comboWindow = 1.5f;

// Disable features for mobile:
FightSystem.Instance.enableComboSystem = true; // Keep enabled
CombatController.inputBufferTime = 0.15f; // Reduce slightly
```

---

## Integration with OmniWorld Systems

### DominionEconomy Integration

```csharp
// Check in EndFight method
if (Economy.DominionEconomy.Instance != null)
{
    // Transfer OMNI tokens
    DominionEconomy.Instance.TransferTokens(
        from: "gym_treasury",
        to: winner.id,
        amount: winnerReward
    );
}
```

### ZoneController Integration

```csharp
// Register gym with zone
ZoneController.Instance.RegisterBuilding(new BuildingData
{
    type = "Recreation",
    subType = "Underground Gym",
    owner = ownerId,
    location = gymLocation,
    value = purchasePrice
});
```

### NFT Marketplace Integration

All gym properties and equipment can be minted as NFTs with:
- Ownership tracking
- Rental capabilities
- Revenue sharing
- Upgrade systems

---

## Future Enhancements

### Planned Features

1. **Advanced Combat:**
   - Ground fighting system
   - Submission mechanics
   - Weapon combat (street arena)
   - Team battles (2v2, 3v3)

2. **Career Mode:**
   - Fighter progression storyline
   - Championship belts
   - Rival system
   - Training camps

3. **Social Features:**
   - Spectator mode
   - Live streaming
   - Chat integration
   - Betting interface

4. **AI Opponents:**
   - Difficulty levels
   - Fighting styles
   - Adaptive AI
   - Boss fighters

5. **Mobile Support:**
   - Touch controls
   - Simplified UI
   - Performance optimization

---

## Troubleshooting

### Common Issues

**Q: Fighter not taking damage**
- Check if fighter is registered in FightSystem
- Verify target ID is correct
- Ensure move type is allowed in fight type

**Q: Combo not counting**
- Check comboWindow setting (default 1.5s)
- Verify enableComboSystem is true
- Ensure attacks are hitting (not missing)

**Q: Membership not working**
- Verify payment processed through DominionEconomy
- Check membership expiry date
- Ensure gym capacity not exceeded

**Q: Animation not playing**
- Verify Animator is assigned in CombatController
- Check animator parameters match hash names
- Ensure animation clips exist in Animator

---

## Credits & License

**Developed by:** OmniWorld Dev Team  
**Version:** 1.0.0  
**License:** Proprietary (Phase 1)  

Part of the OmniWorld metaverse ecosystem.

---

## Support

For technical support, bug reports, or feature requests:
- Discord: https://discord.gg/omniworld
- Email: dev@omniworld.io
- GitHub Issues: https://github.com/fxgeniusllc-oss/OMNI-WORLD-/issues

---

**Fight hard. Train harder. Legend awaits in the underground.**
