using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    
    private Animator _anim;
    private Collider2D _coll;

    public bool IsOpen { get; set; }


    private void Awake()
    {
        _coll = GetComponent<Collider2D>();
        _anim = GetComponent<Animator>();
    }

    public void OpenDoor()
    {
        IsOpen = true;
        _anim.SetBool("Open", true);
    }

    public void CloseDoor()
    {
        _anim.SetBool("Open", false);
    }

    private void DisableCollider()
    {
        _coll.enabled = false;
    }

    private void EnableCollider()
    {
        _coll.enabled = true;
    }
}
