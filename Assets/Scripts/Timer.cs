using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void StartCountdown()
    {
        _anim.SetTrigger("Start");
    }

    public void ResetCountdown()
    {
        _anim.SetTrigger("Reset");
    }

    public void PauseCountdown()
    {
        _anim.speed = 0f;
    }

    public void UnpauseCountdown()
    {
        _anim.speed = 1f;
    }
}