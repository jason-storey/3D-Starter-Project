using KinematicCharacterController;
using UnityEngine;
namespace JS
{
    [AddComponentMenu("KCC/Player (KCC)")]
    public class KCCPlayer : MonoBehaviour
    {
        [SerializeField]
        KCCCharacterController _character;
        [SerializeField]
        KCCCam _cam;

        [SerializeField]
        PlayerData _player;
        
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _cam.SetFollowTransform(_character.CameraFollowPoint);
            _cam._ignoredColliders.Clear();
            _cam._ignoredColliders.AddRange(_character.GetComponentsInChildren<Collider>());
        }
        
        void LateUpdate()
        {
            if (_cam._rotateWithPhysicsMover && _character.Motor.AttachedRigidbody != null)
            {
                _cam.PlanarDirection = _character.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * _cam.PlanarDirection;
                _cam.PlanarDirection = Vector3.ProjectOnPlane(_cam.PlanarDirection, _character.Motor.CharacterUp).normalized;
            }
            HandleCameraInput();
        }

        public void LockMouse() => Cursor.lockState = CursorLockMode.Locked;
        
        void HandleCameraInput()
        {
            var lookInputVector = new Vector3( _inputs.Look.x, _inputs.Look.y, 0f);
            if (Cursor.lockState != CursorLockMode.Locked) 
                lookInputVector = Vector3.zero;
            float scrollInput = -_inputs.CameraScroll;
            _cam.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);
            if (_inputs.ToggleCameraZoom) ToggleCameraZoom();
            
            _player.IsFirstPerson = _cam.TargetDistance <= 0;
        }

        public void ToggleCameraZoom()
        {
            _cam.TargetDistance =
                (_cam.TargetDistance == 0f) ? _cam._defaultDistance : 0f;
        }

        
        
        public void UpdateInput(CharacterInputs inputs)
        {
            _inputs = inputs;
            inputs.CameraRotation = _cam.Transform.rotation;
            inputs.CameraPosition = _cam.Transform.position;
            inputs.CameraForward = _cam.Transform.forward;
            _character.SetInputs(ref inputs);
        }
        CharacterInputs _inputs;
    }
}