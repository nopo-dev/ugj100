using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{

    [SerializeField] private List<GameObject> _goals;
    [SerializeField] private GameObject _flag;

    private int _currentGoal;

    private void Start()
    {
        for (int i = 0; i < _goals.Count - 1; i++)
        {
            _goals[i].GetComponent<Goal>().GoalNum = i;
        }
        _currentGoal = 0;
        _goals[_currentGoal].SetActive(true);
    }

    public void CurrentGoalTouched(int goalNum)
    {
        if (goalNum == _currentGoal)
        {
            SpawnFlag();
            NextGoal();
        }
    }

    private void SpawnFlag()
    {
        Debug.Log(_currentGoal);
        Debug.Log(_goals[_currentGoal].transform.position.x);
        _flag.transform.position = _goals[_currentGoal].transform.position;
        _flag.GetComponent<Animator>().SetTrigger("Spawn");
    }

    private void NextGoal()
    {
        if (_currentGoal < _goals.Count - 1)
            _currentGoal++;
        _goals[_currentGoal].SetActive(true);
    }

}
