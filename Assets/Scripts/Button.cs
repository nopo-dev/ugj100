using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{

    [SerializeField] private Door _connectedDoor;

    private Collider2D _coll;
    private Animator _anim;

    private void Awake()
    {
        _coll = GetComponent<Collider2D>();
        _anim = GetComponent<Animator>();
        _thingsColliding = new List<Collider2D>();
    }

    private List<Collider2D> _thingsColliding;
    private void OnTriggerEnter2D(Collider2D other)
    {
        _thingsColliding.Add(other);
        _anim.SetBool("Pressed", true);
        _connectedDoor.OpenDoor();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _thingsColliding.Remove(other);
        if (_thingsColliding.Count == 0)
        {
            _anim.SetBool("Pressed", false);
            _connectedDoor.CloseDoor();
        }
    }
}
