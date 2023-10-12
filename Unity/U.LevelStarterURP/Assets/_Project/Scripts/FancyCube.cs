using UnityEngine;
namespace LS
{
    public class FancyCube : MonoBehaviour,ICanBeLookedAt
    {
        [SerializeField]
        Renderer _renderer;

        Quaternion _originalRotation;
        Color _originalColor;
        void Start()
        {
            _originalRotation = transform.rotation;
        }
        
        void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _originalColor = _renderer.material.color;
        }

        void Update()
        {
            if (!_beInteresting) return;
            transform.Rotate(Vector3.up, 90 * Time.deltaTime);
            var color = Color.HSVToRGB(Time.time % 1, 1, 1);
            _renderer.material.color = color;
        }

        bool _beInteresting;
        public void PlayerIsLookingAtMe() => _beInteresting = true;
        public void PlayerIsNotLookingAtMe()
        {
            _beInteresting = false;
            ResetSelf();
        }

        void ResetSelf()
        {
            PrimeTween.Tween.Custom(this, transform.rotation, _originalRotation, 0.5f,
                (t, q) => transform.rotation = q, PrimeTween.Ease.OutBack);
            PrimeTween.Tween.Custom(this, _renderer.material.color, _originalColor, 0.5f,
                (t, c) => _renderer.material.color = c, PrimeTween.Ease.OutBack);
        }
    }

    public interface ICanBeLookedAt
    {
        void PlayerIsLookingAtMe();
        void PlayerIsNotLookingAtMe();
    }
    
}
