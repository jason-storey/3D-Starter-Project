using UnityEngine;
namespace LS
{
    public class InputHandlingTriggerBox : MonoBehaviour
    {
        [SerializeField]
        Color _colorInactive;
        [SerializeField]
        Color _colorActive;
        
        [SerializeField]
        PlayerData _playerData;

        bool _playerInside;

        bool _controlsEnabled;
        [SerializeField]
        string _actionMap = "Interaction";
        
        TriggerControls _controls;
        void OnEnable()
        {
            _controls = GetComponent<TriggerControls>();
            _controls.SetPlayerData(_playerData);
        }

        void Update()
        {
            if (!_playerInside) return;
            if (!_controlsEnabled && _playerData.Inputs.Pressed("Interact"))
            {
                _playerData.Inputs.SetMode(_actionMap);
                _controlsEnabled = true;
                _controls.OnActivated();
            }

            if (!_controlsEnabled) return;
            if (_controls.ShouldExit)
            {
                _playerData.Inputs.RestoreMode();
                _controlsEnabled = false;
                _controls.OnDeActivated();
                return;
            }
            
            _controls.WhileInside();
        }


        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInside = true;
                _controls.OnEntered();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = false;
            _controls.OnDeActivated();
        }

        void OnDrawGizmos()
        {
            var t = transform;
            var scale = t.localScale;
            var position = t.position;
            Gizmos.color = _controlsEnabled ? _colorActive : _colorInactive;
            Gizmos.DrawCube(position, scale);
            Gizmos.color = Color.green;
            if (_playerInside)
                Gizmos.DrawWireCube(position, scale);
        }
    }

    public interface TriggerControls
    {
        void OnActivated();
        void WhileInside();
        void OnDeActivated();
        bool ShouldExit { get; }
        void SetPlayerData(PlayerData playerData);
        void OnEntered();
        void OnExited();
    }
    
}