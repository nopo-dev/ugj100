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
        StartCoroutine(ResetTriggers());
    }

    public void ResetCountdown()
    {
        _anim.SetTrigger("Reset");
        StartCoroutine(ResetTriggers());
    }

    private IEnumerator ResetTriggers()
    {
        yield return null;
        _anim.ResetTrigger("Start");
        _anim.ResetTrigger("Reset");
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