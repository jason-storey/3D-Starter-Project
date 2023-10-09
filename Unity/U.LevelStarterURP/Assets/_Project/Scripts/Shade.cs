using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static PrimeTween.Tween;

namespace LS
{
    [AddComponentMenu("LS/Ui/Shade")]
    public class Shade : MonoBehaviour
    {
        [SerializeField] Graphic _graphic;

        void Awake() => _graphic.color = _graphic.color.WithAlpha(1);

        public void CoverScreen(float duration = 0.5f) =>
            Alpha(_graphic, 1, duration);

        public async Task CoverScreenAsync(float duration = 0.5f) =>
            await Alpha(_graphic, 1, duration);

        public void CoverScreenImmediately() =>
            Alpha(_graphic, 1, 0);

        public void CoverScreen(Action onComplete) =>
            Alpha(_graphic, 1, 0.5f).OnComplete(onComplete);

        public void UnCoverScreen(float duration = 0.5f) =>
            Alpha(_graphic, 0, duration);

        public async Task UnCoverScreenAsync(float duration = 0.5f) =>
            await Alpha(_graphic, 0, duration);

        public void UnCoverScreenImmediately() =>
            Alpha(_graphic, 0, 0);

        public void UnCoverScreen(Action onComplete) =>
            Alpha(_graphic, 0, 0.5f).OnComplete(onComplete);

        public void CoverScreenUntilComplete(Func<bool> task,Action onComplete, float duration = 0.5f) => 
            StartCoroutine(DoCoverScreenUntilComplete(task, onComplete, duration));

        public async Task CoverScreenUntilCompleteAsync(Task task, float duration = 0.5f)
        {
            await Alpha(_graphic, 1, duration);
            await task;
            await Alpha(_graphic, 0, 0.5f);
        }

        IEnumerator DoCoverScreenUntilComplete(Func<bool> task,Action onComplete,float duration=0.5f)
        {
            yield return Alpha(_graphic, 1, duration).ToYieldInstruction();
            while (!task())
                yield return null;
            yield return Alpha(_graphic, 0, 0.5f).ToYieldInstruction();
            onComplete();
        }
    }
}
