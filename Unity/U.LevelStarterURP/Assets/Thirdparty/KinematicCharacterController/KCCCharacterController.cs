using System;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Serialization;

namespace JS
{
    [AddComponentMenu("KCC/Character (KCC)")]
    [RequireComponent(typeof(KCCMotor))]
    public class KCCCharacterController : MonoBehaviour, ICharacterController
    {
        #region Fields

        public KCCMotor Motor;

        [SerializeField]
        PlayerData _data;
        
        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15f;
        public float OrientationSharpness = 10f;
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;
        public float SprintSpeed = 10f;
        
        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 15f;
        public float AirAccelerationSpeed = 15f;
        public float Drag = 0.1f;

        [Header("Jumping")]
        public bool AllowJumpingWhenSliding = false;
        public float JumpUpSpeed = 10f;
        public float JumpScalableForwardSpeed = 10f;
        public float JumpPreGroundingGraceTime = 0f;
        public int JumpCount = 0;
        public bool AllowJumpingAirControl = true;
        
        int ConsumedJumps = 0;
        
        [Header("Dashing")]
        public int DashCount = 0;
        public float DashSpeed = 10f;
        public float DashCooldown = 0.5f;
        public float DashesConsumed = 0;
        [SerializeField]
        bool _applyVelocitySubtractionFromDash = true;
        
        public float GroundedDashSpeedMultiplier;
        public float AirDashSpeedModifier;

        public float GroundDashUpwardsModifier = 1f;
        public float AirDashUpwardsModifier;

        [Header("Misc")]
        public List<Collider> IgnoredColliders = new();
        public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
        public float BonusOrientationSharpness = 10f;
        public Vector3 Gravity = new(0, -30f, 0);
        public Transform MeshRoot;
        public Transform CameraFollowPoint;
        public float CrouchedCapsuleHeight = 1f;

        public CharacterState CurrentCharacterState { get; private set; }

        Collider[] _probedColliders = new Collider[8];
        Vector3 _moveInputVector;
        Vector3 _lookInputVector;
        bool _jumpRequested;
        bool _jumpConsumed;
        bool _jumpedThisFrame;
        float _timeSinceJumpRequested = Mathf.Infinity;
        Vector3 _internalVelocityAdd = Vector3.zero;
        bool _shouldBeCrouching;
        bool _isCrouching;
        float _dashRechargingTime;


        #endregion

        #region Handle_Input

        void HandleDefaultInputs(CharacterInputs inputs, Quaternion cameraPlanarRotation, Vector3 moveInputVector,
            Vector3 cameraPlanarDirection)
        {
            _moveInputVector = cameraPlanarRotation * moveInputVector;
            UpdateOrientationToCamera(cameraPlanarDirection);
            
            if (inputs.JumpDown) Init_Jump();
            if (inputs.DashDown) Init_Dash();
           _isSprinting = inputs.SprintDown;
           _dropDown = inputs.DropDown; 
           if (inputs.CrouchDown && _data.IsFirstPerson) Init_Crouch();
            
            if (inputs.CrouchUp) _shouldBeCrouching = false;
        }
        bool _dropDown;

        [SerializeField]
        bool _isSprinting;


        void UpdateOrientationToCamera(Vector3 cameraPlanarDirection) =>
            _lookInputVector = OrientationMethod switch
            {
                OrientationMethod.TowardsCamera => cameraPlanarDirection,
                OrientationMethod.TowardsMovement => _moveInputVector.normalized,
                _ => _lookInputVector
            };

        void Init_Jump()
        {
            _timeSinceJumpRequested = 0f;
            _jumpRequested = true;
        }

        void Init_Dash()
        {
            _dashRequested = true;
        }

        void Init_Crouch()
        {
            _shouldBeCrouching = true;

            if (_isCrouching) return;
            _isCrouching = true;
            Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
            MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
        }

        #endregion

        #region Handle_Rotation

        void UpdateDefaultStateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
                SmoothInterpolateLookDirection(out currentRotation, deltaTime);
            HandleRotationBasedOnGravity(ref currentRotation, deltaTime);
        }

