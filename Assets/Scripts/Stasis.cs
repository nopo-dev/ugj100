using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stasis : MonoBehaviour
{
    [SerializeField] private ObjectiveManager _objManager;

    // private BoxCollider2D _collider;

    private void Awake()
    {
        // _collider = GetComponent<BoxCollider2D>();
        _thingsColliding = new List<GameObject>();
    }

    private List<GameObject> _thingsColliding;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (_thingsColliding.Contains(other.transform.parent.transform.parent.gameObject))
                return;
            
            _thingsColliding.Add(other.transform.parent.transform.parent.gameObject);
            StasisTouched();
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
        _objManager.StasisPlayer();
        gameObject.SetActive(false);
    }

    public void Reset()
    {
        gameObject.SetActive(true);
    }
}
