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
}
