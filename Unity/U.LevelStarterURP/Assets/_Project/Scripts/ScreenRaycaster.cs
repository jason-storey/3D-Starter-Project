using UnityEngine;

namespace LS
{
    public class ScreenRaycaster : Raycaster
    {
        readonly Camera _camera;
        
        float _maxDistance = 100;
        LayerMask _mask;
        public ScreenRaycaster(Camera camera) => _camera = camera;
        public Ray ScreenCenter => _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        public void Check() => Check(ScreenCenter);

        public void SetLayer(LayerMask layer) => _mask = layer;

        protected override bool PerformRaycast(Ray ray, out RaycastHit hit) =>
            _mask == 0 ? 
                Physics.Raycast(ray, out hit, _maxDistance) : 
                Physics.Raycast(ray, out hit, _maxDistance, _mask);

        public void SetMaxDistance(float maxDistance) => _maxDistance = maxDistance;
    }
    
    public class Raycaster
    {
        public Transform Transform => _hit.transform;

        RaycastHit _hit;
        public void Check(Ray ray)
        {
            HitSomething = PerformRaycast(ray, out _hit);
        }
        
        protected virtual bool PerformRaycast(Ray ray,out RaycastHit hit) => Physics.Raycast(ray, out hit);

        public bool HitA<T>(out T item)
        {
            if (!HitSomething)
            {
                item = default;
                return false;
            }
            item = _hit.transform.GetComponent<T>();
            return item != null;
        }
        
        public bool HitSomething { get; set; }
    }
}