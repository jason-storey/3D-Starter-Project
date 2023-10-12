using JS;
using UnityEngine;

namespace LS
{
    public class ChangeSelectedChildObject : MonoBehaviour,TriggerControls
    {
        [SerializeField]
        ChildSelector _selector;
        PlayerData _data;

        [SerializeField]
        bool _showLogs;

        [SerializeField]
        bool _allowRaycasting;
        
        bool _canRaycast;
        public void OnActivated()
        {
            if (!_allowRaycasting)
            {
                _canRaycast = _data.IsRaycastingAllowed;
                _data.SetRaycastingAllowed(false);
            }
            Say("Taking Controls!");
        }

        [SerializeField]
        bool _allowLookingAround;

        public void WhileInside()
        {

            if (_allowLookingAround)
            {
                _data.PlayerSystem.UpdateInput(new CharacterInputs
                {
                    Look = _data.Inputs.InputHandler.actions["Look"].ReadValue<Vector2>(),
                });
            }
            
            if(Press("Accept"))
                _selector.Select();
            if(Press("Next"))
                _selector.Next();
            if(Press("Prev"))
                _selector.Prev();
        }

        public void OnDeActivated()
        {
            if(!_allowRaycasting)
                _data.SetRaycastingAllowed(_canRaycast);
            Say("Restoring Controls!");
        }

        bool Press(string action) => _data.Inputs.Pressed(action);
        
        public bool ShouldExit => Press("Exit");
        public void SetPlayerData(PlayerData playerData) => _data = playerData;
        public void OnEntered() => Say("Press Interact to Control!");

        public void OnExited() => Say("Exited Trigger!");
        
        void Say(object message)
        {
            if(_showLogs)
                Debug.Log(message);
        }
    }
}
