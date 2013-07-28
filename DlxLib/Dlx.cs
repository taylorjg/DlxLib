using System;
using System.Collections.Generic;

// I have used variable names c, r and j deliberately to make it easy to
// relate this code to the original "Dancing Links" paper:
//
//      Dancing Links
//      Donald E. Knuth, Stanford University
//      http://arxiv.org/pdf/cs/0011047v1.pdf

namespace DlxLib
{
    public class Dlx
    {
        private IList<Solution> _solutions;
        private Stack<int> _currentSolution;
        private ColumnHeader Root { get; set; }

        public IEnumerable<Solution> Solve(bool[,] matrix)
        {
            BuildInternalStructure(matrix);
            Search();
            return _solutions;
        }

        public IEnumerable<Solution> Solve<T>(T[,] matrix)
        {
            var defaultEqualityComparerT = EqualityComparer<T>.Default;
            var defaultT = default(T);
            Func<T, bool> isTrueFunc = t => !defaultEqualityComparerT.Equals(t, defaultT);
            return Solve(matrix, isTrueFunc);
        }

        public IEnumerable<Solution> Solve<T>(T[,] matrix, Func<T, bool> isTrueFunc)
        {
            var boolMatrix = ToBoolMatrix(matrix, isTrueFunc);
            return Solve(boolMatrix);
        }

        private static bool[,] ToBoolMatrix<T>(T[,] matrix, Func<T, bool> isTrueFunc)
        {
            var numRows = matrix.GetLength(0);
            var numCols = matrix.GetLength(1);
            var boolMatrix = new bool[numRows, numCols];

            for (var rowIndex = 0; rowIndex < numRows; rowIndex++)
            {
                for (var colIndex = 0; colIndex < numCols; colIndex++)
                {
                    boolMatrix[rowIndex, colIndex] = isTrueFunc(matrix[rowIndex, colIndex]);
                }
            }

            return boolMatrix;
        }

        private void BuildInternalStructure(bool[,] matrix)
        {
            _solutions = new List<Solution>();
            _currentSolution = new Stack<int>();

            var numRows = matrix.GetLength(0);
            var numCols = matrix.GetLength(1);
            var colIndexToColumnHeader = new Dictionary<int, ColumnHeader>();

            Root = new ColumnHeader();

            for (var colIndex = 0; colIndex < numCols; colIndex++)
            {
                var columnHeader = new ColumnHeader();
                Root.AppendColumnHeader(columnHeader);
                colIndexToColumnHeader[colIndex] = FindColumnHeader(colIndex);
            }

            for (var rowIndex = 0; rowIndex < numRows; rowIndex++)
            {
                // We are starting a new row so indicate that this row is currently empty.
                Node firstNodeInThisRow = null;

                for (var colIndex = 0; colIndex < numCols; colIndex++)
                {
                    // We are only interested in the '1's ('true's) in the matrix.
                    // We create a node for each '1'. We ignore all the '0's ('false's).
                    if (!matrix[rowIndex, colIndex])
                    {
                        continue;
                    }

                    // Add the node to the appropriate column header.
                    var columnHeader = colIndexToColumnHeader[colIndex];
                    var node = new Node(columnHeader, rowIndex);

                    if (firstNodeInThisRow != null)
                    {
                        firstNodeInThisRow.AppendRowNode(node);
                    }
                    else
                    {
                        firstNodeInThisRow = node;
                    }
                }
            }
        }

        private ColumnHeader FindColumnHeader(int colIndexToFind)
        {
            var columnHeader = Root;

            for (var colIndex = 0; colIndex <= colIndexToFind; colIndex++)
            {
                columnHeader = columnHeader.NextColumnHeader;
            }

            return columnHeader;
        }

        private bool MatrixIsEmpty()
        {
            return Root.NextColumnHeader == Root;
        }

        private void Search()
        {
            if (MatrixIsEmpty())
            {
                _solutions.Add(new Solution(_currentSolution));
                return;
            }

            var c = ChooseColumnWithLeastRows();
            CoverColumn(c);

            for (var r = c.Down; r != c; r = r.Down)
            {
                _currentSolution.Push(r.RowIndex);

                for (var j = r.Right; j != r; j = j.Right)
                    CoverColumn(j.ColumnHeader);

                Search();

                for (var j = r.Left; j != r; j = j.Left)
                    UncoverColumn(j.ColumnHeader);

                _currentSolution.Pop();
            }

            UncoverColumn(c);
        }

        private ColumnHeader ChooseColumnWithLeastRows()
        {
            ColumnHeader chosenColumn = null;

            var smallestNumberOfRows = int.MaxValue;
            for (var columnHeader = Root.NextColumnHeader; columnHeader != Root; columnHeader = columnHeader.NextColumnHeader)
            {
                if (columnHeader.Size < smallestNumberOfRows)
                {
                    chosenColumn = columnHeader;
                    smallestNumberOfRows = columnHeader.Size;
                }
            }

            return chosenColumn;
        }

        private static void CoverColumn(ColumnHeader c)
        {
            c.UnlinkColumnHeader();

            for (var i = c.Down; i != c; i = i.Down)
            {
                for (var j = i.Right; j != i; j = j.Right)
                {
                    j.ColumnHeader.UnlinkNode(j);
                }
            }
        }

        private static void UncoverColumn(ColumnHeader c)
        {
            for (var i = c.Up; i != c; i = i.Up)
            {
                for (var j = i.Left; j != i; j = j.Left)
                {
                    j.ColumnHeader.RelinkNode(j);
                }
            }

            c.RelinkColumnHeader();
        }
    }
}
