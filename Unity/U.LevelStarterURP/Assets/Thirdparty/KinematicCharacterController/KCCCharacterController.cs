using System;
using KinematicCharacterController;
using UnityEngine;

namespace JS
{
    [AddComponentMenu("KCC/Character (KCC)")]
    [RequireComponent(typeof(KCCMotor))]
    public class KCCCharacterController : MonoBehaviour, ICharacterController
    {
        public KCCMotor Motor;

        [SerializeField]
        PlayerData _data;

        [SerializeField]
        float _jumpHeight = 4;

        [SerializeField]
        float Gravity = 30;
        public float FallMultiplier = 2.5f;
        public float Drag = 0.1f;

        public float Speed = 10;
        public float MovementSharpness = 10;


        public int _allowedJumps;
        public int _usedJumps;
        bool _jumpRequested;

        void Awake() => Motor.CharacterController = this;

        int counter;
        public bool _jumpHeld;

        public float RotateSharpness = 10f;
        
        Vector3 _look;
        public void SetInputs(ref CharacterInputs inputs)
        {
            _look = inputs.Look;
            _jumpHeld = inputs.JumpHeld;
            CalculateCameraForward(ref inputs);
          if(inputs.JumpDown)
              _jumpRequested = true;
        }

        Vector3 _playerInput;
        Vector3 _cameraForward;

        void CalculateCameraForward(ref CharacterInputs inputs)
        {
            _playerInput = Vector3.ClampMagnitude(new Vector3(inputs.Move.x, 0f, inputs.Move.y), 1f);
            _cameraForward = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (_cameraForward.sqrMagnitude == 0f)
                _cameraForward = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
        }

        public Transform CameraFollowPoint;


        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            currentRotation = Quaternion.Slerp(currentRotation, Quaternion.LookRotation(_cameraForward, Motor.CharacterUp), deltaTime * RotateSharpness);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            
            ApplyMovementInput(ref currentVelocity, deltaTime);
            TryToJump(ref currentVelocity);

            if (OnTheGround)
                _usedJumps = 0;
            else
                ApplyGravity(ref currentVelocity, deltaTime);
        }

        void ApplyGravity(ref Vector3 currentVelocity, float deltaTime)
        {
            var gravityToApply = Gravity;
            if(OutOfJumps && !_jumpHeld || IsFalling)
                gravityToApply *= FallMultiplier;
            
            currentVelocity -= gravityToApply * deltaTime * Motor.CharacterUp;
            currentVelocity *= 1f / (1f + Drag * deltaTime);
        }


        Vector3 _inputRelativeToCamera;
        void ApplyMovementInput(ref Vector3 currentVelocity, float deltaTime)
        {
            var moveInputVector =
                Quaternion.LookRotation(_cameraForward, Motor.CharacterUp) * _playerInput;

            _inputRelativeToCamera = moveInputVector;
            Vector3 inputRight = Vector3.Cross(moveInputVector, Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized *
                                      moveInputVector.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * Speed;

            if (!OnTheGround)
            {
                Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity,
                    Motor.CharacterUp);
                currentVelocity += velocityDiff * (1f - Mathf.Exp(-MovementSharpness * deltaTime));
                return;
            }
            else
            {
                currentVelocity =
                    Vector3.Lerp(currentVelocity, targetMovementVelocity,
                        1f - Mathf.Exp(-MovementSharpness * deltaTime));
            }
        }

        public float AmountOfAirControl = 15f;
        public float AmountOfForwardToAddToJump = 10;
        
        void TryToJump(ref Vector3 currentVelocity)
        {
          if (!_jumpRequested || OutOfJumps) return;
          _jumpRequested = false;
          _usedJumps++;
          Motor.ForceUnground();
            currentVelocity += (Motor.CharacterUp * _jumpHeight) -
                               Vector3.Project(currentVelocity, Motor.CharacterUp);
       
            var currentVelocityIsInOppositeDirectionOfInput =
                Vector3.Dot(currentVelocity, _inputRelativeToCamera) < 0f;

            if (currentVelocityIsInOppositeDirectionOfInput)
            {
                var yAmount = currentVelocity.y;
                currentVelocity = -(currentVelocity * 0.5f);
                currentVelocity.y = yAmount;
            }

            currentVelocity += _inputRelativeToCamera * AmountOfForwardToAddToJump;
            
            
            
        }

        bool IsFalling => Motor.Velocity.y < -0.001f;
        bool OnTheGround => Motor.GroundingStatus is { IsStableOnGround: true, GroundNormal: { y: > 0.01f } };
        bool OutOfJumps => _usedJumps >= _allowedJumps;

        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
        }

        public bool IsColliderValidForCollisions(Collider coll) => true;

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }
    }

    [Serializable]
    public struct CharacterInputs
    {
        public Vector2 Move;
        public Vector2 Look;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool JumpHeld;
        public bool CrouchDown;
        public bool CrouchUp;
        public float CameraScroll;
        public bool ToggleCameraZoom;
        public Vector3 CameraForward { get; set; }
        public Vector3 CameraPosition { get; set; }
    }

}