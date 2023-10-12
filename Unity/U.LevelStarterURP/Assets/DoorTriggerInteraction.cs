using UnityEngine;
namespace LS
{
    public class DoorTriggerInteraction : MonoBehaviour
    {
        [SerializeField]
        Door _door;
        
        public void OnEvent(AdvancedTriggerArgs args)
        {
            switch (args.Event)
            {
                case EventType.EnterA:
                    _door.OpenIn();
                    break;
                case EventType.EnterB:
                    _door.OpenOut();
                    break;
                case EventType.ExitA:
                case EventType.ExitB:
                    _door.Close();
                    break;
            }
        }
    }
}
