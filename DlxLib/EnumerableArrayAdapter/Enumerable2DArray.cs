using System.Collections;
using System.Collections.Generic;

namespace DlxLib.EnumerableArrayAdapter
{
    internal class Enumerable2DArray<T> : IEnumerable<Enumerable2DArrayRow<T>>
    {
        private readonly T[,] _array;

        public Enumerable2DArray(T[,] array)
        {
            _array = array;
        }

        public IEnumerator<Enumerable2DArrayRow<T>> GetEnumerator()
        {
            return new Enumerator2DArray<T>(_array);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumerator2DArray<TInner> : IEnumerator<Enumerable2DArrayRow<TInner>>
        {
            private readonly TInner[,] _array;
            private readonly int _numRows;
            private int _rowIndex = -1;

            public Enumerator2DArray(TInner[,] array)
            {
                _array = array;
                _numRows = _array.GetLength(0);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_rowIndex >= _numRows)
                {
                    return false;
                }

                if (++_rowIndex < _numRows)
                {
                    Current = new Enumerable2DArrayRow<TInner>(_array, _rowIndex);
                    return true;
                }

                Current = default(Enumerable2DArrayRow<TInner>);
                return false;
            }

            public void Reset()
            {
                _rowIndex = -1;
            }

            public Enumerable2DArrayRow<TInner> Current { get; private set; }

            object IEnumerator.Current => Current;
        }
    }
}
