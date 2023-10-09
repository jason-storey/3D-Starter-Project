#if PRIME_TWEEN_INSTALLED
using UnityEngine;

namespace PrimeTween.Demo {
    public class SqueezeAnimation : MonoBehaviour {
        [SerializeField] Transform target;
        Tween tween;

        public void PlayAnimation() {
            if (!tween.isAlive) {
                tween = Tween.Scale(target, new Vector3(1.15f, 0.9f, 1.15f), 0.2f, Ease.OutSine, 2, CycleMode.Yoyo);
            }
        }
    }
}
#endif