        void HandleRotationBasedOnGravity(ref Quaternion currentRotation, float deltaTime)
        {
            Vector3 currentUp = (currentRotation * Vector3.up);
            switch (BonusOrientationMethod)
            {
                case BonusOrientationMethod.TowardsGravity:
                {
                    RotateToInvertedGravity(ref currentRotation, deltaTime, currentUp);
                    break;
                }
                case BonusOrientationMethod.TowardsGroundSlopeAndGravity when Motor.GroundingStatus.IsStableOnGround:
                {
                    RotateTowardsGroundAndGravityWhenGrounded(ref currentRotation, deltaTime, currentUp);
                    break;
                }
                case BonusOrientationMethod.TowardsGroundSlopeAndGravity:
                {
                    RotateTowardsGroundAndGravity(ref currentRotation, deltaTime, currentUp);
                    break;
                }
                default:
                {
                    Vector3 smoothedGravityDir =
                        Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                    currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    break;
                }
            }
        }

        void RotateTowardsGroundAndGravity(ref Quaternion currentRotation, float deltaTime, Vector3 currentUp)
        {
            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized,
                1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
        }

        void RotateTowardsGroundAndGravityWhenGrounded(ref Quaternion currentRotation, float deltaTime, Vector3 currentUp)
        {
            Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.Capsule.radius);

            Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal,
                1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;

