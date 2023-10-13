using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using static JS.Keys_PlayerStates;

namespace JS
{
    [AddComponentMenu("KCC/Character (KCC)")]
    [RequireComponent(typeof(KCCMotor))]
    public class KCCCharacterController : MonoBehaviour, ICharacterController
    {
        [Header("Dependencies")]
        [SerializeField]
        ControllerSettings _settings;
        [SerializeField]
        public KCCMotor Motor;
        [SerializeField]
        PlayerData _data;
        
        [SerializeField]
        string _currentStateName;
        
        public Transform CameraFollowPoint;


        Dictionary<string, CharacterControllerState> _states;

        CharacterControllerState _previousState;
        CharacterControllerState _currentState;

        public void SetState(string state)
        {
            var targetState = _states[state];
            if (_currentState == targetState) return;
            _previousState = _currentState;
            _previousState?.ExitingState(state);
            _currentState = targetState;
            _currentState.EnteringState(_previousState);
            _currentStateName = state;
        }

        void Awake()
        {
            Motor.CharacterController = this;
            _states = new Dictionary<string, CharacterControllerState> { { DEFAULT, new DefaultCharacterState() }, { GRINDING, new GrindingCharacterState() } };
            SetupStates();
            SetState(DEFAULT);
        }

        void SetupStates()
        {
            foreach (var state in _states) 
                state.Value.SetDependencies(this, Motor, _data, _settings);
        }


        public void SetInputs(ref CharacterInputs inputs) => _currentState.SetInputs(ref inputs);


        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) => 
            _currentState.UpdateRotation(ref currentRotation, deltaTime);

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) => 
            _currentState.UpdateVelocity(ref currentVelocity, deltaTime);


        public void BeforeCharacterUpdate(float deltaTime) => 
            _currentState.BeforeUpdate(deltaTime);

        public void PostGroundingUpdate(float deltaTime) => _currentState.PostGrounding(deltaTime);

        public void AfterCharacterUpdate(float deltaTime) => 
            _currentState.AfterUpdate(deltaTime);

        public bool IsColliderValidForCollisions(Collider coll) => _currentState.IsColliderValidForCollisions(coll);

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) => _currentState.OnGroundHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport) =>  _currentState.OnMovementHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) =>
            _currentState.ProcessHitStabilityReport(hitCollider, hitNormal, hitPoint, transform.position, transform.rotation, ref hitStabilityReport);

        public void OnDiscreteCollisionDetected(Collider hitCollider) => _currentState.OnDiscreteCollisionDetected(hitCollider);
    }
}