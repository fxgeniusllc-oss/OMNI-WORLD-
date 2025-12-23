using UnityEngine;
using System.Collections.Generic;

namespace OmniWorld.World
{
    /// <summary>
    /// Data structure for music biome configurations
    /// Defines city-specific sound profiles, ambient tracks, and cultural identity
    /// </summary>
    [System.Serializable]
    public class MusicBiomeData
    {
        [Header("Biome Identity")]
        public string cityName;
        public string biomeName;
        public string culturalIdentity;
        
        [Header("Music Configuration")]
        public string primaryGenre;
        public List<string> subGenres = new List<string>();
        public string ambientSoundtrack;
        
        [Header("Sound Environment")]
        public List<string> environmentalSounds = new List<string>();
        public float baseBPM = 120f;
        public string daypartRhythm; // Morning, Afternoon, Evening, Night variations
        
        [Header("Cultural Audio Elements")]
        public List<string> culturalInstruments = new List<string>();
        public List<string> streetSounds = new List<string>();
        public string languageAccent;
        
        [Header("Audio Mix Settings")]
        [Range(0f, 1f)]
        public float ambientVolume = 0.5f;
        [Range(0f, 1f)]
        public float musicVolume = 0.6f;
        [Range(0f, 1f)]
        public float sfxVolume = 0.7f;
        
        [Header("Dynamic Music System")]
        public bool supportsDynamicTransitions = true;
        public float transitionDuration = 3.0f;
        public string introClip;
        public string loopClip;
        public string outroClip;
        
        [Header("District Variations")]
        public Dictionary<string, DistrictSoundProfile> districtProfiles = new Dictionary<string, DistrictSoundProfile>();
    }
    
    [System.Serializable]
    public class DistrictSoundProfile
    {
        public string districtName;
        public string ambientTrack;
        public List<string> additionalSFX = new List<string>();
        public float intensityMultiplier = 1.0f;
    }
    
