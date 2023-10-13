using System;

namespace JS
{
    [Serializable]
    public class ControllerSettings
    {
        public float JumpHeight = 4;
        public float Gravity = 30;
        public float FallMultiplier = 2.5f;
        public float Drag = 0.1f;
        public float Speed = 10;
        public float MovementSharpness = 10;
        public int AllowedJumps;
    }
}