            // Move the position to create a rotation around the bottom hemi center instead of around the pivot
            Motor.SetTransientPosition(initialCharacterBottomHemiCenter +
                                       (currentRotation * Vector3.down * Motor.Capsule.radius));
        }

        void RotateToInvertedGravity(ref Quaternion currentRotation, float deltaTime, Vector3 currentUp)
        {
            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized,
                1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
        }

        void SmoothInterpolateLookDirection(out Quaternion currentRotation, float deltaTime)
        {
            Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector,
                1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
        }

        #endregion


        void UpdateDefaultState(ref Vector3 currentVelocity, float deltaTime)
        {
            if (Motor.GroundingStatus.IsStableOnGround)
                HandleGrounded(ref currentVelocity, deltaTime);
            else
                HandleAirMovement(ref currentVelocity, deltaTime);

            HandleDashing(ref currentVelocity, deltaTime);
            HandleJumping(ref currentVelocity, deltaTime);
            IncludeAdditiveVelocity(ref currentVelocity);

            HandleDropDown(ref currentVelocity, deltaTime);

        }

        void HandleDropDown(ref Vector3 currentVelocity, float deltaTime)
        {
            if (!_dropDown) return;
            if(Motor.GroundingStatus.IsStableOnGround) return;
            _dropDown = false;
            var speed = currentVelocity.magnitude;
            currentVelocity = Vector3.down * speed;
        }

        void HandleDashing(ref Vector3 currentVelocity, float deltaTime)
        {
            if (!_dashRequested)
            {
               NotDashing(deltaTime);
               return;
            }

            PerformDash(ref currentVelocity);
        }


       public bool UseLookVectorForDashingInstead;

        void PerformDash(ref Vector3 currentVelocity)
        {
            _dashRequested = false;
            if (DashesConsumed >= DashCount) return;
      
            currentVelocity.y = 0;
            var dashDirection = _moveInputVector;
            if(dashDirection.magnitude < 0.1f) dashDirection = _cameraPlanarDirection;
            if (UseLookVectorForDashingInstead) dashDirection = _cameraForwardDirection;
            var existingVelocityInDashDirection = Vector3.Project(currentVelocity, dashDirection);
            var grounded = Motor.GroundingStatus.IsStableOnGround;

            float speed = 0;
            Vector3 dir;

            if (grounded)
            {
                speed = DashSpeed*GroundedDashSpeedMultiplier;
                dir = dashDirection + (Vector3.up * GroundDashUpwardsModifier);
            }
            else
            {
                speed = DashSpeed * AirDashSpeedModifier;
                dir = dashDirection + (Vector3.up * AirDashUpwardsModifier);
            }

            currentVelocity += dir.normalized * speed;
            if(_applyVelocitySubtractionFromDash) currentVelocity -= existingVelocityInDashDirection;
            DashesConsumed++;
        }

  
        void NotDashing(float deltaTime)
        {
            _dashRechargingTime += deltaTime;
            if (!(_dashRechargingTime > DashCooldown)) return;
            _dashRechargingTime = 0;
            DashesConsumed--;
            if (DashesConsumed < 0) DashesConsumed = 0;
        }

        void HandleDefaultUpdate(float deltaTime)
        {
            HandleDefaultJump(deltaTime);
            HandleCrouching();
        }


        void HandleDefaultJump(float deltaTime)
        {
            if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime) _jumpRequested = false;
            if (AllowJumpingWhenSliding
                    ? !Motor.GroundingStatus.FoundAnyGround
                    : !Motor.GroundingStatus.IsStableOnGround) return;
            if (!_jumpedThisFrame) ConsumedJumps = 0;
        }

        #region plumbing

        void Awake()
        {
            TransitionToState(CharacterState.Default);
            Motor.CharacterController = this;
        }

        public void TransitionToState(CharacterState newState)
        {
            CharacterState tmpInitialState = CurrentCharacterState;
            OnStateExit(tmpInitialState, newState);
            CurrentCharacterState = newState;
            OnStateEnter(newState, tmpInitialState);
        }

        /// <summary>
        /// Event when entering a state
        /// </summary>
        public void OnStateEnter(CharacterState state, CharacterState fromState)
        {
            switch (state)
            {
                case CharacterState.Default:
                {
                    break;
                }
            }
        }



        /// <summary>
        /// Event when exiting a state
        /// </summary>
        public void OnStateExit(CharacterState state, CharacterState toState)
        {
            switch (state)
            {
                case CharacterState.Default:
                {
                    break;
                }
            }
        }

        Vector3 _cameraForwardDirection;
        Vector3 _cameraPlanarDirection;
        /// <summary>
        /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref CharacterInputs inputs)
        {
            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.Move.x, 0f, inputs.Move.y), 1f);

            _cameraForwardDirection = inputs.CameraForward;
            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
            }
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);
            _cameraPlanarDirection = cameraPlanarDirection;

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    HandleDefaultInputs(inputs, cameraPlanarRotation, moveInputVector, cameraPlanarDirection);
                    break;
                }
            }
        }

        Quaternion _tmpTransientRot;
        bool _dashRequested;

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    UpdateDefaultStateRotation(ref currentRotation, deltaTime);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    UpdateDefaultState(ref currentVelocity, deltaTime);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void IncludeAdditiveVelocity(ref Vector3 currentVelocity)
        {
            if (!(_internalVelocityAdd.sqrMagnitude > 0f)) return;
            currentVelocity += _internalVelocityAdd;
            _internalVelocityAdd = Vector3.zero;
        }

        void HandleJumping(ref Vector3 currentVelocity, float deltaTime)
        {
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;
            if (!_jumpRequested) return;
            if (!IsAllowedToJump()) return;
            ActualPerformJump(ref currentVelocity,deltaTime);
        }

        void ActualPerformJump(ref Vector3 currentVelocity,float deltatime)
        {
            Motor.ForceUnground();
            currentVelocity.y = 0f;
            var jumpIsInOppositeDirection = Vector3.Dot(_moveInputVector, currentVelocity.normalized) < 0;
            if (jumpIsInOppositeDirection)
            {
                currentVelocity = -currentVelocity;
                currentVelocity *= 0.5f;
            }
            currentVelocity += Motor.CharacterUp * JumpUpSpeed;

            

            _jumpRequested = false;
            ConsumedJumps++;
            _jumpConsumed = true;
            _jumpedThisFrame = true;
        }

        void PerformOldJump(ref Vector3 currentVelocity)
        {
            // Calculate jump direction before ungrounding
            Vector3 jumpDirection = Motor.CharacterUp;
            if (Motor.GroundingStatus is { FoundAnyGround: true, IsStableOnGround: false })
                jumpDirection = Motor.GroundingStatus.GroundNormal;

            Motor.ForceUnground();

            var existingVelocity = Vector3.Project(currentVelocity, Motor.CharacterUp);
            var jumpIsInOppositeDirection = Vector3.Dot(_moveInputVector, jumpDirection) < 0;
            if (jumpIsInOppositeDirection)
            {
                currentVelocity = _moveInputVector * existingVelocity.magnitude;
            }


            // Add to the return velocity and reset jump state
            currentVelocity += (jumpDirection * JumpUpSpeed) - existingVelocity;
            currentVelocity += (_moveInputVector * JumpScalableForwardSpeed);
            _jumpRequested = false;
            ConsumedJumps++;
            _jumpConsumed = true;
            _jumpedThisFrame = true;
        }

        bool IsAllowedToJump() => ConsumedJumps < JumpCount;

        void HandleAirMovement(ref Vector3 currentVelocity, float deltaTime)
        {
            // Add move input
            if (_moveInputVector.sqrMagnitude > 0f)
            {
                Vector3 addedVelocity = _moveInputVector * (AirAccelerationSpeed * deltaTime);

                Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                // Limit air velocity from inputs
                if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed)
                {
                    // clamp addedVel to make total vel not exceed max vel on inputs plane
                    Vector3 newTotal =
                        Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, MaxAirMoveSpeed);
                    addedVelocity = newTotal - currentVelocityOnInputsPlane;
                }
                else
                {
                    // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                    if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                    {
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                    }
                }

                // Prevent air-climbing sloped walls
                if (Motor.GroundingStatus.FoundAnyGround)
                {
                    if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                    {
                        Vector3 perpenticularObstructionNormal = Vector3
                            .Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp)
                            .normalized;
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                    }
                }

                // Apply added velocity
                currentVelocity += addedVelocity;
            }

            // Gravity
            currentVelocity += Gravity * deltaTime;

            // Drag
            currentVelocity *= (1f / (1f + (Drag * deltaTime)));
        }

        void HandleGrounded(ref Vector3 currentVelocity, float deltaTime)
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;

            Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

            // Reorient velocity on slope
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                              currentVelocityMagnitude;

            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
            Vector3 reorientedInput =
                Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
            
            var actualSpeed = _isSprinting ? SprintSpeed : MaxStableMoveSpeed;
            
            Vector3 targetMovementVelocity = reorientedInput * actualSpeed;

            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    HandleDefaultUpdate(deltaTime);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void HandleCrouching()
        {
            if (!_isCrouching || _shouldBeCrouching) return;
            // Do an overlap test with the character's standing height to see if there are any obstructions
            Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
            if (Motor.CharacterOverlap(
                    Motor.TransientPosition,
                    Motor.TransientRotation,
                    _probedColliders,
                    Motor.CollidableLayers,
                    QueryTriggerInteraction.Ignore) > 0)
            {
                // If obstructions, just stick to crouching dimensions
                Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
            }
            else
            {
                // If no obstructions, uncrouch
                MeshRoot.localScale = new Vector3(1f, 1f, 1f);
                _isCrouching = false;
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            switch (Motor.GroundingStatus.IsStableOnGround)
            {
                // Handle landing and leaving ground
                case true when !Motor.LastGroundingStatus.IsStableOnGround:
                    OnLanded();
                    break;
                case false when Motor.LastGroundingStatus.IsStableOnGround:
                    OnLeaveStableGround();
                    break;
            }
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Count == 0)
                return true;
            return !IgnoredColliders.Contains(coll);
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void AddVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    _internalVelocityAdd += velocity;
                    break;
                }
            }
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        protected void OnLanded()
        {
        }

        protected void OnLeaveStableGround()
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }
        

        #endregion

        public void TeleportAndLookAt(Vector3 position, Vector3 positionOfTarget)
        {
            var lookRotation = Quaternion.LookRotation(positionOfTarget - position);
            Motor.SetPositionAndRotation(position, lookRotation);
        }
        
        public void TeleportTo(Vector3 position,Quaternion rotation)
        {
            Motor.BaseVelocity = Vector3.zero;
            Motor.ForceUnground();
            Motor.SetPositionAndRotation(position,rotation);
        }

        public void TeleportTo(Vector3 position) => Motor.SetPosition(position);
    }

    #region Sub Types

    public enum CharacterState
    {
        Default,
    }

    public enum OrientationMethod
    {
        TowardsCamera,
        TowardsMovement,
    }

    [Serializable]
    public struct CharacterInputs
    {
        public Vector2 Move;
        public Vector2 Look;
        public Vector3 CameraPosition;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool CrouchDown;
        public bool CrouchUp;
        public float CameraScroll;
        public bool ToggleCameraZoom;
        public bool DashDown;
        public bool DropDown;
        public bool SprintDown { get; set; }
        public Vector3 CameraForward { get; set; }
    }
    

    public enum BonusOrientationMethod
    {
        None,
        TowardsGravity,
        TowardsGroundSlopeAndGravity,
    }
    

    #endregion
}