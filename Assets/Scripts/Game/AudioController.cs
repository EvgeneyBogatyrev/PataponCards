using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioController : MonoBehaviour
{
    // Only one instance is ever needed for the whole app session - lives in TitleScreen (scene
    // index 0, the real entry point) and survives every scene load via DontDestroyOnLoad, so
    // PlaySound works from any scene without needing this object duplicated into all of them.
    // Any other AudioController placed directly in a later scene (there's already one sitting in
    // Game.unity from before this) just self-destroys here instead of conflicting.
    private static AudioController instance;

    public AudioSource audioSource;
    public static AudioSource audioSourceStatic;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Per-clip loudness correction, layered on top of whatever normalization the source files
    // already have - fixes an individual sound that still reads too loud/quiet by ear without
    // needing to re-export audio. Key is the same soundName passed to PlaySound (file name minus
    // extension); 1 = unchanged, e.g. 0.6 plays that one clip at 60% volume. Leave a clip out of
    // this dictionary entirely to leave it at full volume.
    private static readonly Dictionary<string, float> volumeOverrides = new Dictionary<string, float>
    {
        // { "destrobo_hm", 0.7f },
    };

    void Start()
    {
        audioSourceStatic = audioSource;
    }

    public static void PlaySound(string soundName)
    {
        //Debug.Log("Sound");
        //Debug.Log(soundName);
        if (soundName != null)
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/" + soundName);
            float volumeScale = volumeOverrides.TryGetValue(soundName, out float scale) ? scale : 1f;
            AudioController.audioSourceStatic.PlayOneShot(clip, volumeScale);
        }
    }
}