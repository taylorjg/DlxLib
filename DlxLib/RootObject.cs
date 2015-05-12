using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using DlxLib.EnumerableArrayAdapter;

namespace DlxLib
{
    /// <summary>
    /// Root object of DLX binary matrix.  Holds all the elements (arranged in rows
    /// and columns, of course) and gives accessors to them.  Also, ways to create
    /// the data matrix.  Also, ways to manipulate it for the DLX algorithms: cover
    /// and uncover.
    /// </summary>
    /// <remarks>
    /// Knuth didn't use a separate structure for this, reusing a column object for
    /// the root of the data matrix.
    /// </remarks>
    internal class RootObject : HeaderObject, IRoot
    {
        private RootObject()
            : base()
        {
            _AllRows = new List<RowObject>();
            _AllColumns = new List<ColumnObject>();
        }

        public static RootObject Create()
        {
            RootObject root = new RootObject();
            return root;
        }

        public RowObject NewRow()
        {
            RowObject row = new RowObject(this);
            return row;
        }

        public ColumnObject NewColumn(ColumnCover columnCover)
        {
            ColumnObject column = new ColumnObject(this, columnCover);
            return column;
        }

        public ElementObject NewElement(RowObject row, ColumnObject column)
        {
            ElementObject element = new ElementObject(this, row, column);
            return element;
        }

        public ElementObject NewElement(int rowIndex, int columnIndex)
        {
            RowObject row = GetRow(rowIndex);
            ColumnObject column = GetColumn(columnIndex);
            ElementObject element = NewElement(row, column);
            return element;
        }

        /// <summary>
        /// Holds all rows of this data matrix, indexed by RowIndex.
        /// </summary>
        protected internal readonly IList<RowObject> _AllRows;

        /// <summary>
        /// Holds all columns - primary and secondary - of this data matrix, indexed
        /// by ColumnIndex.
        /// </summary>
        protected internal readonly IList<ColumnObject> _AllColumns;

        #region IDataObject Members
        public override RootObject Root { get { return this; } }

        public override IRow RowHeader
        {
            get { return this; }
        }

        public override IColumn ColumnHeader
        {
            get { return this; }
        }

        public override int RowIndex
        {
            get { return -1; }
        }

        public override int ColumnIndex
        {
            get { return -1; }
        }
        #endregion

        #region IHeader
        public override IEnumerable<DataObject> Elements
        {
            get
            {
                for (var column = Right; this != column; column = column.Right)
                {
                    yield return column;
                }

                for (var row = Down; this != row; row = row.Down)
                {
                    yield return row;
                    for (var element = row.Right; row != element; element = element.Right)
                    {
                        yield return element;
                    }
                }
            }
        }
        #endregion

        #region IRow
        public int NumberOfColumns
        {
            // TODO: No need to keep a count of columns - unless this is called quite often
            get
            {
                int n = 0;
                for (var col = Right; this != col; col = col.Right)
                {
                    n++;
                }
                return n;
            }
        }

        void IRow.Append(DataObject dataObject)
        {
            if (!(dataObject is IColumn)) throw new ArgumentException("RootObject.IRow.Append argument must be IColumn", "dataObject");

            _AllColumns.Add((ColumnObject)dataObject);

            if (ColumnCover.Primary == ((IColumn)dataObject).ColumnCover)
            {
                Left.Right = dataObject;
                dataObject.Right = this;
                dataObject.Left = Left;
                Left = dataObject;
            }
        }
        #endregion

        #region IColumn
        public ColumnCover ColumnCover
        {
            get { throw new NotImplementedException(); }
        }

        public int NumberOfRows
        {
            // TODO: No need to keep a count of rows - unless this is called quite often
            get
            {
                int n = 0;
                for (var row = Down; this != row; row = row.Down)
                {
                    n++;
                }
                return n;
            }
        }

        void IColumn.Append(DataObject dataObject)
        {
            if (!(dataObject is IRow)) throw new ArgumentException("RootObject.IColumn.Append argument must be IRow", "dataObject");

            _AllRows.Add((RowObject)dataObject);

            Up.Down = dataObject;
            dataObject.Down = this;
            dataObject.Up = Up;
            Up = dataObject;
        }
        #endregion

        #region IRoot

