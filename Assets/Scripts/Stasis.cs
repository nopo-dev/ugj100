using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stasis : MonoBehaviour
{
    [SerializeField] private ObjectiveManager _objManager;

    private void Awake()
    {
        // _collider = GetComponent<BoxCollider2D>();
        _thingsColliding = new List<GameObject>();
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
        gameObject.SetActive(false);
    }

    private void StasisMirror(GameObject mirror)
    {
        _objManager.StasisMirror(mirror, transform.position);
        gameObject.SetActive(false);
    }

    public void Reset()
    {
        gameObject.SetActive(true);
        _touched = false;
        _thingsColliding = new List<GameObject>();
    }
}
