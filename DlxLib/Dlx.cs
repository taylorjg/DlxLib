using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DlxLib.EnumerableArrayAdapter;

// I have used variable names c, r and j deliberately to make it easier to
// relate the code back to the original "Dancing Links" paper:
//
//      Dancing Links (Donald E. Knuth, Stanford University)
//      http://arxiv.org/pdf/cs/0011047v1.pdf

namespace DlxLib
{
    public class Dlx
    {
        private ColumnObject _root;
        private IList<Solution> _solutions;
        private Stack<int> _currentSolution;
        private int _iteration;
        private readonly ManualResetEventSlim _cancelEvent = new ManualResetEventSlim(false);
        private readonly CancellationToken _cancellationToken;

        public Dlx()
            : this(CancellationToken.None)
        {
        }

        public Dlx(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public IEnumerable<Solution> Solve(bool[,] matrix)
        {
            return Solve<bool>(matrix);
        }

        public IEnumerable<Solution> Solve<T>(T[,] matrix)
        {
            var defaultEqualityComparerT = EqualityComparer<T>.Default;
            var defaultT = default(T);
            Func<T, bool> predicate = t => !defaultEqualityComparerT.Equals(t, defaultT);
            return Solve(matrix, predicate);
        }

        public IEnumerable<Solution> Solve<T>(T[,] matrix, Func<T, bool> predicate)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            return Solve<T[,], IEnumerable<T>, T>(
                matrix,
                (m, f) => { foreach (var r in new Enumerable2DArray<T>(m)) f(r); },
                (r, f) => { foreach (var c in r) f(c); },
                predicate);
        }

        public IEnumerable<Solution> Solve<TData, TRow, TCol>(
            TData data,
            Action<TData, Action<TRow>> iterateRows,
            Action<TRow, Action<TCol>> iterateCols,
            Func<TCol, bool> predicate)
        {
            if (data.Equals(default(TData)))
            {
                throw new ArgumentNullException("data");
            }

            BuildInternalStructure(data, iterateRows, iterateCols, predicate);
            RaiseStarted();

            Search();

            if (IsCancelled())
                RaiseCancelled();
            else
                RaiseFinished();

            return _solutions;
        }

        [Obsolete("Pass a CancellationToken to the Dlx constructor instead")]
        public void Cancel()
        {
            _cancelEvent.Set();
        }

        public EventHandler Started;
        public EventHandler Finished;
        public EventHandler Cancelled;
        public EventHandler<SearchStepEventArgs> SearchStep;
        public EventHandler<SolutionFoundEventArgs> SolutionFound;

        private bool IsCancelled()
        {
            return _cancelEvent.IsSet || _cancellationToken.IsCancellationRequested;
        }

        private void BuildInternalStructure<TData, TRow, TCol>(
            TData data,
            Action<TData, Action<TRow>> iterateRows,
            Action<TRow, Action<TCol>> iterateCols,
            Func<TCol, bool> predicate)
        {
            _root = new ColumnObject();
            _solutions = new List<Solution>();
            _currentSolution = new Stack<int>();
            _iteration = 0;
            _cancelEvent.Reset();

            int? numColumns = null;
            var rowIndex = 0;
            var colIndexToListHeader = new Dictionary<int, ColumnObject>();

            iterateRows(data, row =>
            {
                DataObject firstDataObjectInThisRow = null;
                var localRowIndex = rowIndex;
                var colIndex = 0;

                iterateCols(row, col =>
                {
                    if (localRowIndex == 0)
                    {
                        var listHeader = new ColumnObject();
                        _root.AppendColumnHeader(listHeader);
                        colIndexToListHeader[colIndex] = listHeader;
                    }

                    if (predicate(col))
                    {
                        // Create a new DataObject and add it to the appropriate list header.
                        var listHeader = colIndexToListHeader[colIndex];
                        var dataObject = new DataObject(listHeader, localRowIndex);

                        if (firstDataObjectInThisRow != null)
                            firstDataObjectInThisRow.AppendToRow(dataObject);
                        else
                            firstDataObjectInThisRow = dataObject;
                    }

                    colIndex++;
                });

                if (numColumns.HasValue)
                {
                    if (colIndex != numColumns)
                    {
                        throw new ArgumentException("All rows are meant to contain the same number of rows!", "data");
                    }
                }
                else
                {
                    numColumns = colIndex;
                }

                rowIndex++;
            });
        }

        private bool MatrixIsEmpty()
        {
            return _root.NextColumnObject == _root;
        }

        private void Search()
        {
            if (IsCancelled())
            {
                return;
            }

            RaiseSearchStep();

            _iteration++;

            if (MatrixIsEmpty())
            {
                if (_currentSolution.Count > 0)
                {
                    _solutions.Add(new Solution(_currentSolution));
                    RaiseSolutionFound();
                }
                return;
            }

            var c = GetListHeaderOfColumnWithLeastRows();
            CoverColumn(c);

            for (var r = c.Down; r != c; r = r.Down)
            {
                if (IsCancelled())
                {
                    return;
                }

                _currentSolution.Push(r.RowIndex);

                for (var j = r.Right; j != r; j = j.Right)
                    CoverColumn(j.ListHeader);

                Search();

                for (var j = r.Left; j != r; j = j.Left)
                    UncoverColumn(j.ListHeader);

                _currentSolution.Pop();
            }

            UncoverColumn(c);
        }

        private ColumnObject GetListHeaderOfColumnWithLeastRows()
        {
            ColumnObject listHeader = null;

            var smallestNumberOfRows = int.MaxValue;
            for (var columnHeader = _root.NextColumnObject; columnHeader != _root; columnHeader = columnHeader.NextColumnObject)
            {
                if (columnHeader.NumberOfRows < smallestNumberOfRows)
                {
                    listHeader = columnHeader;
                    smallestNumberOfRows = columnHeader.NumberOfRows;
                }
            }

            return listHeader;
        }

        private static void CoverColumn(ColumnObject c)
        {
            c.UnlinkColumnHeader();

            for (var i = c.Down; i != c; i = i.Down)
            {
                for (var j = i.Right; j != i; j = j.Right)
                {
                    j.ListHeader.UnlinkDataObject(j);
                }
            }
        }

        private static void UncoverColumn(ColumnObject c)
        {
            for (var i = c.Up; i != c; i = i.Up)
            {
                for (var j = i.Left; j != i; j = j.Left)
                {
                    j.ListHeader.RelinkDataObject(j);
                }
            }

            c.RelinkColumnHeader();
        }

        private void RaiseStarted()
        {
            var handler = Started;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void RaiseFinished()
        {
            var handler = Finished;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void RaiseCancelled()
        {
            var handler = Cancelled;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void RaiseSearchStep()
        {
            var handler = SearchStep;

            if (handler != null)
            {
                var rowIndexes = _currentSolution.ToList();
                handler(this, new SearchStepEventArgs(_iteration, rowIndexes));
            }
        }

        private void RaiseSolutionFound()
        {
            var handler = SolutionFound;

            if (handler != null)
            {
                var solution = _solutions.Last();
                var solutionIndex = _solutions.Count - 1;
                handler(this, new SolutionFoundEventArgs(solution, solutionIndex));
            }
        }
    }
}
