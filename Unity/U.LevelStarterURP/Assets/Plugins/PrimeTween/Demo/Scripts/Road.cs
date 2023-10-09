#if PRIME_TWEEN_INSTALLED
using UnityEngine;

namespace PrimeTween.Demo {
    public class Road : Animatable {
        [SerializeField] MeshRenderer roadModel;
        [SerializeField] AnimationCurve ease;
        float currentSpeed;

        void Awake() {
            var matCopy = roadModel.material;
        }

        public override Sequence Animate(bool isAnimating) {
            var currentSpeedTween = Tween.Custom(this, currentSpeed, isAnimating ? 0.3f : 0, 1, (_this, val) => _this.currentSpeed = val);
            var sequence = Sequence.Create(currentSpeedTween);
            if (isAnimating) {
                sequence.Group(Tween.LocalPositionY(transform, 0, -0.5f, 0.7f, ease));
            }
            return sequence;
        }

        void Update() {
            roadModel.material.mainTextureOffset += new Vector2(-1f, 1f) * currentSpeed * Time.deltaTime;
        }
    }
}
#endif