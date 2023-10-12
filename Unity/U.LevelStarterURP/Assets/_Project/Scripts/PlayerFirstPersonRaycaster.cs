using UnityEngine;

namespace LS
{
    public class PlayerFirstPersonRaycaster : MonoBehaviour
    {
        [SerializeField]
        PlayerData _playerData;
        
        [SerializeField]
        LayerMask _layer;

        [SerializeField, Range(0, 1000)]
        float _maxDistance = 100;

        [SerializeField]
        Transform _selected;
        
        ICanBeLookedAt _lookedAt;
        ScreenRaycaster _raycaster;
        void Awake()
        {
            _raycaster = new ScreenRaycaster(Camera.main);
            _raycaster.SetLayer(_layer);
            _raycaster.SetMaxDistance(_maxDistance);
            _selection = new Selection<Transform>();
            _selection.Selected += OnSelected;
            _selection.Unselected += OnUnselected;
        }

        void OnEnable()
        {
            _playerData.RaycastAllowedChanged += OnRaycastAllowedChanged;
        }

        void OnRaycastAllowedChanged(bool canRaycast)
        {
            if (canRaycast) return;

            if(_selected) _selection.Clear();
            
        }

        void OnUnselected(Transform obj)
        {
            if (!obj) return;
            if (_selected.TryGetComponent(out _lookedAt)) _lookedAt.PlayerIsNotLookingAtMe();
            _selected = null;
            _playerData.LookingAt = null;
        }

        void OnSelected(Transform obj)
        {
            if (_selected == obj) return;
            if (!obj)
            {
                _selected = null;
                _playerData.LookingAt = null;
                return;
            }
            
            
            
            _playerData.LookingAt = obj;
            _selected = obj;
           if (_selected.TryGetComponent(out _lookedAt))  _lookedAt.PlayerIsLookingAtMe();
        }

        void OnValidate()
        {
            if (_raycaster == null) return;
               _raycaster.SetLayer(_layer);
               _raycaster.SetMaxDistance(_maxDistance);
        }

        void Update()
        {
            if(!_playerData.IsRaycastingAllowed) return;
            _raycaster.Check();
            if (_raycaster.HitSomething)
                _selection.Set(_raycaster.Transform);
            else
                _selection.Clear();
        }

        void OnDrawGizmos()
        {
            if(_raycaster == null) return;
            Gizmos.color = _selected ? Color.green  :  Color.red;
            var ray = _raycaster.ScreenCenter;
            Gizmos.DrawRay(ray.origin, ray.direction * _maxDistance);
        }

        Selection<Transform> _selection;
    }
}
