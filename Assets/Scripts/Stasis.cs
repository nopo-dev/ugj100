using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stasis : MonoBehaviour
{
    [SerializeField] private ObjectiveManager _objManager;
    // private AudioSource _audioSource;

    private void Awake()
    {
        // _collider = GetComponent<BoxCollider2D>();
        _thingsColliding = new List<GameObject>();
        // _audioSource = GetComponent<AudioSource>();
        // _vol = _audioSource.volume;
    }

    private bool _touched;
    private List<GameObject> _thingsColliding;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_touched)
            return;

        GameObject thing = other.transform.parent.transform.parent.gameObject;
        if (_thingsColliding.Contains(thing))
            return;
        
        _thingsColliding.Add(thing);
        if (thing.tag == "Player")
        {
            _touched = true;
            StasisTouched();
        }
        else if (thing.tag == "Mirror")
        {
            _touched = true;
            StasisMirror(thing);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (_thingsColliding.Contains(other.transform.parent.transform.parent.gameObject))
        {
            _thingsColliding.Remove(other.transform.parent.transform.parent.gameObject);
        }
    }

    private void StasisTouched()
    {
        _objManager.StasisPlayer(transform.position);
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        //gameObject.SetActive(false);
        // _audioSource.pitch = 1f;
        // _audioSource.volume = _vol;
        // _audioSource.Play();
    }

    private void StasisMirror(GameObject mirror)
    {
        _objManager.StasisMirror(mirror, transform.position);
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;

        // _audioSource.pitch = 1f;
        // _audioSource.volume = _vol;
        // _audioSource.Play();
    }

    public void Reset()
    {
        // gameObject.SetActive(true);
        GetComponent<BoxCollider2D>().enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        _touched = false;
        _thingsColliding = new List<GameObject>();
        // StartCoroutine(FadeOut());
    }

    // private float _vol;
    // public IEnumerator FadeOut(float duration =  0.05f)
    // {
    //     if (!_audioSource.isPlaying)
    //         yield break;
    //     float currentTime = 0;
    //     float start = _audioSource.volume;
    //     while (currentTime < duration)
    //     {
    //         currentTime += Time.deltaTime;
    //         _audioSource.volume = Mathf.Lerp(start, 0f, currentTime / duration);
    //         yield return null;
    //     }
    //     _audioSource.Stop();
    // }
}
