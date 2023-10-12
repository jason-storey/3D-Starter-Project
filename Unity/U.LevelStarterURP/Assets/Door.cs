using UnityEngine;

namespace LS
{
    public class Door : MonoBehaviour
    {

        [SerializeField,Range(0,1)]
        float _openDuration = 0.3f;
        
        Status _status = Status.Closed;
        
        [SerializeField]
        Transform _doorPivot;

        public void Open(bool openOut = true)
        {
            if (openOut)
                OpenOut();
            else
                OpenIn();
        }
        
        [ContextMenu("Open Out")]
        public void OpenOut()
        {
            _status = Status.OpenOut;
            PrimeTween.Tween.Rotation(_doorPivot, Vector3.up * 90, _openDuration);
        }

        [ContextMenu("Open In")]
        public void OpenIn()
        {
            _status = Status.OpenIn;
            PrimeTween.Tween.Rotation(_doorPivot, Vector3.down * 90, _openDuration);
        }

        [ContextMenu("Close")]
        public void Close()
        {
            _status = Status.Closed;
            PrimeTween.Tween.Rotation(_doorPivot, Vector3.zero, _openDuration);
        }

        public enum Status
        {
            Closed,
            OpenIn,
            OpenOut
        }
    }
}
