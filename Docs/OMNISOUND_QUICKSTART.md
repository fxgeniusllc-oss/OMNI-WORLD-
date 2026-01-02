# OmniSound Global Grid - Developer Quick Start

## 🚀 Quick Start Guide

This guide will help you integrate and use the OmniSound Global Grid system in OmniWorld.

---

## 📦 System Components

The system consists of 4 main components:

1. **MusicBiomeController** - Manages dynamic music and sound environments
2. **AirportManager** - Handles global travel network and terminal access
3. **CityReputationSystem** - Tracks player reputation across cities
4. **ProceduralGeneration** (Extended) - Generates music-based quests

---

## 🎮 Basic Usage

### 1. Loading a Music Biome

```csharp
// Get the music biome controller instance
MusicBiomeController musicBiome = MusicBiomeController.Instance;

// Load a city's music biome
musicBiome.LoadBiomeForCity("OmniNYC");

// Get current biome information
string info = musicBiome.GetBiomeInfo();
Debug.Log(info);
```

### 2. Using the Airport System

```csharp
// Get airport manager instance
AirportManager airportMgr = AirportManager.Instance;

// Enter an airport
airportMgr.EnterAirport("OmniLanta");

// Get available destinations
List<string> destinations = airportMgr.GetAvailableDestinations();

// Book a flight
bool success = airportMgr.BookFlight("OmniNYC", playerWalletAddress);

// Leave airport (automatically loads city music biome)
airportMgr.LeaveAirport();
```

### 3. Managing City Reputation

```csharp
// Get reputation system instance
CityReputationSystem repSystem = CityReputationSystem.Instance;

// Check current reputation
CityReputationData repData = repSystem.GetCityReputation("OmniNYC");
Debug.Log($"Reputation Level: {repData.level}");
Debug.Log($"Reputation Points: {repData.reputationPoints}");

// Add reputation points
repSystem.AddReputation("OmniNYC", 10, "Completed a quest");

// Track quest completion (automatically adds reputation)
repSystem.OnQuestCompleted("OmniNYC", "boom_bap_quest", 15);

// Increase music mastery
repSystem.IncreaseMusicMastery("OmniNYC", 5);
```

### 4. Generating Music Biome Quests

```csharp
// Get procedural generation instance
ProceduralGeneration procGen = ProceduralGeneration.Instance;

// Generate a music-based quest for a city
Quest musicQuest = procGen.GenerateMusicBiomeQuest("OmniNYC");

Debug.Log($"Quest: {musicQuest.title}");
Debug.Log($"Reward: {musicQuest.reward} $OMNI");
Debug.Log($"Description: {musicQuest.description}");
```

---

## 🎯 Common Scenarios

### Scenario 1: Player Arrives in New City

```csharp
public void OnPlayerArrivesInCity(string cityName)
{
    // 1. Load music biome
    MusicBiomeController.Instance.LoadBiomeForCity(cityName);
    
    // 2. Update reputation (track visit)
    CityReputationSystem.Instance.AddReputation(cityName, 1, "Visited city");
    
    // 3. Check if first visit
    CityReputationData rep = CityReputationSystem.Instance.GetCityReputation(cityName);
    if (rep.reputationPoints == 1)
    {
        Debug.Log($"First time in {cityName}! Welcome!");
        ShowCityIntroduction(cityName);
    }
    
    // 4. Generate city-specific missions
    for (int i = 0; i < 3; i++)
    {
        Quest quest = ProceduralGeneration.Instance.GenerateMusicBiomeQuest(cityName);
        AddQuestToBoard(quest);
    }
}
```

### Scenario 2: Player Completes Music Quest

```csharp
public void OnMusicQuestCompleted(string cityName, string questId, int rewardAmount)
{
    // 1. Award reputation
    CityReputationSystem repSystem = CityReputationSystem.Instance;
    repSystem.OnQuestCompleted(cityName, questId, 15);
    
    // 2. Increase music mastery
    repSystem.IncreaseMusicMastery(cityName, 10);
    
    // 3. Play cultural sound celebration
    MusicBiomeController.Instance.PlayCulturalSound("success_jingle");
    
    // 4. Check for level up
    CityReputationData rep = repSystem.GetCityReputation(cityName);
    Debug.Log($"New reputation level: {rep.level}");
}
```

### Scenario 3: Player Books Flight

