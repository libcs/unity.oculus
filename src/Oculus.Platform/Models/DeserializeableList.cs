namespace Oculus.Platform.Models
{
    using System.Collections;
    using System.Collections.Generic;

    public class DeserializableList<T> : IList<T>
    {
        //IList
        public int Count => _Data.Count;
        bool ICollection<T>.IsReadOnly => ((IList<T>)_Data).IsReadOnly;  //if you insist in getting it...
        public int IndexOf(T obj) => _Data.IndexOf(obj);
        public T this[int index] { get => _Data[index]; set => _Data[index] = value; }

        public void Add(T item) => _Data.Add(item);
        public void Clear() => _Data.Clear();
        public bool Contains(T item) => _Data.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _Data.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _Data.GetEnumerator();
        public void Insert(int index, T item) => _Data.Insert(index, item);
        public bool Remove(T item) => _Data.Remove(item);
        public void RemoveAt(int index) => _Data.RemoveAt(index);

        // taken from examples here: https://msdn.microsoft.com/en-us/library/s793z9y2(v=vs.110).aspx
        IEnumerator GetEnumerator1() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator1();

        // Internals and getters

        // Seems like Obsolete properties are broken in this version of Mono.
        // Anyway, don't use this.
        [System.Obsolete("Use IList interface on the DeserializableList object instead.", false)]
        public List<T> Data => _Data;

        protected List<T> _Data;
        protected string _NextUrl;
        protected string _PreviousUrl;

        public bool HasNextPage => !string.IsNullOrEmpty(NextUrl);
        public bool HasPreviousPage => !string.IsNullOrEmpty(PreviousUrl);
        public string NextUrl => _NextUrl;
        public string PreviousUrl => _PreviousUrl;
    }
}
