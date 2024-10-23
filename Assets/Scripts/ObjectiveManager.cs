using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{

    [SerializeField] private List<GameObject> _goals;
    [SerializeField] private List<GameObject> _cameraPositions;
    [SerializeField] private GameObject _flag;
    [SerializeField] private CameraController _camera;
    [SerializeField] private List<LevelInteractables> _interactables;
    [SerializeField] private GameObject _player;
    [SerializeField] private float _lifetime = 10f;
    [SerializeField] private float _stasisTime = 5f;
    [SerializeField] private GameObject _mirrorPrefab;
    [SerializeField] private GameObject _stasisEffect;
    [SerializeField] private GameObject _lifeCountdown;
    [SerializeField] private GameObject _stasisCountdown;

    private float _lifeTimer;
    public bool TimerActive { get; set; }
    public int CurrentLevel = 0;

    private void Start()
    {
        for (int i = 0; i < _goals.Count - 1; i++)
        {
            _goals[i].GetComponent<Goal>().GoalNum = i;
        }
        _goals[CurrentLevel].SetActive(true);
        MoveCamera(CurrentLevel);
        GetBoxStartPositions();

        _pointsInTime = new List<InputPoint>();
        _mirrorPoints = new List<List<InputPoint>>();
        _mirrors = new List<GameObject>();
    }

    private void StasisBubble()
    {
        _stasisEffect.transform.position = _player.transform.position + Vector3.up * 0.5f;
        _stasisEffect.GetComponent<StasisBubble>().Stasis(_stasisTime);
    }

    private void ResetStasisBubble()
    {
        if (_playerStasis)
            _stasisEffect.GetComponent<StasisBubble>().ResetBubble();
    }

    private void MoveCamera(int level)
    {
        _camera.Position = _cameraPositions[level].transform.position;
    }

    private void GetBoxStartPositions()
    {
        _boxStartPositions = new List<Vector3>();
        foreach (GameObject box in _interactables[CurrentLevel].Boxes)
        {
            _boxStartPositions.Add(box.transform.position);
        }
    }

    public void CurrentGoalTouched(int goalNum)
    {
        if (goalNum == CurrentLevel)
        {
            SpawnFlag();
            KillPlayer();
            NextGoal();
            MoveCamera(CurrentLevel);
            ClearMirrorMemory();
            GetBoxStartPositions();
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
        _flag.transform.position = _goals[CurrentLevel].transform.position;
        _flag.GetComponent<Animator>().SetTrigger("Spawn");
    }

    private void NextGoal()
    {
        if (CurrentLevel < _goals.Count - 1)
            CurrentLevel++;
        _goals[CurrentLevel].SetActive(true);
    }

    public void KillPlayer()
    {
        if (!TimerActive)
            return;
        _player.transform.position = _flag.transform.position;
        StopAllCoroutines();
        ReturnBoxes();
        ResetStasises();
        ResetStasisBubble();
        ResetPlayerStasis();
        ReturnMirrors();
        ResetMirrorStasises();
        _pointsInTime.Add(new InputPoint(_lifeTimer, new Vector2(0f, 0f), false, false, 0f));
        _mirrorPoints.Add(_pointsInTime);

        _pointsInTime = new List<InputPoint>();
        _lifeTimer = 0f;
        TimerActive = false;
        _lifeCountdown.GetComponent<Timer>().ResetCountdown();
        _lifeCountdown.GetComponent<Timer>().UnpauseCountdown();
        _stasisCountdown.SetActive(false);
    }

    private void ResetMirrorStasises()
    {
        foreach (GameObject mirror in _mirrors)
        {
            mirror.GetComponent<MirrorController>().Unstasis();
        }
    }

    private void ResetPlayerStasis()
    {
        if (!_playerStasis)
            return;
        _playerStasis = false;
        _stasisCountdown.SetActive(true);
        _player.GetComponent<DudeController>().Unstasis();
    }

    private bool _playerStasis;
    public void StasisPlayer()
    {
        _pointsInTime.Add(new InputPoint(_lifeTimer, InputManager.Movement, _jumpPress, _jumpRelease, _stasisTime));
        _player.GetComponent<DudeController>().Stasis(_stasisTime);
        StasisBubble();
        _stasisCountdown.SetActive(true);
        StartCoroutine(PauseTimers());
    }

    private IEnumerator PauseTimers()
    {
        _playerStasis = true;
        _lifeCountdown.GetComponent<Timer>().PauseCountdown();
        yield return new WaitForSeconds(_stasisTime);
        _playerStasis = false;
        _lifeCountdown.GetComponent<Timer>().UnpauseCountdown();
    }

    private void ResetLevel()
    {
        KillPlayer();
        ClearMirrorMemory();
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
        for (int i = 0; i < _boxStartPositions.Count; i++)
        {
            _interactables[CurrentLevel].Boxes[i].transform.position = _boxStartPositions[i];
        }
    }

    private void ResetStasises()
    {
        foreach (GameObject stasis in _interactables[CurrentLevel].Stasises)
        {
            stasis.GetComponent<Stasis>().Reset();
        }
    }

    private List<InputPoint> _pointsInTime;
    private List<List<InputPoint>> _mirrorPoints;
    private List<GameObject> _mirrors;
    private InputPoint _activeInput;
    private bool _jumpPress;
    private bool _jumpRelease;
    private void Update()
    {
        if (InputManager.ResetPressed)
        {
            StartCoroutine(ResetLevelNextTick());
        }
        if (InputManager.KillSelfPressed)
        {
            StartCoroutine(KillSelfNextTick());
        }
        if (_playerStasis)
            return;
            
        if (InputManager.JumpPressed)
            _jumpPress = true;
        if (InputManager.JumpReleased)
            _jumpRelease = true;

        if (TimerActive) // timer running
        {
            _lifeTimer += Time.deltaTime;
        }
        else if (InputManager.Movement != Vector2.zero || InputManager.JumpPressed) // start and add current active input
        {
            TimerActive = true;
            _lifeCountdown.GetComponent<Timer>().StartCountdown();
            _activeInput = new InputPoint(_lifeTimer, InputManager.Movement, _jumpPress, _jumpRelease, 0f);
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

        if (_lifeTimer > 0f)
        {
            // compare current input to active input and add if not same
            InputPoint currentInput = new InputPoint(_lifeTimer, InputManager.Movement, _jumpPress, _jumpRelease, 0f);

            if (!currentInput.Equals(_activeInput))
            {
                _pointsInTime.Add(currentInput);
                _activeInput = currentInput;
                _jumpPress = false;
                _jumpRelease = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_lifeTimer > _lifetime)
        {
            KillPlayer();
        }
    }

    private IEnumerator ResetLevelNextTick()
    {
        yield return new WaitForFixedUpdate();
        ResetLevel();
    }

    private IEnumerator KillSelfNextTick()
    {
        yield return new WaitForFixedUpdate();
        KillPlayer();
    }

    private IEnumerator MoveMirror(GameObject mirror, List<InputPoint> mirrorInputs)
    {
        // yield return new WaitForFixedUpdate();
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