    /// <summary>
    /// Predefined music biome configurations for each city
    /// </summary>
    public static class MusicBiomePresets
    {
        public static MusicBiomeData GetBiomeForCity(string cityName)
        {
            switch (cityName)
            {
                case "OmniNYC":
                    return new MusicBiomeData
                    {
                        cityName = "OmniNYC",
                        biomeName = "Boom Bap Metropolitan",
                        culturalIdentity = "Classic Hip-Hop, Financial Capital, Art Scene",
                        primaryGenre = "Hip-Hop",
                        subGenres = new List<string> { "Boom Bap", "Jazz Rap", "Underground" },
                        ambientSoundtrack = "nyc_ambient_loop",
                        environmentalSounds = new List<string> { "subway_rumble", "taxi_horns", "street_chatter", "construction" },
                        baseBPM = 90f,
                        daypartRhythm = "morning_hustle|afternoon_grind|evening_cool|night_pulse",
                        culturalInstruments = new List<string> { "808_drums", "jazz_samples", "vinyl_scratches" },
                        streetSounds = new List<string> { "street_vendors", "police_sirens", "basketball_courts" },
                        languageAccent = "New York English",
                        ambientVolume = 0.4f,
                        musicVolume = 0.6f,
                        sfxVolume = 0.8f,
                        supportsDynamicTransitions = true,
                        transitionDuration = 2.5f
                    };
                    
                case "Berlin":
                    return new MusicBiomeData
                    {
                        cityName = "Berlin",
                        biomeName = "Techno Underground",
                        culturalIdentity = "Cold Concrete Echo, Techno Capital, Underground Culture",
                        primaryGenre = "Techno",
                        subGenres = new List<string> { "Industrial Techno", "Minimal", "Deep House" },
                        ambientSoundtrack = "berlin_ambient_loop",
                        environmentalSounds = new List<string> { "concrete_reverb", "tram_pass", "distant_bass", "warehouse_echo" },
                        baseBPM = 130f,
                        daypartRhythm = "morning_minimal|afternoon_build|evening_peak|night_rave",
                        culturalInstruments = new List<string> { "analog_synth", "drum_machines", "modular_sequences" },
                        streetSounds = new List<string> { "bike_bells", "techno_clubs", "train_stations" },
                        languageAccent = "German",
                        ambientVolume = 0.3f,
                        musicVolume = 0.7f,
                        sfxVolume = 0.6f,
                        supportsDynamicTransitions = true,
                        transitionDuration = 4.0f
                    };
                    
                case "Lagos":
                    return new MusicBiomeData
                    {
                        cityName = "Lagos",
                        biomeName = "Afrobeats Market Energy",
                        culturalIdentity = "Street Market Energy, Afrobeats Polyrhythm, Cultural Hub",
                        primaryGenre = "Afrobeats",
                        subGenres = new List<string> { "Afro-Fusion", "Highlife", "Juju" },
                        ambientSoundtrack = "lagos_ambient_loop",
                        environmentalSounds = new List<string> { "market_chatter", "okada_bikes", "church_bells", "generators" },
                        baseBPM = 110f,
                        daypartRhythm = "morning_awakening|afternoon_hustle|evening_celebration|night_clubs",
                        culturalInstruments = new List<string> { "talking_drums", "shekere", "log_drums", "guitars" },
                        streetSounds = new List<string> { "street_hawkers", "music_blasting", "crowd_energy" },
                        languageAccent = "Nigerian Pidgin",
                        ambientVolume = 0.6f,
                        musicVolume = 0.8f,
                        sfxVolume = 0.9f,
                        supportsDynamicTransitions = true,
                        transitionDuration = 2.0f
                    };
                    
                case "OmniTokyo":
                    return new MusicBiomeData
                    {
                        cityName = "OmniTokyo",
                        biomeName = "Cyber-Minimal Metropolis",
                        culturalIdentity = "Minimalist Tonal Swells, Cyber-Tech, Anime Culture",
                        primaryGenre = "J-Pop",
                        subGenres = new List<string> { "City Pop", "Future Bass", "Vaporwave" },
                        ambientSoundtrack = "tokyo_ambient_loop",
                        environmentalSounds = new List<string> { "train_announcements", "pedestrian_crossing", "pachinko_sounds", "vending_machines" },
                        baseBPM = 128f,
                        daypartRhythm = "morning_serene|afternoon_tech|evening_neon|night_cyber",
                        culturalInstruments = new List<string> { "koto", "shamisen", "synth_pads", "electronic_drums" },
                        streetSounds = new List<string> { "arcade_machines", "convenience_store_jingles", "announcements" },
                        languageAccent = "Japanese",
                        ambientVolume = 0.35f,
                        musicVolume = 0.55f,
                        sfxVolume = 0.7f,
                        supportsDynamicTransitions = true,
                        transitionDuration = 3.5f
                    };
                    
                case "OmniLanta":
                    return new MusicBiomeData
                    {
                        cityName = "OmniLanta",
                        biomeName = "Trap Culture Hub",
                        culturalIdentity = "Creator Culture, Trap Legacy, Tech Hub",
                        primaryGenre = "Trap",
                        subGenres = new List<string> { "Southern Hip-Hop", "R&B", "Neo-Soul" },
                        ambientSoundtrack = "atlanta_ambient_loop",
                        environmentalSounds = new List<string> { "car_bass", "neighborhood_vibes", "studio_sounds", "freeway_hum" },
                        baseBPM = 140f,
                        daypartRhythm = "morning_chill|afternoon_grind|evening_turn_up|night_studio",
                        culturalInstruments = new List<string> { "808s", "hi_hats", "synth_leads", "vocal_samples" },
                        streetSounds = new List<string> { "dice_games", "cookout_music", "block_parties" },
                        languageAccent = "Southern Drawl",
                        ambientVolume = 0.45f,
                        musicVolume = 0.75f,
                        sfxVolume = 0.85f,
                        supportsDynamicTransitions = true,
                        transitionDuration = 2.0f
                    };
                    
                case "OmniVegas":
                    return new MusicBiomeData
                    {
                        cityName = "OmniVegas",
                        biomeName = "Neon Capital Energy",
                        culturalIdentity = "High Stakes, Neon Capital, Risk/Reward",
                        primaryGenre = "EDM",
                        subGenres = new List<string> { "House", "Progressive", "Electro" },
                        ambientSoundtrack = "vegas_ambient_loop",
                        environmentalSounds = new List<string> { "slot_machines", "crowd_cheers", "fountain_shows", "casino_chips" },
                        baseBPM = 128f,
                        daypartRhythm = "morning_recovery|afternoon_poolside|evening_showtime|night_party",
                        culturalInstruments = new List<string> { "synth_stabs", "vocal_chops", "big_drops", "lasers" },
                        streetSounds = new List<string> { "casino_ambience", "live_shows", "party_crowds" },
                        languageAccent = "American English",
                        ambientVolume = 0.5f,
                        musicVolume = 0.8f,
                        sfxVolume = 0.9f,
                        supportsDynamicTransitions = true,
                        transitionDuration = 1.5f
                    };
                    
                case "OmniDubai":
                    return new MusicBiomeData
                    {
                        cityName = "OmniDubai",
                        biomeName = "Luxury Innovation Fusion",
                        culturalIdentity = "Luxury, Innovation, Global Trade",
                        primaryGenre = "Arabic Pop",
                        subGenres = new List<string> { "World Fusion", "Deep House", "Oriental Jazz" },
                        ambientSoundtrack = "dubai_ambient_loop",
                        environmentalSounds = new List<string> { "call_to_prayer", "luxury_cars", "mall_ambience", "fountain_shows" },
                        baseBPM = 115f,
                        daypartRhythm = "morning_elegant|afternoon_business|evening_luxury|night_exclusive",
                        culturalInstruments = new List<string> { "oud", "qanun", "darbuka", "modern_synths" },
                        streetSounds = new List<string> { "luxury_shopping", "valet_service", "multilingual_chatter" },
                        languageAccent = "Arabic/English Mix",
                        ambientVolume = 0.4f,
                        musicVolume = 0.65f,
                        sfxVolume = 0.7f,
                        supportsDynamicTransitions = true,
                        transitionDuration = 3.0f
                    };
                    
                case "OmniLA":
                    return new MusicBiomeData
                    {
                        cityName = "OmniLA",
                        biomeName = "West Coast Vibes",
                        culturalIdentity = "Entertainment Industry, Beach Culture, Creative Hub",
                        primaryGenre = "West Coast Hip-Hop",
                        subGenres = new List<string> { "G-Funk", "Indie Pop", "Latin Urban" },
                        ambientSoundtrack = "la_ambient_loop",
                        environmentalSounds = new List<string> { "ocean_waves", "skateboards", "convertible_cars", "palm_trees_wind" },
                        baseBPM = 95f,
                        daypartRhythm = "morning_sunrise|afternoon_beach|evening_golden_hour|night_hollywood",
                        culturalInstruments = new List<string> { "synth_bass", "guitar_licks", "smooth_keys", "latin_percussion" },
                        streetSounds = new List<string> { "food_trucks", "street_performers", "movie_sets" },
                        languageAccent = "California English",
                        ambientVolume = 0.5f,
                        musicVolume = 0.7f,
                        sfxVolume = 0.75f,
                        supportsDynamicTransitions = true,
                        transitionDuration = 2.5f
                    };
                    
                case "OmniParis":
                    return new MusicBiomeData
                    {
                        cityName = "OmniParis",
                        biomeName = "Artistic Romance",
                        culturalIdentity = "Art, Fashion, Culture, Romance",
                        primaryGenre = "French House",
                        subGenres = new List<string> { "Chanson", "Nu-Disco", "Jazz Manouche" },
                        ambientSoundtrack = "paris_ambient_loop",
                        environmentalSounds = new List<string> { "accordion_distant", "cafe_chatter", "metro_rumble", "heels_on_cobblestone" },
                        baseBPM = 120f,
                        daypartRhythm = "morning_cafe|afternoon_gallery|evening_bistro|night_club",
                        culturalInstruments = new List<string> { "accordion", "violin", "filtered_disco", "vocoders" },
                        streetSounds = new List<string> { "cafe_sounds", "fashion_shows", "art_exhibitions" },
                        languageAccent = "French",
                        ambientVolume = 0.4f,
                        musicVolume = 0.6f,
                        sfxVolume = 0.65f,
                        supportsDynamicTransitions = true,
                        transitionDuration = 3.0f
                    };
                    
                default:
                    return CreateDefaultBiome(cityName);
            }
        }
        
        private static MusicBiomeData CreateDefaultBiome(string cityName)
        {
            return new MusicBiomeData
            {
                cityName = cityName,
                biomeName = "Generic Urban",
                culturalIdentity = "Urban Metropolitan",
                primaryGenre = "Electronic",
                subGenres = new List<string> { "Ambient", "Chill" },
                ambientSoundtrack = "default_ambient_loop",
                environmentalSounds = new List<string> { "city_ambience", "traffic" },
                baseBPM = 100f,
                daypartRhythm = "morning|afternoon|evening|night",
                culturalInstruments = new List<string> { "synth", "drums" },
                streetSounds = new List<string> { "people_talking", "cars_passing" },
                languageAccent = "Neutral",
                ambientVolume = 0.5f,
                musicVolume = 0.6f,
                sfxVolume = 0.7f,
                supportsDynamicTransitions = true,
                transitionDuration = 2.0f
            };
        }
    }
}
