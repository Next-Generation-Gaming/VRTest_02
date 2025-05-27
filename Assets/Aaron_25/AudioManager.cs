using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aaron_25
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        public AudioSource playerAudioSource;
        public AudioSource horseAudioSource;
        public AudioSource musicAudioSource;
        public AudioSource mainMenuSfxAudioSource;
        public AudioSource mainMenuMusicAudioSource;

        [Header("Settings")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 1f;

        // Singleton instance
        public static AudioManager Instance { get; private set; }

        // Internal collections
        private Dictionary<AudioLibraryType, AudioLibrary> audioLibraries = new Dictionary<AudioLibraryType, AudioLibrary>();
        private Dictionary<AudioLibraryType, AudioSource> audioSources = new Dictionary<AudioLibraryType, AudioSource>();
        private Queue<AudioClipData> audioClipQueue = new Queue<AudioClipData>();

        private void Awake()
        {
            // Implement proper singleton pattern
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Multiple AudioManager instances detected. Destroying duplicate on {gameObject.name}");
                Destroy(this);
                return;
            }
            
            Instance = this;
           // DontDestroyOnLoad(gameObject);
            
            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            // Map audio sources to their corresponding library types
            if (playerAudioSource != null)
            {
                audioSources[AudioLibraryType.Player] = playerAudioSource;
            }
            
            if (horseAudioSource != null)
            {
                audioSources[AudioLibraryType.Horse] = horseAudioSource;
            }
            
            if (musicAudioSource != null)
            {
                audioSources[AudioLibraryType.Music] = musicAudioSource;
            }
            
            if (mainMenuSfxAudioSource != null)
            {
                audioSources[AudioLibraryType.MainMenuSFX] = mainMenuSfxAudioSource;
            }
            
            if (mainMenuMusicAudioSource != null)
            {
                audioSources[AudioLibraryType.MainMenuMusic] = mainMenuMusicAudioSource;
            }
        }
        
        
        //Horse and Player have different libaries, so we need to register them separately
        public void RegisterAudioLibrary(AudioLibrary audioLibrary, AudioLibraryType libraryType)
        {
            if (audioLibrary == null)
            {
                Debug.LogError("Attempted to register null AudioLibrary");
                return;
            }

            if (audioLibraries.ContainsKey(libraryType))
            {
                Debug.LogWarning($"AudioLibrary of type {libraryType} already registered. Overwriting.");
            }

            audioLibraries[libraryType] = audioLibrary;
            Debug.Log($"AudioLibrary registered: {audioLibrary.name} as {libraryType}");
        }
        
        
        public void PlayAudio(int audioClipID, AudioLibraryType audioLibraryType, bool shouldOverride = false)
        {
            // Validate library exists
            if (!audioLibraries.ContainsKey(audioLibraryType))
            {
                Debug.LogError($"No AudioLibrary registered for type: {audioLibraryType}");
                return;
            }

            // Validate audio source exists
            if (!audioSources.ContainsKey(audioLibraryType) || audioSources[audioLibraryType] == null)
            {
                Debug.LogError($"No AudioSource assigned for type: {audioLibraryType}");
                return;
            }

            // Get the audio clip
            AudioClip clip = audioLibraries[audioLibraryType].GetAudioClipByID(audioClipID);
            if (clip == null)
            {
                Debug.LogError($"Failed to get audio clip with ID {audioClipID} from {audioLibraryType} library");
                return;
            }

            AudioSource source = audioSources[audioLibraryType];

            // Check if we should override currently playing audio
            if (!shouldOverride && source.isPlaying)
            {
                Debug.Log($"Audio already playing on {audioLibraryType} source. Use shouldOverride=true to interrupt.");
                QueueAudio(audioClipID, audioLibraryType);
                return;
            }
            
            // Stop currently playing audio if shouldOverride is true
            if (shouldOverride && source.isPlaying)
            {
                Debug.Log($"Stopping currently playing audio on {audioLibraryType} source to play new clip");
                source.Stop();
            }
            
            // Play the audio
            source.clip = clip;
            source.volume = masterVolume;
            source.Play();
            Debug.Log($"Playing audio: {clip.name} on {audioLibraryType} source");
            StartCoroutine(WaitForAudioToFinish(source));
            Debug.Log("Audio Finished Playing: " + clip.name);
            
            // Check queue for next audio
            if (audioClipQueue.Count > 0)
            {
                Debug.Log("Playing next queued audio clip");
                PlayNextQueuedAudio();
            }
            
         
        }

        public void PlayAudioByName(string clipName, AudioLibraryType audioLibraryType, bool shouldOverride = false)
        {
            if (!audioLibraries.ContainsKey(audioLibraryType))
            {
                Debug.LogError($"No AudioLibrary registered for type: {audioLibraryType}");
                return;
            }

            AudioClip clip = audioLibraries[audioLibraryType].GetAudioClipByName(clipName);
            if (clip == null)
            {
                Debug.LogError($"Failed to get audio clip '{clipName}' from {audioLibraryType} library");
                return;
            }

            // Find the ID and use the existing PlayAudio method
            int clipID = audioLibraries[audioLibraryType].GetAudioClipID(clipName);
            if (clipID >= 0)
            {
                PlayAudio(clipID, audioLibraryType, shouldOverride);
            }
        }

        public void QueueAudio(int audioClipID, AudioLibraryType audioLibraryType)
        {
            audioClipQueue.Enqueue(new AudioClipData(audioClipID, audioLibraryType));
            Debug.Log($"Queued audio clip ID {audioClipID} from {audioLibraryType} library");
        }
        
        IEnumerator WaitForAudioToFinish(AudioSource source)
        {
            while (source.isPlaying)
            {
                yield return null; // Wait for the next frame
            }
        }

        public void PlayNextQueuedAudio()
        {
            // wait until current audio finishes playing
            if (audioClipQueue.Count > 0)
            {
                AudioClipData clipData = audioClipQueue.Dequeue();
                PlayAudio(clipData.audioClipID, clipData.audioLibraryType, false);
            }
            else
            {
                Debug.Log("No audio clips in the queue.");
            }
        }

        public void StopAudio(AudioLibraryType audioLibraryType)
        {
            if (audioSources.ContainsKey(audioLibraryType) && audioSources[audioLibraryType] != null)
            {
                audioSources[audioLibraryType].Stop();
                Debug.Log($"Stopped audio on {audioLibraryType} source");
            }
        }

        public void StopAllAudio()
        {
            foreach (var source in audioSources.Values)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }
            Debug.Log("Stopped all audio sources");
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            
            // Update all currently playing sources
            foreach (var source in audioSources.Values)
            {
                if (source != null)
                {
                    source.volume = masterVolume;
                }
            }
        }
        
        //Used by XR Simple Interactable Object in the Main Menu Scene, in the OnHover event
        public void PlayMainMenuHoverSFX()
        {
            PlayAudio(1, AudioLibraryType.MainMenuSFX);
        }
        public void PlayPickupSFX()
        {
            PlayAudio(8, AudioLibraryType.Player);
        }
        
        public void PlayPutdownSFX()
        {
            PlayAudio(9, AudioLibraryType.Player);
        }

        public bool IsPlaying(AudioLibraryType audioLibraryType)
        {
            return audioSources.ContainsKey(audioLibraryType) && 
                   audioSources[audioLibraryType] != null && 
                   audioSources[audioLibraryType].isPlaying;
        }

        public int GetQueueCount()
        {
            return audioClipQueue.Count;
        }

        public void ClearQueue()
        {
            audioClipQueue.Clear();
            Debug.Log("Audio queue cleared");
        }
    }
    
    public struct AudioClipData
    {
        public int audioClipID;
        public AudioLibraryType audioLibraryType;

        public AudioClipData(int id, AudioLibraryType type)
        {
            audioClipID = id;
            audioLibraryType = type;
        }
    }
}