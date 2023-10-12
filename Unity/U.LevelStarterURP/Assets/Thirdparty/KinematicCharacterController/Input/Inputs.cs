using System;
using System.Collections.Generic;
using JS;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inputs : MonoBehaviour
{
    [SerializeField]
    PlayerInput _input;
   
    [SerializeField]
    PlayerData _data;
    [SerializeField]
    GameData _gameData;

    [SerializeField]
    string _mapMode;
    
    void OnEnable()
    {
        _input.onActionTriggered += ActionWasTriggered;
        _data.Inputs = this;
        SetInitialInputMap();
        _instance = this;
    }

    public PlayerInput InputHandler => _input;

    void SetInitialInputMap()
    {
        _mapMode = _input.currentActionMap.name;
        _inputModes.Push(_mapMode);
        _playerMode = _mapMode == "Player";
    }

    [SerializeField]
    bool _logInputs = true;
    [SerializeField]
    bool _onlyLogPerformed = true;
    
    void ActionWasTriggered(InputAction.CallbackContext obj)
    {
        var map = obj.action.actionMap.name;
        if(_logInputs && IsAButton(obj) && (!_onlyLogPerformed || obj.performed))
            Debug.Log($"{map}.{obj.action.name}: {obj.phase}");
        if (!obj.performed) return;
        HandleMapSwitching(obj);
        ActionTriggered?.Invoke(map,obj.action.name,obj);
    }

    bool IsAButton(InputAction.CallbackContext callbackContext) => callbackContext.action.type == InputActionType.Button;

    void HandleMapSwitching(InputAction.CallbackContext obj)
    {
        if (!obj.performed) return;
        if (obj.action.name == "ShowTerminal") SwitchToTerminal();
        if (obj.action.name == "CloseTerminal") RestoreMode();
    }

    public event Action<string,string,InputAction.CallbackContext> ActionTriggered; 

    void OnDisable() => _input.onActionTriggered -= ActionWasTriggered;

    public void SwitchToPlayer() => SetMode("Player");
    public void SwitchToTerminal() => SetMode("Terminal");
    public void SwitchToUi() => SetMode("Ui");

    Stack<string> _inputModes   = new();
    bool _playerMode;
    public void SetMode(string mode)
    {
        var current = _input.currentActionMap.name;
        if(_inputModes.Count == 0 || _inputModes.Peek() != mode) _inputModes.Push(current);
        _input.SwitchCurrentActionMap(mode);
        _mapMode = _input.currentActionMap.name;
        _playerMode = mode == "Player";
        
    }
    
    public void RestoreMode()
    {
        if(_inputModes.Count == 0) return;
        var mode = _inputModes.Pop();
        _input.SwitchCurrentActionMap(mode);
        _mapMode = _input.currentActionMap.name;
        _playerMode = mode == "Player";
    }

    bool _couldControlPlayer;

    private static Inputs _instance;
    public static Inputs Instance => _instance;

    [SerializeField]
    bool _shouldLockMouse = false;
    void Update()
    {
        if (!_playerMode)
        {
            _data.UpdateInput(new CharacterInputs());
            return;
        }

        if(_shouldLockMouse && Mouse.current.leftButton.wasPressedThisFrame) _data.LockMouse();
        var p = _input.actions;
        _data.UpdateInput(new CharacterInputs
        {
            Look = p["Look"].ReadValue<Vector2>(),
            Move = p["Move"].ReadValue<Vector2>(),
            JumpDown = p["Jump"].triggered,
            CrouchDown = p["Crouch"].WasPerformedThisFrame(),
            CrouchUp = p["Crouch"].WasReleasedThisFrame(),
            ToggleCameraZoom = p["CameraZoom"].triggered,
            CameraScroll = p["CameraScroll"].ReadValue<float>()
        });
    }

    public bool Pressed(string action)
    {
        var a = _input.actions[action];
        return a.triggered;
    }
}
