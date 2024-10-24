using System;
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
    [SerializeField] private float _rewindLength = 3f;

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
        _playerPStates = new List<PositionState>();
        _mirrorPStates = new List<List<PositionState>>();
        _lifeCountdownPStates = new List<PositionState>();
        InitializeBoxPStateList();
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
            StopAllCoroutines();
            ResetStasises();
            ResetStasisBubble();
            ResetPlayerStasis();
            ResetMirrorStasises();

            _player.transform.position = _flag.transform.position;
            ReturnMirrors();

            _lifeTimer = 0f;
            TimerActive = false;
            _lifeCountdown.GetComponent<Timer>().UnpauseCountdown();
            _lifeCountdown.GetComponent<Timer>().ResetCountdown();
            _stasisCountdown.SetActive(false);

            _playerPStates = new List<PositionState>();
            _player.GetComponent<DudeController>().ForwardAnimations();
            _rewinding = false;

            NextGoal();
            MoveCamera(CurrentLevel);
            ClearMirrorMemory();
            GetBoxStartPositions();
            InitializeBoxPStateList();
        }
    }

    private void InitializeBoxPStateList()
    {
        _boxPStates = new List<List<PositionState>>(_interactables[CurrentLevel].Boxes.Count);
        for (int i = 0; i < _interactables[CurrentLevel].Boxes.Count; i++)
        {
            _boxPStates.Add(new List<PositionState>());
        }

    }

    private void ClearMirrorMemory()
    {
        _pointsInTime = new List<InputPoint>();
        _mirrorPoints = new List<List<InputPoint>>();
        _mirrorPStates = new List<List<PositionState>>();
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
        StopAllCoroutines();
        ResetStasises();
        ResetStasisBubble();
        ResetPlayerStasis();
        ResetMirrorStasises();

        StartCoroutine(RewindPlayer());
    }

    private List<PositionState> _playerPStates;
    private float InverseSmoothStep(float time)
    {
        float x = time / _lifeTimer;
        return (x + (x - (x * x * (3f - 2f * x)))) * _rewindLength;
    }

    private bool _rewinding;
    private IEnumerator RewindPlayer()
    {
        _rewinding = true;

        if (TimerActive)
        {
            _player.GetComponent<DudeController>().ReverseAnimations();
            foreach (GameObject mirror in _mirrors)
            {
                mirror.GetComponent<MirrorController>().ReverseAnimations();
            }

            int playerPStateIndex = _playerPStates.Count - 1;
            for (float time = _rewindLength; time >= 0; time -= Time.deltaTime)
            {
                for (int i = playerPStateIndex; i >= 0; i--)
                {
                    if (InverseSmoothStep(_playerPStates[i].TimeInLife) <= time)
                    {
                        _player.transform.position = _playerPStates[i].Position;
                        _player.GetComponent<DudeController>().SetAnimatorState(_playerPStates[i].AnimatorState);

                        // handle mirrors here also
                        for (int m = 0; m < _mirrorPStates.Count; m++)
                        {
                            if (i >= _mirrorPStates[m].Count)
                                break;
                            _mirrors[m].transform.position = _mirrorPStates[m][i].Position;
                            _mirrors[m].GetComponent<MirrorController>().SetAnimatorState(_mirrorPStates[m][i].AnimatorState);
                        }

                        // handle boxes here too
                        for (int b = 0; b < _boxPStates.Count; b++)
                        {
                            if (i >= _boxPStates[b].Count)
                                break;
                            _interactables[CurrentLevel].Boxes[b].transform.position = _boxPStates[b][i].Position;
                        }

                        // handle lifecountdown ui
                        _lifeCountdown.GetComponent<Timer>().SetAnimatorState(_lifeCountdownPStates[i].AnimatorState);

                        playerPStateIndex = i;
                        break;
                    }
                }
                yield return null;
            }
        }        
        
        _player.transform.position = _flag.transform.position;
        ReturnMirrors();
        ReturnBoxes();

        _pointsInTime.Add(new InputPoint(_lifeTimer, new Vector2(0f, 0f), false, false, 0f));
        _mirrorPoints.Add(_pointsInTime);
        _mirrorPStates = new List<List<PositionState>>(_mirrorPoints.Count);
        for (int i = 0; i < _mirrorPoints.Count; i++)
        {
            _mirrorPStates.Add(new List<PositionState>());
        }
        InitializeBoxPStateList();

        _pointsInTime = new List<InputPoint>();
        _lifeTimer = 0f;
        TimerActive = false;
        _lifeCountdown.GetComponent<Timer>().UnpauseCountdown();
        _lifeCountdown.GetComponent<Timer>().ResetCountdown();
        _lifeCountdownPStates = new List<PositionState>();

        _playerPStates = new List<PositionState>();
        _player.GetComponent<DudeController>().ForwardAnimations();
        foreach (GameObject mirror in _mirrors)
        {
            mirror.GetComponent<MirrorController>().ForwardAnimations();
        }
        
        if (_clearMirrorsOnKillPlayer)
        {
            ClearMirrorMemory();
            _clearMirrorsOnKillPlayer = false;
        }
        _rewinding = false;
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
        _stasisCountdown.SetActive(false);
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

    private bool _clearMirrorsOnKillPlayer;
    private void ResetLevel()
    {
        _clearMirrorsOnKillPlayer = true;
        KillPlayer();
    }

    private void ReturnMirrors()
    {
        foreach (GameObject mirror in _mirrors)
        {
            mirror.SetActive(false);
            mirror.transform.position = _flag.transform.position;
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
    private List<List<PositionState>> _mirrorPStates;
    private List<PositionState> _lifeCountdownPStates;
    private List<List<PositionState>> _boxPStates;
    private List<GameObject> _mirrors;
    private InputPoint _activeInput;
    private bool _jumpPress;
    private bool _jumpRelease;
    private void Update()
    {
        if (_rewinding)
            return;
        if (InputManager.ResetPressed)
        {
            StartCoroutine(ResetLevelNextTick());
        }
        if (InputManager.KillSelfPressed && TimerActive)
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
            _playerPStates.Add(new PositionState(_lifeTimer, _player.transform.position, _player.GetComponent<DudeController>().GetAnimatorState()));
            for (int i = 0; i < _mirrorPStates.Count; i++)
            {
                _mirrorPStates[i].Add(new PositionState(_lifeTimer, _mirrors[i].transform.position, _mirrors[i].GetComponent<MirrorController>().GetAnimatorState()));
            }
            for (int i = 0; i < _interactables[CurrentLevel].Boxes.Count; i++)
            {
                _boxPStates[i].Add(new PositionState(_lifeTimer, _interactables[CurrentLevel].Boxes[i].transform.position));
            }
            _lifeCountdownPStates.Add(new PositionState(_lifeTimer, _lifeCountdown.GetComponent<Timer>().GetAnimatorState()));
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
        if (_rewinding)
            return;

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
