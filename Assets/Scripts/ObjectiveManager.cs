using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{

    [SerializeField] private List<GameObject> _goals;
    [SerializeField] private GameObject _flag;
    [SerializeField] private List<GameObject> _boxes;
    [SerializeField] private GameObject _player;
    [SerializeField] private float _lifetime = 10f;
    [SerializeField] private GameObject _mirrorPrefab;


    private float _lifeTimer;
    public bool TimerActive { get; set; }
    private int _currentGoal;

    private void Start()
    {
        for (int i = 0; i < _goals.Count - 1; i++)
        {
            _goals[i].GetComponent<Goal>().GoalNum = i;
        }
        _currentGoal = 0;
        _goals[_currentGoal].SetActive(true);

        _boxStartPositions = new List<Vector3>();
        foreach (GameObject box in _boxes)
        {
            _boxStartPositions.Add(box.transform.position);
        }

        _pointsInTime = new List<InputPoint>();
        _mirrorPoints = new List<List<InputPoint>>();
        _mirrors = new List<GameObject>();
    }

    public void CurrentGoalTouched(int goalNum)
    {
        if (goalNum == _currentGoal)
        {
            SpawnFlag();
            NextGoal();
            KillPlayer();
            ClearMirrorMemory();
        }
    }

    private void ClearMirrorMemory()
    {
        _pointsInTime = new List<InputPoint>();
        _mirrorPoints = new List<List<InputPoint>>();
        foreach (GameObject mirror in _mirrors)
        {
            Destroy(mirror);
        }
        _mirrors = new List<GameObject>();
    }

    private void SpawnFlag()
    {
        _flag.transform.position = _goals[_currentGoal].transform.position;
        _flag.GetComponent<Animator>().SetTrigger("Spawn");
    }

    private void NextGoal()
    {
        if (_currentGoal < _goals.Count - 1)
            _currentGoal++;
        _goals[_currentGoal].SetActive(true);
    }

    public void KillPlayer()
    {
        if (!TimerActive)
            return;
        _player.transform.position = _flag.transform.position;
        StopAllCoroutines();
        ReturnBoxes();
        ReturnMirrors();
        _pointsInTime.Add(new InputPoint(_lifeTimer, new Vector2(0f, 0f), false, false));
        _mirrorPoints.Add(_pointsInTime);

        _pointsInTime = new List<InputPoint>();
        _lifeTimer = 0f;
        TimerActive = false;
    }

    private void ReturnMirrors()
    {
        foreach (GameObject mirror in _mirrors)
        {
            mirror.SetActive(false);
            mirror.transform.position = _player.transform.position;
        }
    }

    private List<Vector3> _boxStartPositions;
    private void ReturnBoxes()
    {
        for (int i = 0; i < _boxes.Count; i++)
        {
            _boxes[i].transform.position = _boxStartPositions[i];
        }
    }


    private bool _jumpPress;
    private bool _jumpRelease;
    private void Update()
    {
        if (InputManager.JumpPressed)
            _jumpPress = true;
        if (InputManager.JumpReleased)
            _jumpRelease = true;
    }

    private List<InputPoint> _pointsInTime;
    private List<List<InputPoint>> _mirrorPoints;
    private List<GameObject> _mirrors;
    private InputPoint _activeInput;
    private void FixedUpdate()
    {
        if (TimerActive) // timer running
        {
            _lifeTimer += Time.fixedDeltaTime;
        }
        else if (InputManager.Movement != Vector2.zero || InputManager.JumpPressed) // start and add current active input
        {
            TimerActive = true;
            _activeInput = new InputPoint(_lifeTimer, InputManager.Movement, _jumpPress, _jumpRelease);
            _jumpPress = false;
            _jumpRelease = false;
            _pointsInTime.Add(_activeInput);

            // replay mirrors
            for (int i = 0; i < _mirrors.Count; i++)
            {
                 StartCoroutine(MoveMirror(_mirrors[i], _mirrorPoints[i]));
            }

            GameObject nextMirror = Instantiate(_mirrorPrefab);
            _mirrors.Add(nextMirror);
            nextMirror.SetActive(false);
        }

        if (_lifeTimer > _lifetime)
        {
            KillPlayer();
        }
        else if (_lifeTimer > 0f)
        {
            // compare current input to active input and add if not same
            InputPoint currentInput = new InputPoint(_lifeTimer, InputManager.Movement, _jumpPress, _jumpRelease);

            if (!currentInput.Equals(_activeInput))
            {
                _pointsInTime.Add(currentInput);
                _activeInput = currentInput;
                _jumpPress = false;
                _jumpRelease = false;
            }
        }
    }

    private IEnumerator MoveMirror(GameObject mirror, List<InputPoint> mirrorInputs)
    {
        mirror.transform.position = _player.transform.position;
        mirror.SetActive(true);
        MirrorController mController = mirror.GetComponent<MirrorController>();

        for (int i = 0; i < mirrorInputs.Count; i++)
        {
            mController.SetInput(mirrorInputs[i]);
            if (i < mirrorInputs.Count - 1)
                yield return new WaitForSeconds(mirrorInputs[i + 1].InputTime - mirrorInputs[i].InputTime);
        }
    }
}
