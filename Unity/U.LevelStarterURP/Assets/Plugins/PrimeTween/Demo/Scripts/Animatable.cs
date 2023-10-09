#if PRIME_TWEEN_INSTALLED
using UnityEngine;

namespace PrimeTween.Demo {
    public abstract class Animatable : MonoBehaviour {
        public virtual void OnClick() {
        }

        public abstract Sequence Animate(bool toEndValue);
    }
}
#endif