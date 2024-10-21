using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 StartPos
    {
        get { return _startPos; }
        set { _startPos = value; }
    }
    public Vector3 EndPos
    {
        get { return _endPos; }
        set { _endPos = value; }
    }
    public bool Move;

    [SerializeField] private Vector3 _startPos;
    [SerializeField] private Vector3 _endPos;
    [SerializeField] private float _moveTime;

    private VelocityCalculator _velocityCalc;
    private bool _direction = true;
    private float _moveTimer;

    private void Awake()
    {
        _velocityCalc = GetComponent<VelocityCalculator>();
        _velocityCalc.TrackVelocity = true;
    }

    private void Update()
    {
        if (!Move)  return;

        _moveTimer += Time.fixedDeltaTime;
        Debug.Log(_velocityCalc.Velocity);

        if (_direction)
            transform.position = Vector3.Lerp(StartPos, EndPos, _moveTimer / _moveTime);
        else
            transform.position = Vector3.Lerp(EndPos, StartPos, _moveTimer / _moveTime);

        if (_moveTimer >= _moveTime)
        {
            _moveTimer = 0f;
            _direction = !_direction;
        }
    }
}
