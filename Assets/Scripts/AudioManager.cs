using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] Sounds;

    private void Awake()
    {
        foreach (Sound s in Sounds)
        {
            s.Source = gameObject.AddComponent<AudioSource>();
            s.Source.clip = s.Clip;
            s.Source.volume = s.Volume;
            s.Source.pitch = s.Pitch;
            s.Source.loop = s.Loop;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.Name == name);
        if (s == null)
        {
            return;
        }
        s.Source.Play();
    }

    public static IEnumerator FadeSound(Sound s, float duration, float targetVolume)
    {
        float vol = s.Source.volume;
        if (!s.Source.isPlaying)
            yield break;
        float currentTime = 0;
        float start = s.Source.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            s.Source.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        s.Source.Stop();
        s.Source.volume = vol;
    }

    public void Duck(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.Name == name);
        if (s == null)
        {
            return;
        }
        s.Source.volume *= 0.5f;
    }

    public void Unduck(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.Name == name);
        if (s == null)
        {
            return;
        }
        s.Source.volume *= 2f;
    }
}