        /// <summary>
        /// Return the row for the given rowIndex.  Throws exception if no such row.
        /// </summary>
        public RowObject GetRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _AllRows.Count)
                throw new IndexOutOfRangeException(String.Format("Row with offset {0} not in matrix", rowIndex));
            return _AllRows[rowIndex];
        }

        IRow IRoot.GetRow(int rowIndex)
        {
            return GetRow(rowIndex);
        }

        /// <summary>
        /// Return the column for the given columnIndex.  Throws exception if no such column.
        /// </summary>
        public ColumnObject GetColumn(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= _AllColumns.Count)
                throw new IndexOutOfRangeException(String.Format("Column with offset {0} not in matrix", columnIndex));
            return _AllColumns[columnIndex];
        }

        IColumn IRoot.GetColumn(int columnIndex)
        {
            return GetColumn(columnIndex);
        }
        #endregion

        public int NumberOfOriginalRows { get { return _AllRows.Count; } }

        public int NumberOfOriginalColumns { get { return _AllColumns.Count; } }

        public int NumberOfOriginalPrimaryColumns { get { return _AllColumns.Count(column => ColumnCover.Primary == column.ColumnCover); } }

        public int NumberOfOriginalSecondaryColumns { get { return _AllColumns.Count(column => ColumnCover.Secondary == column.ColumnCover); } }

        internal static Tuple<RootObject, RowObject[], ColumnObject[]> CreateEmptyMatrix(int nRows, int nPrimaryColumns)
        {
            return CreateEmptyMatrix(nRows, nPrimaryColumns, 0);
        }

        /// <summary>
        /// Creates an empty matrix with the given number of rows and columns (primary and secondary).
        /// </summary>
        internal static Tuple<RootObject, RowObject[], ColumnObject[]> CreateEmptyMatrix(int nRows, int nPrimaryColumns, int nSecondaryColumns)
        {
            var root = RootObject.Create();
            var rows = Enumerable.Range(0, nRows).Select(_ => root.NewRow()).ToArray();
            var primaryColumns = Enumerable.Range(0, nPrimaryColumns).Select(_ => root.NewColumn(ColumnCover.Primary));
            var secondaryColumns = Enumerable.Range(0, nSecondaryColumns).Select(_ => root.NewColumn(ColumnCover.Secondary));
            var allColumns = primaryColumns.Concat(secondaryColumns).ToArray();
            return Tuple.Create(root, rows, allColumns);
        }

        /// <summary>
        /// Add an entire matrix of elements to the data matrix.  Matrix is specified
        /// as a 2D array of boolean.  AddMatrix can only be called on an empty data
        /// matrix.
        /// </summary>
        public void AddMatrix(bool[,] values)
        {
            AddMatrix(values.AsNestedEnumerables());
        }

        /// <summary>
        /// Add an entire matrix of elements to the data matrix.  Matrix is specified
        /// as an array (actually, an enumerable of enumerable) of boolean.  AddMatrix
        /// can only be called on an empty data matrix.
        /// </summary>
        public void AddMatrix(IEnumerable<IEnumerable<bool>> values)
        {
            int row = 0;
            foreach (var rowValues in values)
            {
                if (NumberOfOriginalRows <= row)
                    throw new ArgumentException("too many rows", "values");
                AddRow(_AllRows[row], rowValues);
                row++;
            }
        }

        /// <summary>
        /// Add an entire row of elements to the data matrix.  Row is specified
        /// as a vector (actually, an enumerable) of boolean.  AddRow can only be
        /// called on empty rows.
        /// </summary>
        public void AddRow(RowObject row, IEnumerable<bool> values)
        {
            if (0 != row.NumberOfColumns)
                throw new InvalidOperationException(String.Format("Cannot AddRow to {0} - already has elements", row.ToString()));

            int column = 0;
            foreach (bool b in values)
            {
                if (NumberOfOriginalColumns <= column)
                    throw new ArgumentException(String.Format("row {0} is too long", row.RowIndex), "values");
                if (b)
                {
                    NewElement(row, _AllColumns[column]);
                }
                column++;
            }
        }

        public int HighestColumn
        {
            get
            {
                return Left.ColumnIndex;
            }
        }

        public int HighestRow
        {
            get
            {
                return Up.RowIndex;
            }
        }

        public struct ElementCoordinate
        {
            public readonly int Row;
            public readonly int Column;

            public ElementCoordinate(int row, int column)
            {
                Row = row;
                Column = column;
            }
        }

        /// <summary>
        /// Returns the elements of the data matrix as a sequence of coordinates
        /// (in row-major order). Never includes any elements from secondary columns.
        /// </summary>
        public IEnumerable<ElementCoordinate> ToCoordinates()
        {
            return from element in Elements.OfType<ElementObject>()
                   orderby element.RowIndex, element.ColumnIndex
                   select new ElementCoordinate(element.RowIndex, element.ColumnIndex);
        }

        /// <summary>
        /// Returns the elements of the data matrix as a 2D array of bool.
        /// Never includes any elements from secondary columns.
        /// </summary>
        public bool[,] ToArray()
        {
            var result = new bool[NumberOfRows, NumberOfColumns];
            foreach (var element in Elements.OfType<ElementObject>())
            {
                result[element.RowIndex, element.ColumnIndex] = true;
            }
            return result;
        }

        public override string ToString()
        {
            return String.Format("{0}[{1}x{2}]", Kind, NumberOfRows, NumberOfColumns);
        }

    }
}
