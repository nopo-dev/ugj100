using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    [SerializeField] private ObjectiveManager _objManager;

    private BoxCollider2D _collider;

    public int GoalNum { get; set; }

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider2D>();
    }

    private void GoalTouched()
    {
        _anim.SetTrigger("Touched");
        _objManager.CurrentGoalTouched(GoalNum);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            GoalTouched();
            _collider.enabled = false;
        }
    }

    private void GoalInactive()
    {
        gameObject.SetActive(false);
    }
}
