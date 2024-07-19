using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioController : MonoBehaviour
{
    public AudioSource audioSource;
    public static AudioSource audioSourceStatic;

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
            AudioController.audioSourceStatic.PlayOneShot(clip);
        }
    }
}