#if PRIME_TWEEN_INSTALLED
using UnityEngine;

namespace PrimeTween.Demo {
    public class CameraController : MonoBehaviour {
        [SerializeField] HighlightedElementController highlightedElementController;
        [SerializeField] SwipeTutorial swipeTutorial;
        [SerializeField] Camera mainCamera;
        [SerializeField, Range(0f, 1f)] float cameraShakeStrength = 0.4f;
        float currentAngle;
        Vector3? inputBeginPos;
        bool isAnimating;
        float curRotationSpeed;

        void Awake() {
            currentAngle = transform.localEulerAngles.y;
            isAnimating = true;
            Tween.Custom(this, 0, 5, 2, (target, val) => target.curRotationSpeed = val);
        }

        void Update() {
            if (isAnimating) {
                currentAngle += curRotationSpeed * Time.deltaTime;
                transform.localEulerAngles = new Vector3(0f, currentAngle);
            }
            if (highlightedElementController.current == null && Input.GetMouseButtonDown(0)) {
                inputBeginPos = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0)) {
                inputBeginPos = null;
            }
            if (inputBeginPos.HasValue) {
                var deltaMove = Input.mousePosition - inputBeginPos.Value;
                if (Mathf.Abs(deltaMove.x) / Screen.width > 0.05f) {
                    isAnimating = false;
                    inputBeginPos = null;
                    currentAngle += Mathf.Sign(deltaMove.x) * 45f;
                    Tween.LocalRotation(transform, new Vector3(0f, currentAngle), 1.5f, Ease.OutCubic);
                    swipeTutorial.Hide();
                }
            }
        }

        public void ShakeCamera() {
            Shake();
        }

        internal Sequence Shake(float startDelay = 0) {
            return Tween.ShakeCamera(mainCamera, cameraShakeStrength, startDelay: startDelay);
        }
    }
}
#endif