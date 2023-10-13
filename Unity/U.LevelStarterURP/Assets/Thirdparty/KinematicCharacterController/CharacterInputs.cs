using System;
using UnityEngine;

namespace JS
{
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
        public bool GrindDown;
    }
}