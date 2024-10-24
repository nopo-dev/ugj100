using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionState
{
    
    public float TimeInLife;
    public Vector3 Position;
    public (int, float, bool) AnimatorState;

    public PositionState(float timeInLife, Vector3 position, (int, float, bool) animatorState)
    {
        TimeInLife = timeInLife;
        Position = position;
        AnimatorState = animatorState;
    }

    public PositionState(float timeInLife, Vector3 position)
    {
        TimeInLife = timeInLife;
        Position = position;
    }

    public PositionState(float timeInLife, (int, float, bool) animatorState)
    {
        TimeInLife = timeInLife;
        AnimatorState = animatorState;
    }
}
