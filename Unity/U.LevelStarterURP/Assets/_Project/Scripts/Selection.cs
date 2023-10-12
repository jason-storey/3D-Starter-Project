using System;
using System.Collections.Generic;

namespace LS
{
    public class Selection<T>
    {
        public void Set(T item)
        {
            if (EqualityComparer<T>.Default.Equals(_current, item))
                return;
            if (_current != null) 
                OnUnselected(_current);

            _current = item;
            OnSelected(_current);
        }

        public T Current => _current;
        public bool HasValue => _current != null;

        public static explicit operator T(Selection<T> selection) => selection.Current;
        
        public event Action<T> Unselected;
        public event Action<T> Selected;
        public event Action Cleared;
        T _current;

        public void Clear()
        {
            Set(default);
            Cleared?.Invoke();
        }

        protected virtual void OnSelected(T obj) => Selected?.Invoke(obj);

        protected virtual void OnUnselected(T obj) => Unselected?.Invoke(obj);
    }
}
