using System.Collections;
using System.Collections.Generic;

namespace DlxLib.EnumerableArrayAdapter
{
    internal class Enumerable2DArrayRow<T> : IEnumerable<T>
    {
        private readonly T[,] _array;
        private readonly int _rowIndex;

        public Enumerable2DArrayRow(T[,] array, int rowIndex)
        {
            _array = array;
            _rowIndex = rowIndex;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator2DArrayRow<T>(_array, _rowIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumerator2DArrayRow<TInner> : IEnumerator<TInner>
        {
            private readonly TInner[,] _array;
            private readonly int _numCols;
            private readonly int _rowIndex;
            private int _colIndex = -1;

            public Enumerator2DArrayRow(TInner[,] array, int rowIndex)
            {
                _array = array;
                _numCols = _array.GetLength(1);
                _rowIndex = rowIndex;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_colIndex >= _numCols)
                {
                    return false;
                }

                if (++_colIndex < _numCols)
                {
                    Current = _array[_rowIndex, _colIndex];
                    return true;
                }

                Current = default(TInner);
                return false;
            }

            public void Reset()
            {
                _colIndex = -1;
            }

            public TInner Current { get; private set; }

            object IEnumerator.Current => Current;
        }
    }
}
