using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string Name;
    public AudioClip Clip;

    [Range(0f, 2f)] public float Volume;
    [Range(-3f, 3f)] public float Pitch = 1f;
    public bool Loop;

    [HideInInspector]
    public AudioSource Source;
}