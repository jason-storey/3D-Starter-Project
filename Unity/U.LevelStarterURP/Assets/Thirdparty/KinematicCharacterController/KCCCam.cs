using System.Collections.Generic;
using UnityEngine;
namespace JS
{
    [AddComponentMenu("KCC/Camera (KCC)")]
    public class KCCCam : MonoBehaviour
    {
      
        #region Fields
        [Header("Framing")]
        public Vector2 _followPointFraming = new(0f, 0f);
        public float _followingSharpness = 10000f;

        [Header("Distance")]
        public float _defaultDistance = 6f;
        public float _minDistance;
        public float _maxDistance = 10f;
        public float _distanceMovementSpeed = 5f;
        public float _distanceMovementSharpness = 10f;

        [Header("Rotation")]
        public bool _invertX;
        public bool _invertY;
        [Range(-90f, 90f)]
        public float _defaultVerticalAngle = 20f;
        [Range(-90f, 90f)]
        public float _minVerticalAngle = -90f;
        [Range(-90f, 90f)]
        public float _maxVerticalAngle = 90f;
        public float _rotationSpeed = 1f;
        public float _rotationSharpness = 10000f;
        
        [SerializeField]
        public bool _rotateWithPhysicsMover = true;

        [Header("Obstruction")]
        public float _obstructionCheckRadius = 0.2f;
        public LayerMask _obstructionLayers = -1;
        public float _obstructionSharpness = 10000f;
        public List<Collider> _ignoredColliders = new();

        public Transform Transform { get; private set; }
        public Transform FollowTransform { get; private set; }

        public Vector3 PlanarDirection { get; set; }
        public float TargetDistance { get; set; }

        bool _distanceIsObstructed;
        float _currentDistance;
        float _targetVerticalAngle;
        RaycastHit _obstructionHit;
        int _obstructionCount;
        readonly RaycastHit[] _obstructions = new RaycastHit[MAX_OBSTRUCTIONS];
        float _obstructionTime;
        Vector3 _currentFollowPosition;

        const int MAX_OBSTRUCTIONS = 32;

        #endregion
        
        void OnValidate()
        {
            _defaultDistance = Mathf.Clamp(_defaultDistance, _minDistance, _maxDistance);
            _defaultVerticalAngle = Mathf.Clamp(_defaultVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
        }

        public void UpdateWithInput(float deltaTime, float zoomInput, Vector3 rotationInput)
        {
            if (!FollowTransform) return;
            if (_invertX) rotationInput.x *= -1f;
            if (_invertY) rotationInput.y *= -1f;
            
            Transform.rotation = HandleInputRotation(deltaTime, rotationInput);
            HandleDistanceMovement(zoomInput);
            SmoothFollowPosition(deltaTime);
            HandleObstructions(deltaTime);
            var targetPosition = SmoothOrbit();
            targetPosition = FrameCamera(targetPosition);
            Transform.position = targetPosition;
        }

        #region plumbing

        void Awake()
        {
            Transform = transform;
            _currentDistance = _defaultDistance;
            TargetDistance = _currentDistance;
            _targetVerticalAngle = 0f;
            PlanarDirection = Vector3.forward;
        }

        public void SetFollowTransform(Transform t)
        {
            FollowTransform = t;
            PlanarDirection = FollowTransform.forward;
            _currentFollowPosition = FollowTransform.position;
        }

        
        Vector3 FrameCamera(Vector3 targetPosition)
        {
            targetPosition += Transform.right * _followPointFraming.x;
            targetPosition += Transform.up * _followPointFraming.y;
            return targetPosition;
        }

        Vector3 SmoothOrbit() => _currentFollowPosition - Transform.rotation * Vector3.forward * _currentDistance;

        void HandleObstructions(float deltaTime)
        {
            var closestHit = new RaycastHit
            {
                distance = Mathf.Infinity
            };
            _obstructionCount = Physics.SphereCastNonAlloc(_currentFollowPosition, _obstructionCheckRadius, -Transform.forward,
                _obstructions, TargetDistance, _obstructionLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < _obstructionCount; i++)
            {
                bool isIgnored = false;
                for (int j = 0; j < _ignoredColliders.Count; j++)
                {
                    if (_ignoredColliders[j] != _obstructions[i].collider) continue;
                    isIgnored = true;
                    break;
                }

                for (int j = 0; j < _ignoredColliders.Count; j++)
                {
                    if (_ignoredColliders[j] != _obstructions[i].collider) continue;
                    isIgnored = true;
                    break;
                }

                if (!isIgnored && _obstructions[i].distance < closestHit.distance && _obstructions[i].distance > 0)
                    closestHit = _obstructions[i];
            }
            
            if (closestHit.distance < Mathf.Infinity)
                CalculateObstructionDistance(deltaTime, closestHit);
            else
                NoObstructions(deltaTime);
        }

        void CalculateObstructionDistance(float deltaTime, RaycastHit closestHit)
        {
            _distanceIsObstructed = true;
            _currentDistance = Mathf.Lerp(_currentDistance, closestHit.distance,
                1 - Mathf.Exp(-_obstructionSharpness * deltaTime));
        }

        void NoObstructions(float deltaTime)
        {
            _distanceIsObstructed = false;
            _currentDistance = Mathf.Lerp(_currentDistance, TargetDistance,
                1 - Mathf.Exp(-_distanceMovementSharpness * deltaTime));
        }

        void SmoothFollowPosition(float deltaTime)
        {
            _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, FollowTransform.position,
                1f - Mathf.Exp(-_followingSharpness * deltaTime));
        }

