using System;
using System.Collections.Generic;
using System.Linq;

// I have used variable names c, r and j deliberately to make it easier to
// relate the code back to the original "Dancing Links" paper:
//
//      Dancing Links (Donald E. Knuth, Stanford University)
//      http://arxiv.org/pdf/cs/0011047v1.pdf

namespace DlxLib
{
    public class Dlx
    {
        private IList<Solution> _solutions;
        private Stack<int> _currentSolution;
        private int _iteration;
        private ColumnObject Root { get; set; }

        public IEnumerable<Solution> Solve(bool[,] matrix)
        {
            BuildInternalStructure(matrix);
            RaiseStarted();
            Search();
            RaiseFinished();
            return _solutions;
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
            var boolMatrix = ToBoolMatrix(matrix, predicate);
            return Solve(boolMatrix);
        }

        public EventHandler Started;
        public EventHandler Finished;
        public EventHandler<SolutionFoundEventArgs> SolutionFound;
        public EventHandler<SearchStepEventArgs> SearchStep;

        private static bool[,] ToBoolMatrix<T>(T[,] matrix, Func<T, bool> predicate)
        {
            var numRows = matrix.GetLength(0);
            var numCols = matrix.GetLength(1);
            var boolMatrix = new bool[numRows, numCols];

            for (var rowIndex = 0; rowIndex < numRows; rowIndex++)
            {
                for (var colIndex = 0; colIndex < numCols; colIndex++)
                {
                    var element = matrix[rowIndex, colIndex];
                    boolMatrix[rowIndex, colIndex] = predicate(element);
                }
            }

            return boolMatrix;
        }

        private void BuildInternalStructure(bool[,] matrix)
        {
            _solutions = new List<Solution>();
            _currentSolution = new Stack<int>();
            _iteration = 0;

            var numRows = matrix.GetLength(0);
            var numCols = matrix.GetLength(1);
            var colIndexToListHeader = new Dictionary<int, ColumnObject>();

            Root = new ColumnObject();

            for (var colIndex = 0; colIndex < numCols; colIndex++)
            {
                var listHeader = new ColumnObject();
                Root.AppendColumnHeader(listHeader);
                colIndexToListHeader[colIndex] = listHeader;
            }

            for (var rowIndex = 0; rowIndex < numRows; rowIndex++)
            {
                // We are starting a new row so indicate that this row is currently empty.
                DataObject firstDataObjectInThisRow = null;

                for (var colIndex = 0; colIndex < numCols; colIndex++)
                {
                    // We are only interested in the 'true's in the matrix.
                    // We create a DataObject for each 'true'. We ignore all the 'false's.
                    if (!matrix[rowIndex, colIndex])
                    {
                        continue;
                    }

                    // Create a new DataObject and add it to the appropriate list header.
                    var listHeader = colIndexToListHeader[colIndex];
                    var dataObject = new DataObject(listHeader, rowIndex);

                    if (firstDataObjectInThisRow != null)
                    {
                        firstDataObjectInThisRow.AppendToRow(dataObject);
                    }
                    else
                    {
                        firstDataObjectInThisRow = dataObject;
                    }
                }
            }
        }

        private bool MatrixIsEmpty()
        {
            return Root.NextColumnObject == Root;
        }

        private void Search(int k = 0)
        {
            _iteration++;

            RaiseSearchStep(k);

            if (MatrixIsEmpty())
            {
                _solutions.Add(new Solution(_currentSolution));
                RaiseSolutionFound();
                return;
            }

            var c = GetListHeaderOfColumnWithLeastRows();
            CoverColumn(c);

            for (var r = c.Down; r != c; r = r.Down)
            {
                _currentSolution.Push(r.RowIndex);

                for (var j = r.Right; j != r; j = j.Right)
                    CoverColumn(j.ListHeader);

                Search(k + 1);

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
            for (var columnHeader = Root.NextColumnObject; columnHeader != Root; columnHeader = columnHeader.NextColumnObject)
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
            var started = Started;

            if (started != null)
            {
                started(this, EventArgs.Empty);
            }
        }

        private void RaiseFinished()
        {
            var finished = Finished;

            if (finished != null)
            {
                finished(this, EventArgs.Empty);
            }
        }

        private void RaiseSolutionFound()
        {
            var solutionFound = SolutionFound;

            if (solutionFound != null)
            {
                var solution = _solutions.Last();
                var solutionIndex = _solutions.Count - 1;
                solutionFound(this, new SolutionFoundEventArgs(solution, solutionIndex));
            }
        }

        private void RaiseSearchStep(int depth)
        {
            var searchStep = SearchStep;

            if (searchStep != null)
            {
                var rowIndexes = _currentSolution.ToList();
                searchStep(this, new SearchStepEventArgs(depth, _iteration, rowIndexes));
            }
        }
    }
}
