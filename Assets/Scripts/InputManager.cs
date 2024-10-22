using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;
    public static bool JumpPressed;
    public static bool JumpHeld;
    public static bool JumpReleased;
    public static bool ResetPressed;
    
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _resetAction;

    private void Awake()
    {
        // QualitySettings.vSyncCount = 0;
        // Application.targetFrameRate = 60;
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _resetAction = PlayerInput.actions["Reset"];
    }

    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();

        JumpPressed = _jumpAction.WasPressedThisFrame();
        JumpHeld = _jumpAction.IsPressed();
        JumpReleased = _jumpAction.WasReleasedThisFrame();

        ResetPressed = _resetAction.WasPressedThisFrame();
    }

}
