using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputPoint
{
    public float InputTime;
    public Vector2 MovementInput;
    public bool JumpPressed;
    public bool JumpReleased;

    public InputPoint(float inputTime, Vector2 movementInput, bool jumpPressed, bool jumpReleased)
    {
        InputTime = inputTime;
        MovementInput = movementInput;
        JumpPressed = jumpPressed;
        JumpReleased = jumpReleased;
    }

    public override string ToString()
    {
        return InputTime + " (" + MovementInput.x + ", " + MovementInput.y + ") " + JumpPressed + " " + JumpReleased;
    }

    public bool Equals(InputPoint other)
    {
        return other.MovementInput == MovementInput && other.JumpPressed == JumpPressed &&
            other.JumpReleased == JumpReleased;
    }
}
