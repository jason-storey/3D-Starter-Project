using System;
using JS;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    [SerializeField] Transform _lookingAt;
    [SerializeField] bool _isFirstPerson;

    public bool IsFirstPerson
    {
        get => _isFirstPerson;
        set
        {
            if(_isFirstPerson == value) return;
            _isFirstPerson = value;
            ChangedCameraMode?.Invoke();
        }
    }


    public Transform LookingAt
    {
        get => _lookingAt;
        set
        {
            _lookingAt = value;
            LookingAtChanged?.Invoke(_lookingAt);
        }
    }

    public event Action ChangedCameraMode; 
    public bool IsLookingAtSomething => _lookingAt;
    public event Action<Transform> LookingAtChanged;

    public void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UpdateInput(CharacterInputs characterInputs)
    {
        _inputs = characterInputs;
        InputsChanged?.Invoke(_inputs);
    }

    public event Action<CharacterInputs> InputsChanged;
    
    [SerializeField]
    CharacterInputs _inputs;
}