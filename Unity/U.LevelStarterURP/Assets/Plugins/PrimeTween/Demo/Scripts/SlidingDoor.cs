#if PRIME_TWEEN_INSTALLED
using UnityEngine;

namespace PrimeTween.Demo {
    public class SlidingDoor : Animatable {
        [SerializeField] Transform animationAnchor;
        [SerializeField] Vector3 openedPos, midPos, closedPos;
        bool isClosed;
        Sequence sequence;

        public override void OnClick() {
            Animate(!isClosed);
        }

        public override Sequence Animate(bool _isClosed) {
            if (isClosed == _isClosed) {
                return Sequence.Create();
            }
            isClosed = _isClosed;
            if (sequence.isAlive) {
                sequence.Stop();
            }
            var tweenSettings = new TweenSettings(0.4f, Ease.OutBack, endDelay: 0.1f);
            sequence = Tween.LocalPosition(animationAnchor, midPos, tweenSettings)
                .Chain(Tween.LocalPosition(animationAnchor, _isClosed ? closedPos : openedPos, tweenSettings));
            return sequence;
        }
    }
}
#endif