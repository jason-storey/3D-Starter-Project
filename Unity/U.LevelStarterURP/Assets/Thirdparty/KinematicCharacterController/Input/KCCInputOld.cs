using UnityEngine;

namespace JS
{
    [AddComponentMenu("KCC/Player Input (KCC - Old Input)")]
    public class KCCInputOld : MonoBehaviour
    {
        [SerializeField]
        KCCPlayer _player;
        
        void Update()
        {
            if(Input.GetMouseButtonDown(0)) _player.LockMouse();
            _player.UpdateInput(new CharacterInputs
            {
                Look = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                JumpDown = Input.GetKeyDown(KeyCode.Space),
                CrouchDown = Input.GetKeyDown(KeyCode.C),
                CrouchUp = Input.GetKeyUp(KeyCode.C),
                ToggleCameraZoom = Input.GetMouseButtonDown(1),
                CameraScroll = Input.GetAxisRaw("Mouse ScrollWheel")
            });
        }
    }
}
