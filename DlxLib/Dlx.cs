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
    /// <summary>
    /// 
    /// </summary>
    public class Dlx
    {
        private readonly CancellationToken _cancellationToken;

        private class SearchData
        {
            public SearchData(ColumnObject root)
            {
                Root = root;
            }

            public ColumnObject Root { get; private set; }
            public int IterationCount { get; private set; }
            public int SolutionCount { get; private set; }

            public void IncrementIterationCount()
            {
                IterationCount++;
            }

            public void IncrementSolutionCount()
            {
                SolutionCount++;
            }

            public void PushCurrentSolutionRowIndex(int rowIndex)
            {
                _currentSolution.Push(rowIndex);
            }

            public void PopCurrentSolutionRowIndex()
            {
                _currentSolution.Pop();
            }

            public Solution CurrentSolution
            {
                get { return new Solution(_currentSolution); }
            }

            private readonly Stack<int> _currentSolution = new Stack<int>();
        }

        /// <summary>
        /// 
        /// </summary>
        public Dlx()
            : this(CancellationToken.None)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public Dlx(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public IEnumerable<Solution> Solve(bool[,] matrix)
        {
            return Solve<bool>(matrix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public IEnumerable<Solution> Solve<T>(T[,] matrix)
        {
            var defaultEqualityComparerT = EqualityComparer<T>.Default;
            var defaultT = default(T);
            Func<T, bool> predicate = t => !defaultEqualityComparerT.Equals(t, defaultT);
            return Solve(matrix, predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TRow"></typeparam>
        /// <typeparam name="TCol"></typeparam>
        /// <param name="data"></param>
        /// <param name="iterateRows"></param>
        /// <param name="iterateCols"></param>
        /// <returns></returns>
        public IEnumerable<Solution> Solve<TData, TRow, TCol>(
            TData data,
            Action<TData, Action<TRow>> iterateRows,
            Action<TRow, Action<TCol>> iterateCols)
        {
            var defaultEqualityComparerTCol = EqualityComparer<TCol>.Default;
            var defaultTCol = default(TCol);
            Func<TCol, bool> predicate = col => !defaultEqualityComparerTCol.Equals(col, defaultTCol);

            return Solve(
                data,
                iterateRows,
                iterateCols,
                predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TRow"></typeparam>
        /// <typeparam name="TCol"></typeparam>
        /// <param name="data"></param>
        /// <param name="iterateRows"></param>
        /// <param name="iterateCols"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IEnumerable<Solution> Solve<TData, TRow, TCol>(
            TData data,
            Action<TData, Action<TRow>> iterateRows,
            Action<TRow, Action<TCol>> iterateCols,
            Func<TCol, bool> predicate)
        {
            if (data.Equals(default(TData))) throw new ArgumentNullException("data");
            var root = BuildInternalStructure(data, iterateRows, iterateCols, predicate);
            return Search(0, new SearchData(root));
        }

        /// <summary>
        /// 
        /// </summary>
        public EventHandler Started;

        /// <summary>
        /// 
        /// </summary>
        public EventHandler Finished;

        /// <summary>
        /// 
        /// </summary>
        public EventHandler Cancelled;

        /// <summary>
        /// 
        /// </summary>
        public EventHandler<SearchStepEventArgs> SearchStep;

        /// <summary>
        /// 
        /// </summary>
        public EventHandler<SolutionFoundEventArgs> SolutionFound;

        private bool IsCancelled()
        {
            return _cancellationToken.IsCancellationRequested;
        }

        private static ColumnObject BuildInternalStructure<TData, TRow, TCol>(
            TData data,
            Action<TData, Action<TRow>> iterateRows,
            Action<TRow, Action<TCol>> iterateCols,
            Func<TCol, bool> predicate)
        {
            var root = new ColumnObject();

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
                        root.AppendColumnHeader(listHeader);
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
                        throw new ArgumentException("All rows must contain the same number of columns!", "data");
                    }
                }
                else
                {
                    numColumns = colIndex;
                }

                rowIndex++;
            });

            return root;
        }

        private static bool MatrixIsEmpty(ColumnObject root)
        {
            return root.NextColumnObject == root;
        }

        private IEnumerable<Solution> Search(int k, SearchData searchData)
        {
            try
            {
                if (k == 0) RaiseStarted();

                if (IsCancelled())
                {
                    RaiseCancelled();
                    yield break;
                }

                RaiseSearchStep(searchData.IterationCount, searchData.CurrentSolution.RowIndexes);
                searchData.IncrementIterationCount();

                if (MatrixIsEmpty(searchData.Root))
                {
                    if (searchData.CurrentSolution.RowIndexes.Any())
                    {
                        searchData.IncrementSolutionCount();
                        var solutionIndex = searchData.SolutionCount - 1;
                        var solution = searchData.CurrentSolution;
                        RaiseSolutionFound(solution, solutionIndex);
                        yield return solution;
                    }

                    yield break;
                }

                var c = ChooseColumnWithLeastRows(searchData.Root);
                CoverColumn(c);

                for (var r = c.Down; r != c; r = r.Down)
                {
                    if (IsCancelled())
                    {
                        RaiseCancelled();
                        yield break;
                    }

                    searchData.PushCurrentSolutionRowIndex(r.RowIndex);

                    for (var j = r.Right; j != r; j = j.Right)
                        CoverColumn(j.ListHeader);

                    var recursivelyFoundSolutions = Search(k + 1, searchData);
                    foreach (var solution in recursivelyFoundSolutions) yield return solution;

                    for (var j = r.Left; j != r; j = j.Left)
                        UncoverColumn(j.ListHeader);

                    searchData.PopCurrentSolutionRowIndex();
                }

                UncoverColumn(c);

            }
            finally
            {
                if (k == 0) RaiseFinished();
            }
        }

        private static ColumnObject ChooseColumnWithLeastRows(ColumnObject root)
        {
            ColumnObject chosenColumn = null;

            for (var columnHeader = root.NextColumnObject; columnHeader != root; columnHeader = columnHeader.NextColumnObject)
            {
                if (chosenColumn == null || columnHeader.NumberOfRows < chosenColumn.NumberOfRows)
                    chosenColumn = columnHeader;
            }

            return chosenColumn;
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
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void RaiseFinished()
        {
            var handler = Finished;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void RaiseCancelled()
        {
            var handler = Cancelled;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void RaiseSearchStep(int iteration, IEnumerable<int> rowIndexes)
        {
            var handler = SearchStep;
            if (handler != null) handler(this, new SearchStepEventArgs(iteration, rowIndexes));
        }

        private void RaiseSolutionFound(Solution solution, int solutionIndex)
        {
            var handler = SolutionFound;
            if (handler != null) handler(this, new SolutionFoundEventArgs(solution, solutionIndex));
        }
    }
}
