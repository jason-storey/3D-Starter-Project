using UnityEngine;
using UnityEngine.InputSystem;

namespace JS
{
    [AddComponentMenu("KCC/Player Input (KCC - Simple)")]
    public class KCCInputNew : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        KCCPlayer _player;

        [SerializeField]
        PlayerData _data;


        void UpdatePlayer(CharacterInputs obj) => _player.UpdateInput(obj);

        void OnEnable()
        {
            _data.InputsChanged += UpdatePlayer;

        }

        void ChangePlayerControl() {}

        void OnDisable()
        {
            _data.InputsChanged -= UpdatePlayer;
        }
    }
}