        void HandleDistanceMovement(float zoomInput)
        {
            if (_distanceIsObstructed && Mathf.Abs(zoomInput) > 0f) TargetDistance = _currentDistance;

            TargetDistance += zoomInput * _distanceMovementSpeed;
            TargetDistance = Mathf.Clamp(TargetDistance, _minDistance, _maxDistance);
        }

        Quaternion? _target;
        public void SetLookRotationTo(Quaternion target)
        {
            _target = target;
        }

        Quaternion HandleInputRotation(float time, Vector3 input)
        {
            var up = FollowTransform.up;
            var rotationFromInput = Quaternion.Euler(up * (input.x * _rotationSpeed));
         
            PlanarDirection = rotationFromInput * PlanarDirection;
            PlanarDirection = Vector3.Cross(up, Vector3.Cross(PlanarDirection, up));
            var planarRot = Quaternion.LookRotation(PlanarDirection, up);
            _targetVerticalAngle -= (input.y * _rotationSpeed);
            _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
            var verticalRot = Quaternion.Euler(_targetVerticalAngle, 0, 0);
            var targetFinalRot = planarRot * verticalRot;

            if (_target.HasValue && _target != Quaternion.identity)
            {
                targetFinalRot = _target.Value;
                PlanarDirection = CalculatePlanarDirectionFromRotation(targetFinalRot);
                _targetVerticalAngle = CalculateTargetVerticalAngleFromRotation(targetFinalRot);
            }

            var targetRotation = Quaternion.Slerp(Transform.rotation, targetFinalRot,
                1f - Mathf.Exp(-_rotationSharpness * time));
            return targetRotation;
        }
        
        

        #endregion

        public void ClearLookTarget()
        {
            _target = null;
        }
        
        float CalculateTargetVerticalAngleFromRotation(Quaternion rotation)
        {
            // Convert the rotation to Euler angles.
            Vector3 eulerAngles = rotation.eulerAngles;

            // Extract the pitch (vertical) angle from the Euler angles.
            // In Unity, the pitch angle is usually represented by the x-axis rotation.
            float verticalAngle = eulerAngles.x;

            return verticalAngle;
        }
        
        Vector3 CalculatePlanarDirectionFromRotation(Quaternion rotation)
        {
            // Create a vector representing the forward direction in the local coordinate system (0, 0, 1).
            Vector3 localForward = Vector3.forward;

            // Rotate the local forward vector by the given rotation to obtain the PlanarDirection.
            Vector3 planarDirection = rotation * localForward;

            // Make sure the result is normalized to ensure it's a unit vector.
            planarDirection.Normalize();

            return planarDirection;
        }

        
        public void LookAt(Vector3 targetPosition)
        {
            var targetRotation = Quaternion.LookRotation(targetPosition - Transform.position);
            SetLookRotationTo(targetRotation);
            Invoke(nameof(ClearLookTarget), 0.1f);
        }
        
        public void Watch(Vector3 targetPosition)
        {
            var targetRotation = Quaternion.LookRotation(targetPosition - Transform.position);
            SetLookRotationTo(targetRotation);
        }
        
        public void SetRotation(Quaternion targetRotation)
        {
            SetLookRotationTo(targetRotation);
            Invoke(nameof(ClearLookTarget), 0.1f);
        }
    }
}