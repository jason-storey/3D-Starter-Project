using UnityEngine;

namespace LS
{
    public class ChildSelector : MonoBehaviour,ICanChangeSelection
    {
        [SerializeField]
        GameObject _current;

        [SerializeField]
        int index = -1;

        void Awake()
        {
            if (index != -1)
            {
                _current = transform.GetChild(index % transform.childCount).gameObject;
                _current.SetActive(true);
            }
        }


        public void Next()
        {
            var childCount = transform.childCount;
            if (childCount == 0) return;
            index = (index + 1) % childCount;
            if(_current) _current.SetActive(false);
            _current = transform.GetChild(index).gameObject;
            _current.SetActive(true);
        }

        public void Prev()
        {
            var childCount = transform.childCount;
            if (childCount == 0) return;
            index = (index - 1 + childCount) % childCount;
            if(_current) _current.SetActive(false);
            _current = transform.GetChild(index).gameObject;
            _current.SetActive(true);
            
        }

        public void Select()
        {
            
        }
    }

    public interface ICanChangeSelection
    {
        void Next();
        void Prev();
        void Select();
    }
    
}
