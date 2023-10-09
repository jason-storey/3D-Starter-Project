using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/GameData", order = 1)]
public class GameData : ScriptableObject
{
    [SerializeField]
    string _inputMode;

    public string InputMode
    {
        get => _inputMode;
        set
        {
            _inputMode = value;
            InputModeChanged?.Invoke();
        }
    }

    public event Action InputModeChanged;
}
