using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rewind : MonoBehaviour
{
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void PlayRewind()
    {
        _anim.Play("rewind wind");
    }

    public void PlayReset()
    {
        _anim.Play("rewind set");
    }

    public void PlayPlay()
    {
        _anim.Play("rewind play");
        _anim.speed = 1f;
    }

    public void SetAnimSpeed(float speed)
    {
        _anim.speed = speed;
    }

    public void ShowRewind()
    {
        GetComponent<SpriteRenderer>().enabled = true;
    }

    public void HideRewind()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }
}
