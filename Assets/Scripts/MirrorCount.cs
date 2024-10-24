using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorCount : MonoBehaviour
{
    [SerializeField] private GameObject _mirrorCounterPrefab;

    [SerializeField] private int _numColumns;
    [SerializeField] private float _xMultiplier;
    [SerializeField] private float _yMultiplier;

    private List<GameObject> _mirrorCounters;

    private void Awake()
    {
        _mirrorCounters = new List<GameObject>();
    }

    public void AddCounter()
    {
        GameObject newCounter = Instantiate(_mirrorCounterPrefab, transform);

        int xPos = _mirrorCounters.Count % _numColumns;
        int yPos = -_mirrorCounters.Count / _numColumns;

        newCounter.transform.localPosition = new Vector3(xPos * _xMultiplier + xPos, yPos * _yMultiplier + yPos, 0f);
        newCounter.GetComponent<SpriteRenderer>().enabled = true;
        _mirrorCounters.Add(newCounter);
    }

    public void RemoveCounter()
    {
        GameObject toBeRemoved = _mirrorCounters[_mirrorCounters.Count - 1];
        _mirrorCounters.Remove(toBeRemoved);
        Destroy(toBeRemoved);
    }

    public int GetMirrorCount()
    {
        return _mirrorCounters.Count;
    }

    public void RemoveAllCounters()
    {
        foreach (GameObject mirrorCounter in _mirrorCounters)
        {
            Destroy(mirrorCounter);
        }
        _mirrorCounters = new List<GameObject>();
    }

    public void ShowMirrorCount()
    {
        foreach (GameObject mirrorCounter in _mirrorCounters)
        {
            mirrorCounter.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    public void HideMirrorCount()
    {
        foreach (GameObject mirrorCounter in _mirrorCounters)
        {
            mirrorCounter.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    private float _delayTime = 3f;
    public void DelayHideMirrorCount()
    {
        StopAllCoroutines();
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(_delayTime);
        HideMirrorCount();
    }
}