```csharp
public void BookFlightToCity(string destinationCity)
{
    AirportManager airportMgr = AirportManager.Instance;
    CityReputationSystem repSystem = CityReputationSystem.Instance;
    
    // 1. Check if airport is unlocked
    AirportData airport = airportMgr.GetAirport(destinationCity);
    if (airport == null || !airport.isUnlocked)
    {
        ShowMessage($"{destinationCity} airport is locked!");
        ShowUnlockRequirements(airport);
        return;
    }
    
    // 2. Get player reputation
    CityReputationData rep = repSystem.GetCityReputation(destinationCity);
    int playerRep = rep.reputationPoints;
    
    // 3. Book flight
    bool success = airportMgr.BookFlight(destinationCity, playerWalletAddress);
    
    if (success)
    {
        // Flight booked successfully
        // AirportManager automatically handles:
        // - City transition
        // - Music biome loading
        // - Arrival at destination
        Debug.Log($"Flight booked to {destinationCity}!");
    }
}
```

### Scenario 4: Unlocking a New City

```csharp
public void TryUnlockCity(string cityName, string walletAddress, int playerReputation)
{
    AirportManager airportMgr = AirportManager.Instance;
    
    // Attempt to unlock
    bool success = airportMgr.UnlockAirport(cityName, walletAddress, playerReputation);
    
    if (success)
    {
        Debug.Log($"✈️ {cityName} unlocked!");
        
        // Show celebration UI
        ShowCityUnlockedUI(cityName);
        
        // Generate welcome quests
        for (int i = 0; i < 5; i++)
        {
            Quest quest = ProceduralGeneration.Instance.GenerateMusicBiomeQuest(cityName);
            AddQuestToBoard(quest);
        }
    }
    else
    {
        Debug.Log($"Cannot unlock {cityName} yet.");
    }
}
```

---

## 🔧 Configuration

### Music Biome Configuration

Edit city music biomes in `Assets/Config/MusicBiomes/`:

```json
{
  "cityName": "OmniNYC",
  "biomeName": "Boom Bap Metropolitan",
  "primaryGenre": "Hip-Hop",
  "baseBPM": 90,
  "ambientSoundtrack": "nyc_ambient_loop",
  "environmentalSounds": ["subway_rumble", "taxi_horns"]
}
```

### Airport Configuration

Edit airports in `Assets/Config/Airports/`:

```json
{
  "airportCode": "JFK",
  "airportName": "OmniGate JFK International",
  "cityName": "OmniNYC",
  "unlockCost": 250,
  "requiredReputation": 25
}
```

---

## 🎨 UI Integration Examples

### Display Current City Music Info

```csharp
void DisplayMusicInfo()
{
    MusicBiomeController musicBiome = MusicBiomeController.Instance;
    
    if (musicBiome.currentBiome != null)
    {
        MusicBiomeData biome = musicBiome.currentBiome;
        
        UI_CityName.text = biome.cityName;
        UI_BiomeName.text = biome.biomeName;
        UI_Genre.text = biome.primaryGenre;
        UI_BPM.text = $"{biome.baseBPM} BPM";
        UI_Culture.text = biome.culturalIdentity;
    }
}
```

### Display Reputation Progress Bar

```csharp
void DisplayReputationBar(string cityName)
{
    CityReputationSystem repSystem = CityReputationSystem.Instance;
    CityReputationData rep = repSystem.GetCityReputation(cityName);
    
    // Calculate progress to next level
    int currentPoints = rep.reputationPoints;
    int nextThreshold = GetNextLevelThreshold(rep.level);
    float progress = (float)currentPoints / nextThreshold;
    
    UI_ReputationBar.fillAmount = progress;
    UI_ReputationLevel.text = rep.level.ToString();
    UI_ReputationPoints.text = $"{currentPoints}/{nextThreshold}";
}
```

### Display Available Flights

```csharp
void DisplayFlightOptions()
{
    AirportManager airportMgr = AirportManager.Instance;
    List<string> destinations = airportMgr.GetAvailableDestinations();
    
    foreach (string dest in destinations)
    {
        AirportData airport = airportMgr.GetAirport(dest);
        
        // Create flight button
        GameObject button = Instantiate(flightButtonPrefab, flightListContainer);
        button.GetComponent<Text>().text = $"{dest} - {airport.baseLandingFee} $OMNI";
        
        button.GetComponent<Button>().onClick.AddListener(() => {
            BookFlightToCity(dest);
        });
    }
}
```

---

## 📊 Events and Callbacks

### Subscribe to Reputation Events

```csharp
void Start()
{
    CityReputationSystem repSystem = CityReputationSystem.Instance;
    
    // Subscribe to reputation change event
    repSystem.OnReputationChanged += HandleReputationChanged;
    
    // Subscribe to level up event
    repSystem.OnReputationLevelUp += HandleReputationLevelUp;
}

void HandleReputationChanged(string cityName, int newPoints, ReputationLevel level)
{
    Debug.Log($"{cityName} reputation: {newPoints} ({level})");
    UpdateReputationUI(cityName, newPoints, level);
}

void HandleReputationLevelUp(string cityName, ReputationLevel newLevel)
{
    Debug.Log($"🎉 Level up in {cityName}! Now {newLevel}");
    ShowLevelUpUI(cityName, newLevel);
    PlayLevelUpSound();
}
```

