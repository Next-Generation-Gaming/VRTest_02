using System.Collections.Generic;
using UnityEngine;

namespace Aaron_25
{
    public class AudioLibrary : MonoBehaviour
    {
        [Header("Configuration")]
        public AudioLibraryType libraryType;
        
        [Header("Audio Clips")]
        public List<AudioClip> audioClips = new List<AudioClip>();

        private void Start()
        {
            RegisterWithAudioManager();
        }

        private void RegisterWithAudioManager()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogError("AudioManager instance not found. Ensure AudioManager exists in the scene and is initialized before AudioLibrary.");
                return;
            }

            if (audioClips == null || audioClips.Count == 0)
            {
                Debug.LogWarning($"AudioLibrary '{gameObject.name}' has no audio clips assigned.");
            }

            AudioManager.Instance.RegisterAudioLibrary(this, libraryType);
            Debug.Log($"AudioLibrary '{gameObject.name}' registered as {libraryType} with {audioClips.Count} clips");
        }

        public AudioClip GetAudioClipByID(int id)
        {
            if (audioClips == null || id < 0 || id >= audioClips.Count)
            {
                Debug.LogError($"Invalid audio clip ID: {id}. Valid range: 0-{(audioClips?.Count - 1 ?? -1)}");
                return null;
            }

            if (audioClips[id] == null)
            {
                Debug.LogError($"Audio clip at index {id} is null");
                return null;
            }

            return audioClips[id];
        }

        public AudioClip GetAudioClipByName(string clipName)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                Debug.LogError("Audio clip name cannot be null or empty");
                return null;
            }

            if (audioClips == null)
            {
                Debug.LogError("Audio clips list is null");
                return null;
            }

            foreach (var clip in audioClips)
            {
                if (clip != null && clip.name.Equals(clipName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return clip;
                }
            }

            Debug.LogError($"Audio clip not found: '{clipName}' in {libraryType} library");
            return null;
        }

        public int GetAudioClipID(string clipName)
        {
            if (string.IsNullOrEmpty(clipName) || audioClips == null)
            {
                return -1;
            }

            for (int i = 0; i < audioClips.Count; i++)
            {
                if (audioClips[i] != null && audioClips[i].name.Equals(clipName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        public int GetAudioClipCount()
        {
            return audioClips?.Count ?? 0;
        }

        public List<string> GetAllClipNames()
        {
            List<string> names = new List<string>();
            if (audioClips != null)
            {
                foreach (var clip in audioClips)
                {
                    names.Add(clip != null ? clip.name : "NULL");
                }
            }
            return names;
        }

        // Editor helper method
        #if UNITY_EDITOR
        [ContextMenu("Log All Clip Names")]
        private void LogAllClipNames()
        {
            var names = GetAllClipNames();
            Debug.Log($"Audio clips in {libraryType} library: {string.Join(", ", names)}");
        }
        #endif
    }

    public enum AudioLibraryType
    {
        Player,
        Horse,
        Music,
        MainMenuSFX,
        MainMenuMusic
    }
}