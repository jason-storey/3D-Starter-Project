using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LS
{
    public class LutColorizer : MonoBehaviour
    {
        [SerializeField]
        VolumeProfile _profile;
        [SerializeField]
        Texture[] _luts;
        [SerializeField]
        RectTransform _rectTransform;
        [SerializeField]
        TMP_Text Lbl;
        [SerializeField]
        int _index;
        
        [SerializeField] Ease _ease = Ease.OutSine;

        int numberOfSteps = 4;
        float lutStrength = 1;

        float lutStepSize;
        void Awake() => lutStepSize = 1f / numberOfSteps;

        public void IncreaseLutStrength()
        {
            lutStrength += lutStepSize;
            if (lutStrength > 1)
                lutStrength = 0;
            SetLutStrength(lutStrength);
        }
        
        public void DecreaseLutStrength()
        {
            lutStrength -= lutStepSize;
            if (lutStrength < 0)
                lutStrength = 1;
            SetLutStrength(lutStrength);
        }
        

        void SetLutStrength(float f)
        {
            if (!_profile.TryGet<ColorLookup>(out var colorLookup)) return;
            Tween.Custom(colorLookup.contribution.value, f, 1,
                x => colorLookup.contribution.value = x, ease:_ease);
        }


        [ContextMenu("Next")]
        public void NextLut()
        {
            _index = (_index + 1) % _luts.Length;
            SetLut(_index);
        }

        void Update()
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
                PrevLut();
            if (Keyboard.current.eKey.wasPressedThisFrame)
                NextLut();
            if (Keyboard.current.rKey.wasPressedThisFrame)
                IncreaseLutStrength();
            if (Keyboard.current.fKey.wasPressedThisFrame)
                DecreaseLutStrength();
        }

        void PrevLut()
        {
            _index = (_index - 1 + _luts.Length) % _luts.Length;
            SetLut(_index);
        }

        void SetLut(int index)
        {
            if (!_profile.TryGet<ColorLookup>(out var colorLookup)) return;
            var lut = _luts[index];
            var current = colorLookup.contribution.value;
            var seq = Sequence.Create();
            seq.Chain(Tween.Custom(current,0, 0.2f, x => colorLookup.contribution.value = x, ease: Ease.Linear));
            seq.Group(Tween.UIAnchoredPosition(_rectTransform, new Vector2(33f, -150), 0.3f, Ease.InSine));
            seq.ChainCallback(() =>
            {
                colorLookup.texture.value = lut;
                Lbl.text = lut.name;
            });
            seq.Chain(Tween.Custom(0,current, 0.4f, x => colorLookup.contribution.value = x,Ease.InSine));
            seq.Group(Tween.UIAnchoredPosition(_rectTransform, new Vector2(33f, 150f), 0.3f, Ease.OutSine));
        }
    }
}
