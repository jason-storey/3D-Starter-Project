#if PRIME_TWEEN_INSTALLED
using UnityEngine;

namespace PrimeTween.Demo {
    public class JumpAnimation : MonoBehaviour {
        [SerializeField] Transform target;
        Sequence sequence;

        public void PlayAnimation() {
            if (!sequence.isAlive) {
                const float jumpDuration = 0.3f;
                sequence = Tween.Scale(target, new Vector3(1.1f, 0.8f, 1.1f), 0.15f, Ease.OutQuad, 2, CycleMode.Yoyo)
                    .Chain(Tween.LocalPositionY(target, 1, jumpDuration))
                    .Chain(Tween.LocalEulerAngles(target, Vector3.zero, new Vector3(0, 360), 1.5f, Ease.InOutBack))
                    .Chain(Tween.LocalPositionY(target, 0, jumpDuration));
            }
        }
    }
}
#endif