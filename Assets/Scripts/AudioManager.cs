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
        if (!s.Source.isPlaying)
            s.Source.Play();
        else
        {
            StartCoroutine(FadeSound(s, 0.03f, 0f));
            StartCoroutine(PlayAfterDelay(s, 0.05f));
        }
    }

    private IEnumerator PlayAfterDelay(Sound s, float delay)
    {
        yield return new WaitForSeconds(delay);
        s.Source.volume = 1f;
        s.Source.Play();
    }

    public void FadeOut(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.Name == name);
        StartCoroutine(FadeSound(s, 0.05f, 0f));
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
        s.Source.volume = 1f;
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
