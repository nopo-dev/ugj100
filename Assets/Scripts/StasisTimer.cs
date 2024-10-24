using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StasisTimer : MonoBehaviour
{
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        SetInactive();
    }

    private void SetInactive()
    {
        gameObject.SetActive(false);
    }

    public void PauseCountdown()
    {
        _anim.speed = 0f;
    }

    public void UnpauseCountdown()
    {
        _anim.speed = 1f;
    }

    public (int, float, bool) GetAnimatorState()
    {
        return new (_anim.GetCurrentAnimatorStateInfo(0).fullPathHash, _anim.GetCurrentAnimatorStateInfo(0).normalizedTime, false);
    }

    public void SetAnimatorState((int, float, bool) animatorState)
    {
        _anim.Play(animatorState.Item1, 0, animatorState.Item2);
    }
}
