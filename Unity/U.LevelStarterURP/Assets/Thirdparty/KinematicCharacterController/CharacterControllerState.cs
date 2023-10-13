using KinematicCharacterController;
using UnityEngine;
using static JS.Keys_PlayerStates;

namespace JS
{
    public class DefaultCharacterState : CharacterControllerState
    {
        int counter;
        public bool _jumpHeld;
        Vector3 _look;
        public float RotateSharpness = 10f;
        
        public int _usedJumps;
        bool _jumpRequested;
        Vector3 _playerInput;
        Vector3 _cameraForward;
        
        public override void SetInputs(ref CharacterInputs inputs)
        {
            _look = inputs.Look;
            _jumpHeld = inputs.JumpHeld;
            CalculateCameraProjections(ref inputs,out _playerInput,out _cameraForward);
            if(inputs.JumpDown)
                _jumpRequested = true;
            if (inputs.GrindDown)
                SetState(GRINDING);
        }

        public override void UpdateRotation(ref Quaternion current, float delta) =>
            current = Quaternion.Slerp(current, Quaternion.LookRotation(_cameraForward, Motor.CharacterUp), delta * RotateSharpness);



        
        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
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
            var gravityToApply = Settings.Gravity;
            if(OutOfJumps && !_jumpHeld || IsFalling)
                gravityToApply *= Settings.FallMultiplier;
            
            currentVelocity -= gravityToApply * deltaTime * Motor.CharacterUp;
            currentVelocity *= 1f / (1f + Settings.Drag * deltaTime);
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
            Vector3 targetMovementVelocity = reorientedInput * Settings.Speed;

            if (!OnTheGround)
            {
                Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity,
                    Motor.CharacterUp);
                currentVelocity += velocityDiff * (1f - Mathf.Exp(-Settings.MovementSharpness * deltaTime));
                return;
            }

            currentVelocity =
                Vector3.Lerp(currentVelocity, targetMovementVelocity,
                    1f - Mathf.Exp(-Settings.MovementSharpness * deltaTime));
        }

       
        public float AmountOfForwardToAddToJump = 10;
        
        void TryToJump(ref Vector3 currentVelocity)
        {
          if (!_jumpRequested || OutOfJumps) return;
          _jumpRequested = false;
          _usedJumps++;
          Motor.ForceUnground();
            currentVelocity += (Motor.CharacterUp * Settings.JumpHeight) -
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
        bool OutOfJumps => _usedJumps >= Settings.AllowedJumps-1;

    }

    public abstract class CharacterControllerState
    {
        public void SetDependencies(KCCCharacterController controller,KCCMotor motor,PlayerData data,ControllerSettings settings)
        {
            Controller = controller;
            Motor = motor;
            Data = data;
            Settings = settings;
        }

        protected void CalculateCameraProjections(ref CharacterInputs inputs,out Vector3 playerInput,out Vector3 camForward)
        {
            playerInput = Vector3.ClampMagnitude(new Vector3(inputs.Move.x, 0f, inputs.Move.y), 1f);
            camForward = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (camForward.sqrMagnitude == 0f)
                camForward = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
        }
        
        public KCCCharacterController Controller { get; set; }

        public ControllerSettings Settings { get; set; }

        public PlayerData Data { get; set; }

        public KCCMotor Motor { get; set; }

        public abstract void SetInputs(ref CharacterInputs inputs);

        public virtual void UpdateRotation(ref Quaternion current,float delta){}
        
        public abstract void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);

        public virtual void BeforeUpdate(float delta){}
        public virtual void AfterUpdate(float delta){}
        public virtual void PostGrounding(float delta){}
        
        public virtual bool IsColliderValidForCollisions(Collider coll) => true;
        
        public virtual void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport){}
        
        public virtual void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport){}
        
        public virtual void OnDiscreteCollisionDetected(Collider hitCollider){}

        public virtual void ExitingState(string nextState)
        {
        }
        
        public virtual void EnteringState(CharacterControllerState previousState)
        {
        }

        public virtual void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }
        
        public void SetState(string state) => Controller.SetState(state);
    }
}