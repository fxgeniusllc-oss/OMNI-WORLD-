using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OmniWorld.World
{
    /// <summary>
    /// Controls music biome system - dynamically switches sound environments per city
    /// Manages ambient soundtracks, environmental SFX, and daypart transitions
    /// </summary>
    public class MusicBiomeController : MonoBehaviour
    {
        private static MusicBiomeController _instance;
        public static MusicBiomeController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MusicBiomeController>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MusicBiomeController");
                        _instance = go.AddComponent<MusicBiomeController>();
                    }
                }
                return _instance;
            }
        }

        [Header("Current Biome")]
        public MusicBiomeData currentBiome;
        public string currentCityName;
        
        [Header("Audio Sources")]
        public AudioSource ambientMusicSource;
        public AudioSource environmentalSFXSource;
        public AudioSource culturalSoundSource;
        
        [Header("Daypart System")]
        public DayPart currentDayPart = DayPart.Morning;
        public float daypartTransitionTime = 60f; // seconds per daypart
        private float daypartTimer = 0f;
        
        [Header("Dynamic Transitions")]
        public bool enableDynamicTransitions = true;
        public float crossfadeDuration = 3.0f;
        private Coroutine transitionCoroutine;
        
        [Header("Audio State")]
        private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
        private bool isTransitioning = false;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAudioSources();
        }

        private void Start()
        {
            // Start with default city biome
            LoadBiomeForCity("OmniLanta");
        }

        private void Update()
        {
            UpdateDaypartCycle();
        }

        /// <summary>
        /// Initialize audio sources if they don't exist
        /// </summary>
        private void InitializeAudioSources()
        {
            if (ambientMusicSource == null)
            {
                GameObject musicGO = new GameObject("AmbientMusic");
                musicGO.transform.SetParent(transform);
                ambientMusicSource = musicGO.AddComponent<AudioSource>();
                ambientMusicSource.loop = true;
                ambientMusicSource.spatialBlend = 0f; // 2D sound
            }
            
            if (environmentalSFXSource == null)
            {
                GameObject sfxGO = new GameObject("EnvironmentalSFX");
                sfxGO.transform.SetParent(transform);
                environmentalSFXSource = sfxGO.AddComponent<AudioSource>();
                environmentalSFXSource.loop = true;
                environmentalSFXSource.spatialBlend = 0f;
            }
            
            if (culturalSoundSource == null)
            {
                GameObject culturalGO = new GameObject("CulturalSounds");
                culturalGO.transform.SetParent(transform);
                culturalSoundSource = culturalGO.AddComponent<AudioSource>();
                culturalSoundSource.loop = false;
                culturalSoundSource.spatialBlend = 0f;
            }
            
            Debug.Log("Music Biome Controller audio sources initialized");
        }

        /// <summary>
        /// Load music biome for a specific city
        /// </summary>
        public void LoadBiomeForCity(string cityName)
        {
            if (currentCityName == cityName && currentBiome != null)
            {
                Debug.Log($"Already in {cityName} biome");
                return;
            }

            MusicBiomeData newBiome = MusicBiomePresets.GetBiomeForCity(cityName);
            
            if (newBiome == null)
            {
                Debug.LogWarning($"No biome preset found for city: {cityName}");
                return;
            }

            Debug.Log($"Loading music biome: {newBiome.biomeName} for {cityName}");
            
            if (currentBiome != null && enableDynamicTransitions && newBiome.supportsDynamicTransitions)
            {
                TransitionToBiome(newBiome);
            }
            else
            {
                ApplyBiomeImmediate(newBiome);
            }
            
            currentCityName = cityName;
            currentBiome = newBiome;
        }

        /// <summary>
        /// Apply biome immediately without transition
        /// </summary>
        private void ApplyBiomeImmediate(MusicBiomeData biome)
        {
            currentBiome = biome;
            
            // Set volumes
            if (ambientMusicSource != null)
                ambientMusicSource.volume = biome.musicVolume;
            if (environmentalSFXSource != null)
                environmentalSFXSource.volume = biome.ambientVolume;
            if (culturalSoundSource != null)
                culturalSoundSource.volume = biome.sfxVolume;
            
            // Load and play ambient soundtrack
            LoadAndPlayAmbient(biome.ambientSoundtrack);
            
            // Play environmental sounds
            PlayEnvironmentalSounds(biome.environmentalSounds);
            
            Debug.Log($"Biome applied: {biome.biomeName}");
            Debug.Log($"Genre: {biome.primaryGenre} | BPM: {biome.baseBPM} | Culture: {biome.culturalIdentity}");
        }

        /// <summary>
        /// Transition smoothly between biomes using crossfade
        /// </summary>
        private void TransitionToBiome(MusicBiomeData newBiome)
        {
            if (isTransitioning && transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            
            transitionCoroutine = StartCoroutine(CrossfadeTransition(newBiome));
        }

        /// <summary>
        /// Coroutine for smooth audio crossfade between biomes
        /// </summary>
        private IEnumerator CrossfadeTransition(MusicBiomeData newBiome)
        {
            isTransitioning = true;
            
            float duration = newBiome.transitionDuration;
            float elapsed = 0f;
            
            // Store original volumes
            float originalMusicVol = ambientMusicSource != null ? ambientMusicSource.volume : 0f;
            float originalAmbientVol = environmentalSFXSource != null ? environmentalSFXSource.volume : 0f;
            
            // Fade out current
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2f);
                
                if (ambientMusicSource != null)
                    ambientMusicSource.volume = Mathf.Lerp(originalMusicVol, 0f, t);
                if (environmentalSFXSource != null)
                    environmentalSFXSource.volume = Mathf.Lerp(originalAmbientVol, 0f, t);
                
                yield return null;
            }
            
            // Switch to new biome
            ApplyBiomeImmediate(newBiome);
            
            // Start with zero volume
            if (ambientMusicSource != null)
                ambientMusicSource.volume = 0f;
            if (environmentalSFXSource != null)
                environmentalSFXSource.volume = 0f;
            
            // Fade in new
            elapsed = 0f;
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2f);
                
                if (ambientMusicSource != null)
                    ambientMusicSource.volume = Mathf.Lerp(0f, newBiome.musicVolume, t);
                if (environmentalSFXSource != null)
                    environmentalSFXSource.volume = Mathf.Lerp(0f, newBiome.ambientVolume, t);
                
                yield return null;
            }
            
            // Ensure final volumes are correct
            if (ambientMusicSource != null)
                ambientMusicSource.volume = newBiome.musicVolume;
            if (environmentalSFXSource != null)
                environmentalSFXSource.volume = newBiome.ambientVolume;
            
            isTransitioning = false;
            
            Debug.Log($"Biome transition complete: {newBiome.biomeName}");
        }

        /// <summary>
        /// Load and play ambient soundtrack
        /// </summary>
        private void LoadAndPlayAmbient(string trackName)
        {
            // In a real implementation, this would load from Resources or AssetBundles
            // For now, we'll log the track that should be playing
            Debug.Log($"Loading ambient track: {trackName}");
            
            // TODO: Implement actual audio clip loading
            // AudioClip clip = Resources.Load<AudioClip>($"Audio/Ambient/{trackName}");
            // if (clip != null && ambientMusicSource != null)
            // {
            //     ambientMusicSource.clip = clip;
            //     ambientMusicSource.Play();
            // }
        }

        /// <summary>
        /// Play environmental sound effects
        /// </summary>
        private void PlayEnvironmentalSounds(List<string> sounds)
        {
            if (sounds == null || sounds.Count == 0)
                return;
            
            Debug.Log($"Environmental sounds: {string.Join(", ", sounds)}");
            
            // TODO: Implement layered environmental sound system
            // Could use multiple audio sources for different environmental layers
        }

        /// <summary>
        /// Update daypart cycle
        /// </summary>
        private void UpdateDaypartCycle()
        {
            daypartTimer += Time.deltaTime;
            
            if (daypartTimer >= daypartTransitionTime)
            {
                daypartTimer = 0f;
                AdvanceDaypart();
            }
        }

        /// <summary>
        /// Advance to next daypart
        /// </summary>
        private void AdvanceDaypart()
        {
            currentDayPart = (DayPart)(((int)currentDayPart + 1) % 4);
            
            Debug.Log($"Daypart changed to: {currentDayPart}");
            
            // Adjust music intensity/variation based on daypart
            AdjustForDaypart(currentDayPart);
        }

        /// <summary>
        /// Adjust audio parameters based on time of day
        /// </summary>
        private void AdjustForDaypart(DayPart daypart)
        {
            if (currentBiome == null)
                return;
            
            // Parse daypart rhythm if available
            if (!string.IsNullOrEmpty(currentBiome.daypartRhythm))
            {
                string[] rhythms = currentBiome.daypartRhythm.Split('|');
                if (rhythms.Length >= 4)
                {
                    string rhythm = rhythms[(int)daypart];
                    Debug.Log($"Current rhythm: {rhythm}");
                    
                    // TODO: Implement rhythm variations
                    // Could adjust BPM, volume, or switch to variation tracks
                }
            }
        }

        /// <summary>
        /// Play a cultural sound effect (one-shot)
        /// </summary>
        public void PlayCulturalSound(string soundName)
        {
            if (culturalSoundSource == null)
                return;
            
            Debug.Log($"Playing cultural sound: {soundName}");
            
            // TODO: Load and play cultural sound
            // AudioClip clip = Resources.Load<AudioClip>($"Audio/Cultural/{currentCityName}/{soundName}");
            // if (clip != null)
            //     culturalSoundSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Get current biome info for UI display
        /// </summary>
        public string GetBiomeInfo()
        {
            if (currentBiome == null)
                return "No biome loaded";
            
            return $"{currentBiome.biomeName}\n" +
                   $"Genre: {currentBiome.primaryGenre}\n" +
                   $"BPM: {currentBiome.baseBPM}\n" +
                   $"Culture: {currentBiome.culturalIdentity}";
        }

        /// <summary>
        /// Set master volume for music biome system
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            
            if (ambientMusicSource != null && currentBiome != null)
                ambientMusicSource.volume = currentBiome.musicVolume * volume;
            if (environmentalSFXSource != null && currentBiome != null)
                environmentalSFXSource.volume = currentBiome.ambientVolume * volume;
            if (culturalSoundSource != null && currentBiome != null)
                culturalSoundSource.volume = currentBiome.sfxVolume * volume;
        }

        /// <summary>
        /// Play district-specific variation
        /// </summary>
        public void EnterDistrict(string districtName)
        {
            if (currentBiome == null || currentBiome.districtProfiles == null)
                return;
            
            if (currentBiome.districtProfiles.ContainsKey(districtName))
            {
                DistrictSoundProfile profile = currentBiome.districtProfiles[districtName];
                Debug.Log($"Entering district: {districtName} with profile: {profile.districtName}");
                
                // TODO: Apply district-specific audio variations
                // Could layer additional SFX or switch ambient track
            }
        }
    }

    /// <summary>
    /// Time of day periods for dynamic music shifts
    /// </summary>
    public enum DayPart
    {
        Morning = 0,    // 6am-12pm: Awakening, hustle
        Afternoon = 1,  // 12pm-6pm: Peak activity
        Evening = 2,    // 6pm-12am: Wind down, nightlife prep
        Night = 3       // 12am-6am: Late night, club hours
    }
}
