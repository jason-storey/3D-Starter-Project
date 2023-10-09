#if PRIME_TWEEN_INSTALLED
using UnityEngine;

namespace PrimeTween.Demo {
    public class HighlightableElement : MonoBehaviour {
        [SerializeField] public Transform highlightAnchor;
        public MeshRenderer[] models { get; private set; }

        void OnEnable() {
            models = GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in models) {
                var matCopy = mr.material;
            }
        }
    }
}
#endif