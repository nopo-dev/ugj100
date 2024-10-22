using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Portal _pairedPortal;
    private Vector3 _sendPosition;
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _acceptedPortals = new List<GameObject>();
        _sentPortals = new List<GameObject>();
        // _sendPosition = new Vector3(_pairedPortal.transform.position.x, _pairedPortal.transform.position.y - 0.5f, 0);
        _sendPosition = _pairedPortal.transform.position;
    }

    private List<GameObject> _sentPortals;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject thing = other.transform.parent.transform.parent.gameObject;
        if (_acceptedPortals.Contains(thing) || _sentPortals.Contains(thing))
            return;

        _sentPortals.Add(thing);
        SendPortal(thing);
        Vector3 offset = thing.transform.position - transform.position;
        thing.transform.position = _sendPosition + offset;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject thing = other.transform.parent.transform.parent.gameObject;
        if (_acceptedPortals.Contains(thing))
            _acceptedPortals.Remove(thing);
        if (_sentPortals.Contains(thing))
            _sentPortals.Remove(thing);
    }
    
    private void SendPortal(GameObject thing)
    {
        _pairedPortal.AcceptPortal(thing);
        _anim.SetTrigger("Pop");
    }

    private List<GameObject> _acceptedPortals;
    private void AcceptPortal(GameObject thing)
    {
        _acceptedPortals.Add(thing);
        _anim.SetTrigger("Pop");
    }
}
