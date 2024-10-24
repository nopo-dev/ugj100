using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Rewind _rewind;
    [SerializeField] private MirrorCount _mirrorCount;
    [SerializeField] private float _rewindLength = 3f;

    private float _lifeTimer;
    public bool TimerActive { get; set; }
    public int CurrentLevel = 0;

    private void Start()
    {
        for (int i = 0; i < _goals.Count; i++)
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

        _rewind.HideRewind();
        //_rewind.PlayPlay();

        _bigPlayerPStates = new List<List<PositionState>>();
        _bigMirrorPStates = new List<List<List<PositionState>>>();
        _bigBoxPStates = new List<List<List<PositionState>>>();
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

            // add pstates to big lists before they get cleared
            AddPStatesToBigLists();

            _player.transform.position = _flag.transform.position;
            ReturnMirrors();

            _lifeTimer = 0f;
            TimerActive = false;
            _lifeCountdown.GetComponent<Timer>().UnpauseCountdown();
            _lifeCountdown.GetComponent<Timer>().ResetCountdown();
            _lifeCountdownPStates = new List<PositionState>();
            _stasisCountdown.SetActive(false);

            _playerPStates = new List<PositionState>();
            _player.GetComponent<DudeController>().ForwardAnimations();
            _rewinding = false;

            _mirrorCount.RemoveAllCounters();
            ClearMirrorMemory();
            NextGoal();
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

    private int _maxMirrorsUsed;
    private void ClearMirrorMemory()
    {
        if (_mirrors.Count > _maxMirrorsUsed)
            _maxMirrorsUsed = _mirrors.Count;

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

    private bool _lastGoalTouched;
    private void NextGoal()
    {
        if (CurrentLevel + 1 < _goals.Count)
        {
            CurrentLevel++;
            _goals[CurrentLevel].SetActive(true);
            MoveCamera(CurrentLevel);
            GetBoxStartPositions();
            InitializeBoxPStateList();
            return;
        }
        
        if (_lastGoalTouched == false)
        {
            LastGoalTouched();
            _lastGoalTouched = true;
        }
    }

    private List<List<PositionState>> _bigPlayerPStates;
    private List<List<List<PositionState>>> _bigMirrorPStates;
    private List<List<List<PositionState>>> _bigBoxPStates;

    private void LastGoalTouched()
    {
        StartCoroutine(BigRewind());
    }

    [SerializeField]private float _bigRewindLength = 5f;
    private IEnumerator BigRewind()
    {
        _rewinding = true;
        _bigRewinding = true;
        _deathCounter.DisplayNumber();
        _mirrorCount.RemoveAllCounters();
        _lifeCountdown.GetComponent<SpriteRenderer>().enabled = false;
        _stasisCountdown.GetComponent<SpriteRenderer>().enabled = false;
        _rewind.ShowRewind();
        _rewind.PlayReset();

        for (int i = _mirrors.Count; i < _maxMirrorsUsed; i++)
        {
            _mirrors.Add(Instantiate(_mirrorPrefab, new Vector3(-10f, -10f, 0f), Quaternion.identity));
        }

        _player.GetComponent<DudeController>().ReverseAnimations();
        foreach (GameObject mirror in _mirrors)
        {
            mirror.GetComponent<MirrorController>().ReverseAnimations();
        }

        // whats the idea here
        // iterate through bigplayerpstatesbackwards (list of lists)
        // for each list in bigpstates do the rewind
        int levelIndex = _bigPlayerPStates.Count - 1;
        int totalDeaths = _deathCount;
        int playerPStateIndex = _bigPlayerPStates.LastOrDefault().Count - 1;
        float maxLifeTimer = _bigPlayerPStates.LastOrDefault().LastOrDefault().TimeInLife;
        for (float time = _bigRewindLength; time >= 0; time -= Time.deltaTime)
        {
            _rewind.SetAnimSpeed(QuadraticSmoothing(time, _bigRewindLength));
            if ((int) (time / _bigRewindLength * totalDeaths) < _deathCount)
            {
                _deathCount--;
                _deathCounter.UpdateNumber(_deathCount);
            }

            _playerPStates = _bigPlayerPStates[levelIndex];
            _mirrorPStates = _bigMirrorPStates[levelIndex];
            _boxPStates = _bigBoxPStates[levelIndex];
            playerPStateIndex = _playerPStates.Count - 1;

            for (int i = playerPStateIndex; i >= 0; i--)
            {
                if (InverseSmoothStep(_playerPStates[i].TimeInLife, maxLifeTimer, _bigRewindLength) <= time)
                {
                    _player.transform.position = _playerPStates[i].Position;
                    _player.GetComponent<DudeController>().SetAnimatorState(_playerPStates[i].AnimatorState);

                    // handle mirrors here also
                    for (int m = 0; m < _mirrorPStates.Count; m++)
                    {
                        if (i >= _mirrorPStates[m].Count)
                            break;
                        _mirrors[m].transform.position = _mirrorPStates[m][i].Position;
                        _mirrors[m].SetActive(true);
                        _mirrors[m].GetComponent<MirrorController>().SetAnimatorState(_mirrorPStates[m][i].AnimatorState);
                    }

                    // handle boxes here too
                    for (int b = 0; b < _boxPStates.Count; b++)
                    {
                        if (i >= _boxPStates[b].Count)
                            break;
                        _interactables[levelIndex].Boxes[b].transform.position = _boxPStates[b][i].Position;
                    }

                    playerPStateIndex = i;
                    break;
                }
                if (i == 0)
                {
                    levelIndex--;
                    if (levelIndex >= 0)
                        MoveCamera(levelIndex);
                    break;
                }
            }
            if (levelIndex == -1)   break;
            yield return null;
        }
        _player.transform.position = new Vector3(-4f, 0f, 0f);
        ClearMirrorMemory();
        _gameFinished = true;
        StartCoroutine(RollCredits());
    }

    [SerializeField] private GameObject _blackScreen;
    [SerializeField] private GameObject _titleText;
    [SerializeField] private GameObject _creditText;
    private IEnumerator RollCredits()
    {
        yield return new WaitForSeconds(3f);
        _deathCounter.GetComponent<DeathCounter>().HideNumber();
        _blackScreen.SetActive(true);

        yield return new WaitForSeconds(2f);
        _titleText.SetActive(true);

        yield return new WaitForSeconds(5f);
        _creditText.SetActive(true);
    }

    private bool _gameFinished;

    // at the end of each level, add playerpstates, list mirrorpstates, boxpstates to biglists
    private void AddPStatesToBigLists()
    {
        // make sure to add old player max timeinlife to lists being added
        float previousPlayerTime = 0f;
        if (_bigPlayerPStates.Count > 0)
        {
            previousPlayerTime = _bigPlayerPStates.LastOrDefault().LastOrDefault().TimeInLife;
        }

        _bigPlayerPStates.Add(AdjustTime(_playerPStates, previousPlayerTime));

        List<List<PositionState>> temp = new List<List<PositionState>>();
        foreach (List<PositionState> pState in _mirrorPStates)
        {
            temp.Add(AdjustTime(pState, previousPlayerTime));
        }
        _bigMirrorPStates.Add(temp);

        temp = new List<List<PositionState>>();
        foreach (List<PositionState> pState in _boxPStates)
        {
            temp.Add(AdjustTime(pState, previousPlayerTime));
        }
        _bigBoxPStates.Add(_boxPStates);
    }

    private List<PositionState> AdjustTime(List<PositionState> pStates, float timeOffset)
    {
        List<PositionState> adjustedPStates = new List<PositionState>();
        foreach (PositionState pState in pStates)
        {
            adjustedPStates.Add(new PositionState(pState.TimeInLife + timeOffset, pState.Position, pState.AnimatorState));
        }
        return adjustedPStates;
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

    private int _deathCount = 0;
    [SerializeField] private DeathCounter _deathCounter;

    private bool _bigRewinding;
    private float QuadraticSmoothing(float time, float rewindLength)
    {
        float x = time / rewindLength;
        // lifetimer / lifetime = scalar to determine rewind animation speed
        // equivalent to how long you took over how much time you had
        // rewinds faster when you took more time out of available time

        float result = -4f * x * x + 4f * x;
        if (!_bigRewinding) result *= _lifeTimer / _lifetime;
        return result;
    }

    private float InverseSmoothStep(float time, float lifeTimer, float rewindLength)
    {
        float x = time / lifeTimer;
        return (x + (x - (x * x * (3f - 2f * x)))) * rewindLength;
    }

    private bool _rewinding;
    private List<PositionState> _playerPStates;
    private IEnumerator RewindPlayer()
    {
        _rewinding = true;
        _deathCounter.DisplayNumber();
        _mirrorCount.ShowMirrorCount();
        _rewind.ShowRewind();
        int mirrorCounters = 0;
        if (_clearMirrorsOnKillPlayer)
        {
            _rewind.PlayReset();
            mirrorCounters = _mirrorCount.GetMirrorCount();
        }
        else    _rewind.PlayRewind();

        _player.GetComponent<DudeController>().ReverseAnimations();
        foreach (GameObject mirror in _mirrors)
        {
            mirror.GetComponent<MirrorController>().ReverseAnimations();
        }

        bool updatedDeath = false;
        int playerPStateIndex = _playerPStates.Count - 1;
        int mirrorCountIndex = mirrorCounters;
        for (float time = _rewindLength; time >= 0; time -= Time.deltaTime)
        {
            _rewind.SetAnimSpeed(QuadraticSmoothing(time, _rewindLength));
            if (time <= 0.66f * _rewindLength && !updatedDeath)
            {
                updatedDeath = true;
                _deathCount++;
                _deathCounter.UpdateNumber(_deathCount);
                if (!_clearMirrorsOnKillPlayer) _mirrorCount.AddCounter();
            }
            if (_clearMirrorsOnKillPlayer && mirrorCounters > 0)
            {
                if ((int) (time / _rewindLength * mirrorCounters) < mirrorCountIndex)
                {
                    _mirrorCount.RemoveCounter();
                    mirrorCountIndex--;
                }
            }
            for (int i = playerPStateIndex; i >= 0; i--)
            {
                if (InverseSmoothStep(_playerPStates[i].TimeInLife, _lifeTimer, _rewindLength) <= time)
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
        _rewind.HideRewind();
        //_rewind.PlayPlay();
        _deathCounter.DelayHideNumber();
        _mirrorCount.DelayHideMirrorCount();
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
        if (_rewinding || _gameFinished)
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
            _deathCounter.HideNumber();
            _deathCounter.StopAllCoroutines();
            _mirrorCount.HideMirrorCount();
            _mirrorCount.StopAllCoroutines();
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
        if (_rewinding || _gameFinished)
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
