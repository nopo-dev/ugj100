using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityCalculator : MonoBehaviour
{
    private Vector3 _previousPos;
    public bool TrackVelocity { get; set; }

    private Vector2 _velocity;

    public Vector2 Velocity
    {
        get
        {
            return _velocity;
        }
        set
        {
            _velocity = value;
        }
    }

    private void Awake()
    {
        _previousPos = transform.position;
    }

    private void Update()
    {
        if (TrackVelocity)
        {
            _velocity = (transform.position - _previousPos) / Time.deltaTime;
            _previousPos = transform.position;
        }
    }
}