---

## 🎵 Audio Asset Setup

### Required Directory Structure

```
Assets/
  Resources/
    Audio/
      Ambient/
        nyc_ambient_loop.mp3
        atlanta_ambient_loop.mp3
        tokyo_ambient_loop.mp3
        ...
      Environmental/
        NYC/
          subway_rumble.wav
          taxi_horns.wav
        Atlanta/
          car_bass.wav
          studio_sounds.wav
        Tokyo/
          train_announcements.wav
          vending_machines.wav
      Cultural/
        NYC/
          808_hit.wav
          vinyl_scratch.wav
        Atlanta/
          trap_hi_hat.wav
          808_bass.wav
        Tokyo/
          koto_strum.wav
          shamisen_pluck.wav
```

### Loading Audio Assets

```csharp
// In MusicBiomeController, implement asset loading
AudioClip ambientClip = Resources.Load<AudioClip>($"Audio/Ambient/{trackName}");
if (ambientClip != null && ambientMusicSource != null)
{
    ambientMusicSource.clip = ambientClip;
    ambientMusicSource.Play();
}
```

---

## 🧪 Testing

### Test Music Biome System

```csharp
[Test]
public void TestMusicBiomeLoading()
{
    MusicBiomeController musicBiome = MusicBiomeController.Instance;
    
    // Test loading different cities
    musicBiome.LoadBiomeForCity("OmniNYC");
    Assert.IsNotNull(musicBiome.currentBiome);
    Assert.AreEqual("OmniNYC", musicBiome.currentCityName);
    
    musicBiome.LoadBiomeForCity("OmniTokyo");
    Assert.AreEqual("OmniTokyo", musicBiome.currentCityName);
}
```

### Test Airport System

```csharp
[Test]
public void TestAirportUnlocking()
{
    AirportManager airportMgr = AirportManager.Instance;
    
    // Test unlocking an airport
    bool unlocked = airportMgr.UnlockAirport("OmniNYC", "test_wallet", 30);
    Assert.IsTrue(unlocked);
    
    AirportData airport = airportMgr.GetAirport("OmniNYC");
    Assert.IsTrue(airport.isUnlocked);
}
```

### Test Reputation System

```csharp
[Test]
public void TestReputationProgression()
{
    CityReputationSystem repSystem = CityReputationSystem.Instance;
    
    // Start at 0
    CityReputationData rep = repSystem.GetCityReputation("OmniNYC");
    Assert.AreEqual(0, rep.reputationPoints);
    
    // Add reputation
    repSystem.AddReputation("OmniNYC", 30, "Test");
    rep = repSystem.GetCityReputation("OmniNYC");
    Assert.AreEqual(30, rep.reputationPoints);
    Assert.AreEqual(ReputationLevel.Local, rep.level);
}
```

---

## 🐛 Troubleshooting

### Music Not Playing
- Ensure audio assets are in `Resources/Audio/` directory
- Check `MusicBiomeController` audio source components are initialized
- Verify volume levels in `MusicBiomeData`

### Flight Not Working
- Check if destination airport is unlocked
- Verify player has sufficient reputation
- Ensure destination is in `availableDestinations` list

### Reputation Not Updating
- Verify `CityReputationSystem.Instance` is initialized
- Check event subscriptions are properly set up
- Ensure city name matches exactly (case-sensitive)

---

## 📚 Additional Resources

- Full Documentation: `Docs/OMNISOUND_GLOBAL_GRID.md`
- Architecture Guide: `Docs/ARCHITECTURE.md`
- City Configurations: `Assets/Config/MusicBiomes/`
- Airport Configurations: `Assets/Config/Airports/`

---

## 💡 Tips and Best Practices

1. **Always load music biome when changing cities** - Creates immersive experience
2. **Track reputation for economic benefits** - Higher reputation = better rewards
3. **Use music quests for engagement** - Unique content per city keeps players exploring
4. **Crossfade audio smoothly** - Use `transitionDuration` settings for seamless changes
5. **Test with headphones** - Spatial audio and subtle effects are important

---

## 🤝 Contributing

When adding new cities:

1. Create music biome preset in `MusicBiomePresets.GetBiomeForCity()`
2. Create airport data in `AirportPresets.GetAirportForCity()`
3. Add music quest generator in `ProceduralGeneration`
4. Create JSON configs in `Assets/Config/`
5. Test all integrations

---

*Happy Building! 🎵✈️🌍*
