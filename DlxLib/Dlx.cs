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
    /// Use this class to solve exact cover problems.
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
                get { return new Solution(_currentSolution.ToList()); }
            }

            private readonly Stack<int> _currentSolution = new Stack<int>();
        }

        /// <summary>
        /// Callers should use this constructor when they do not need to be able to request cancellation.
        /// </summary>
        public Dlx()
            : this(CancellationToken.None)
        {
        }

        /// <summary>
        /// Callers should use this constructor when they need to be able to request cancellation.
        /// </summary>
        /// <example>
        /// <code>
        /// var matrix = new[,]
        ///     {
        ///         {0, 0, 1, 0, 1, 1, 0},
        ///         {1, 0, 0, 1, 0, 0, 1},
        ///         {0, 1, 1, 0, 0, 1, 0},
        ///         {1, 0, 0, 1, 0, 0, 0},
        ///         {0, 1, 0, 0, 0, 0, 1},
        ///         {0, 0, 0, 1, 1, 0, 1}
        ///     };
        ///
        /// var cancellationTokenSource = new CancellationTokenSource();
        /// var dlx = new Dlx(cancellationTokenSource.Token);
        ///
        /// dlx.SolutionFound += (_, e) =>
        /// {
        ///     var managedThreadId1 = Thread.CurrentThread.ManagedThreadId;
        ///     Console.WriteLine("[{0}] Found solution {1} - now sleeping", managedThreadId1, e.SolutionIndex);
        ///     Thread.Sleep(2000);
        ///     Console.WriteLine("[{0}] Found solution {1} - now waking", managedThreadId1, e.SolutionIndex);
        /// };
        ///
        /// var thread = new Thread(() => dlx.Solve(matrix).ToList());
        ///
        /// thread.Start();
        ///
        /// var managedThreadId2 = Thread.CurrentThread.ManagedThreadId;
        /// Console.WriteLine("[{0}] Sleeping before calling Cancel...", managedThreadId2);
        /// Thread.Sleep(1000);
        /// Console.WriteLine("[{0}] Calling Cancel...", managedThreadId2);
        /// cancellationTokenSource.Cancel();
        ///
        /// Console.WriteLine("[{0}] Before Join...", managedThreadId2);
        /// thread.Join();
        /// Console.WriteLine("[{0}] After Join", managedThreadId2);
        ///
        /// // The example displays the following output:
        /// //    [5] Sleeping before calling Cancel...
        /// //    [6] Found solution 0 - now sleeping
        /// //    [5] Calling Cancel...
        /// //    [5] Before Join...
        /// //    [6] Found solution 0 - now waking
        /// //    [5] After Join
        /// </code>
        /// </example>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken" /> that
        /// the Solve method overloads will observe.</param>
        public Dlx(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Find all possible solutions to an exact cover problem given a 2-dimensional array of <typeparamref name="T"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// var matrix = new[,]
        ///     {
        ///         {1, 0, 0},
        ///         {0, 1, 0},
        ///         {0, 0, 1}
        ///     };
        /// var dlx = new Dlx();
        /// var solutions = dlx.Solve(matrix);
        /// </code>
        /// </example>
        /// <remarks>
        /// This Solve method overload determines whether a matrix value is a logical 1 or a logical 0
        /// using the following default predicate:
        /// <code>
        /// private static Func&lt;T, bool&gt; DefaultPredicate&lt;T&gt;()
        /// {
        ///     return t => !EqualityComparer&lt;T&gt;.Default.Equals(t, default(T));
        /// }
        /// </code>
        /// </remarks>
        /// <typeparam name="T">The type of elements in the matrix.</typeparam>
        /// <param name="matrix">A matrix of <typeparamref name="T"/> values representing an exact cover problem.</param>
        /// <returns>Yields <see cref="Solution" /> objects as they are found.</returns>
        public IEnumerable<Solution> Solve<T>(T[,] matrix)
        {
            return Solve(matrix, DefaultPredicate<T>());
        }

        /// <summary>
        /// Find all possible solutions to an exact cover problem given a 2-dimensional array of <typeparamref name="T"/>
        /// and a predicate.
        /// </summary>
        /// <example>
        /// <code>
        /// var matrix = new[,]
        ///     {
        ///         {'X', 'O', 'O'},
        ///         {'O', 'X', 'O'},
        ///         {'O', 'O', 'X'}
        ///     };
        /// var dlx = new Dlx();
        /// var solutions = dlx.Solve(matrix, c => c == 'X');
        /// </code>
        /// </example>
        /// <typeparam name="T">The type of elements in the matrix.</typeparam>
        /// <param name="matrix">A matrix of <typeparamref name="T"/> values representing an exact cover problem.</param>
        /// <param name="predicate">A predicate which is invoked for each value in the matrix to determine
        /// whether the value represents a logical 1 or a logical 0 indicated by returning <c>true</c>
        /// or <c>false</c> respectively.</param>
        /// <returns>Yields <see cref="Solution" /> objects as they are found.</returns>
        public IEnumerable<Solution> Solve<T>(T[,] matrix, Func<T, bool> predicate)
        {
            if (matrix == null) throw new ArgumentNullException("matrix");
            return Solve(matrix, m => new Enumerable2DArray<T>(m), r => r, predicate);
        }

        /// <summary>
        /// Find all possible solutions to an exact cover problem given an arbitrary data structure representing
        /// the matrix.
        /// </summary>
        /// <example>
        /// <code>
        /// var data = new List&lt;Tuple&lt;int[], string&gt;&gt;
        ///     {
        ///         Tuple.Create(new[] {1, 0, 0}, "Some data associated with row 0"),
        ///         Tuple.Create(new[] {0, 1, 0}, "Some data associated with row 1"),
        ///         Tuple.Create(new[] {0, 0, 1}, "Some data associated with row 2")
        ///     };
        /// var solutions = new Dlx().Solve(data, d => d, r => r.Item1);
        /// </code>
        /// </example>
        /// <remarks>
        /// This Solve method overload determines whether a matrix value is a logical 1 or a logical 0
        /// using the following default predicate:
        /// <code>
        /// private static Func&lt;T, bool&gt; DefaultPredicate&lt;T&gt;()
        /// {
        ///     return t => !EqualityComparer&lt;T&gt;.Default.Equals(t, default(T));
        /// }
        /// </code>
        /// </remarks>
        /// <typeparam name="TData">The type of the data structure that represents the exact cover problem.</typeparam>
        /// <typeparam name="TRow">The type of the data structure that represents rows in the matrix.</typeparam>
        /// <typeparam name="TCol">The type of the data structure that represents columns in the matrix.</typeparam>
        /// <param name="data">The top-level data structure that represents the exact cover problem.</param>
        /// <param name="iterateRows">A System.Func delegate that will be invoked to iterate the rows in the matrix.</param>
        /// <param name="iterateCols">A System.Func delegate that will be invoked to iterate the columns
        /// in a particular row in the matrix.</param>
        /// <returns>Yields <see cref="Solution" /> objects as they are found.</returns>
        public IEnumerable<Solution> Solve<TData, TRow, TCol>(
            TData data,
            Func<TData, IEnumerable<TRow>> iterateRows,
            Func<TRow, IEnumerable<TCol>> iterateCols)
        {
            return Solve(data, iterateRows, iterateCols, DefaultPredicate<TCol>());
        }

        /// <summary>
        /// Find all possible solutions to an exact cover problem given an arbitrary data structure representing
        /// the matrix and a predicate.
        /// </summary>
        /// <example>
        /// <code>
        /// var data = new List&lt;Tuple&lt;char[], string&gt;&gt;
        ///     {
        ///         Tuple.Create(new[] {'X', 'O', 'O'}, "Some data associated with row 0"),
        ///         Tuple.Create(new[] {'O', 'X', 'O'}, "Some data associated with row 1"),
        ///         Tuple.Create(new[] {'O', 'O', 'X'}, "Some data associated with row 2")
        ///     };
        /// var solutions = new Dlx().Solve(data, d => d, r => r.Item1, c => c == 'X');
        /// </code>
        /// </example>
        /// <typeparam name="TData">The type of the data structure that represents the exact cover problem.</typeparam>
        /// <typeparam name="TRow">The type of the data structure that represents rows in the matrix.</typeparam>
        /// <typeparam name="TCol">The type of the data structure that represents columns in the matrix.</typeparam>
        /// <param name="data">The top-level data structure that represents the exact cover problem.</param>
        /// <param name="iterateRows">A System.Func delegate that will be invoked to iterate the rows in the matrix.</param>
        /// <param name="iterateCols">A System.Func delegate that will be invoked to iterate the columns
        /// in a particular row in the matrix.</param>
        /// <param name="predicate">A predicate which is invoked for each value in the matrix to determine
        /// whether the value represents a logical 1 or a logical 0 indicated by returning <c>true</c>
        /// or <c>false</c> respectively.</param>
        /// <returns>Yields <see cref="Solution" /> objects as they are found.</returns>
        public IEnumerable<Solution> Solve<TData, TRow, TCol>(
            TData data,
            Func<TData, IEnumerable<TRow>> iterateRows,
            Func<TRow, IEnumerable<TCol>> iterateCols,
            Func<TCol, bool> predicate)
        {
            if (data.Equals(default(TData))) throw new ArgumentNullException("data");
            var root = BuildInternalStructure(data, iterateRows, iterateCols, predicate);
            return Search(0, new SearchData(root));
        }

        /// <summary>
        /// Occurs once when the internal search algorithm starts.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs once when the internal search algorithm finishes.
        /// </summary>
        public event EventHandler Finished;

        /// <summary>
        /// Occurs when the caller requests cancellation via the <see cref="System.Threading.CancellationToken" /> passed to <see cref="Dlx(CancellationToken)" />.
        /// </summary>
        public event EventHandler Cancelled;

        /// <summary>
        /// Occurs for each set of rows considered by the internal search algorithm.
        /// </summary>
        public event EventHandler<SearchStepEventArgs> SearchStep;

        /// <summary>
        /// Occurs for each solution found to the exact cover problem.
        /// </summary>
        public event EventHandler<SolutionFoundEventArgs> SolutionFound;

        private static Func<T, bool> DefaultPredicate<T>()
        {
            return t => !EqualityComparer<T>.Default.Equals(t, default(T));
        }

        private bool IsCancelled()
        {
            return _cancellationToken.IsCancellationRequested;
        }

        private static ColumnObject BuildInternalStructure<TData, TRow, TCol>(
            TData data,
            Func<TData, IEnumerable<TRow>> iterateRows,
            Func<TRow, IEnumerable<TCol>> iterateCols,
            Func<TCol, bool> predicate)
        {
            // TODO: Need to adapt to new matrix structure, but in the meantime ...
            var actualRoot = RootObject.Create();                             //DSB

            var root = actualRoot.NewColumn(ColumnCover.Primary);

            int? numColumns = null;
            var rowIndex = 0;
            var colIndexToListHeader = new Dictionary<int, ColumnObject>();

            foreach (var row in iterateRows(data))
            {
                RowObject actualRow = actualRoot.NewRow(); //DSB
                (actualRoot as IColumn).Append(actualRow);                 //DSB

                DataObject firstDataObjectInThisRow = null;
                var localRowIndex = rowIndex;
                var colIndex = 0;

                foreach (var col in iterateCols(row))
                {
                    if (localRowIndex == 0)
                    {
                        var listHeader = actualRoot.NewColumn(ColumnCover.Primary);
                        root.AppendColumnHeader(listHeader);
                        colIndexToListHeader[colIndex] = listHeader;
                    }

                    if (predicate(col))
                    {
                        var listHeader = colIndexToListHeader[colIndex];
                        var elementObject = (ElementObject)null; // DSB new ElementObject(actualRoot, listHeader, localRowIndex, colIndex);

                        if (firstDataObjectInThisRow != null)
                            firstDataObjectInThisRow.AppendToRow(elementObject);
                        else
                            firstDataObjectInThisRow = elementObject;
                    }

                    colIndex++;
                }

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
            }

            return root;
        }

        private static bool MatrixIsEmpty(ColumnObject root)
        {
            return root.Right == root;
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
                        CoverColumn(j.ColumnHeader as ColumnObject);

                    var recursivelyFoundSolutions = Search(k + 1, searchData);
                    foreach (var solution in recursivelyFoundSolutions) yield return solution;

                    for (var j = r.Left; j != r; j = j.Left)
                        UncoverColumn(j.ColumnHeader as ColumnObject);

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

            for (var columnHeader = root.Right as ColumnObject; columnHeader != root; columnHeader = columnHeader.Right as ColumnObject)
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
                    (j.ColumnHeader as ColumnObject).UnlinkDataObject(j);
                }
            }
        }

        private static void UncoverColumn(ColumnObject c)
        {
            for (var i = c.Up; i != c; i = i.Up)
            {
                for (var j = i.Left; j != i; j = j.Left)
                {
                    (j.ColumnHeader as ColumnObject).RelinkDataObject(j);
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
