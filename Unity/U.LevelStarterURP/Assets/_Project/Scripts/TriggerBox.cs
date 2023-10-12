using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LS
{
    public class TriggerBox : MonoBehaviour
    {
        [SerializeField]
        Color _color;

        void Awake()
        {
            _actors = new HashSet<Collider>();
        }

        [SerializeField]
        public UnityEvent Entered;

        public UnityEvent Exited;
        
        void OnTriggerEnter(Collider other)
        {

            _actors.Add(other);
            Entered.Invoke();
        }

        void OnTriggerExit(Collider other)
        {
            _actors.Remove(other);
            Exited.Invoke();
        }

        void OnDrawGizmos()
        {
            var t = transform;
            var scale = t.localScale;
            var position = t.position;
            Gizmos.color = _color;
            Gizmos.DrawCube(position, scale);
            if (!Application.isPlaying || _actors == null || _actors.Count == 0) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(position, scale);
        }

        HashSet<Collider> _actors;
    }
}
