using UnityEngine;
using static PrimeTween.Tween;

namespace LS
{
    public class Door : MonoBehaviour
    {
        [SerializeField,Header("Dependencies")]
        Transform _doorPivot;

        [SerializeField,Range(0,1),Header("Settings")]
        float _openDuration = 0.3f;
        
        [ContextMenu("Open Out")]
        public void OpenOut() => Rotation(_doorPivot, Vector3.up * 90, _openDuration);

        [ContextMenu("Open In")]
        public void OpenIn() => Rotation(_doorPivot, Vector3.down * 90, _openDuration);

        [ContextMenu("Close")]
        public void Close() => Rotation(_doorPivot, Vector3.zero, _openDuration);
    }
}
