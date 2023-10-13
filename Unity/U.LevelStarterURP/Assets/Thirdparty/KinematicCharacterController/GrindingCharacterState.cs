using UnityEngine;
using static JS.Keys_PlayerStates;

namespace JS
{
    public class GrindingCharacterState : CharacterControllerState
    {
        
        bool _shouldExit;
        Vector3 _playerInput;
        Vector3 _cameraForward;

        bool _directionLocked;
        public override void SetInputs(ref CharacterInputs inputs)
        {
            if (inputs.GrindDown) _shouldExit = true;
            if (_directionLocked || !(_playerInput.magnitude > 0.2f)) return;
            _directionLocked = true;
            CalculateCameraProjections(ref inputs, out _playerInput, out _cameraForward);
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (!_directionLocked) return;
            
            currentVelocity = _cameraForward * Settings.Speed;
            currentVelocity.y = 0;
        }


        public override void OnDiscreteCollisionDetected(Collider hitCollider) => 
            _shouldExit = true;

        public override void AfterUpdate(float delta)
        {
            if(_shouldExit)
                SetState(DEFAULT);
        }

        public override void ExitingState(string nextState)
        {
            _shouldExit = false;
            _directionLocked = false;
        }
    }
}