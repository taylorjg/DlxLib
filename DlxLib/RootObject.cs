using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

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

        #region IDataObject Members
        public RootObject Root { get { return this; } }

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
            if (ColumnCover.Secondary == ((IColumn)dataObject).ColumnCover)
            {
                NumberOfSecondaryColumns++;
            }
            else
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
            for (var row = Down; this != row; row = row.Down)
                if (rowIndex == row.RowIndex)
                    return (RowObject)row;
            throw new IndexOutOfRangeException(String.Format("Row with offset {0} not in matrix", rowIndex));
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
            for (var column = Right; this != column; column = column.Right)
                if (columnIndex == column.ColumnIndex)
                    return (ColumnObject)column;
            throw new IndexOutOfRangeException(String.Format("Column with offset {0} not in matrix", columnIndex));
        }

        IColumn IRoot.GetColumn(int columnIndex)
        {
            return GetColumn(columnIndex);
        }
        #endregion

        public int NumberOfPrimaryColumns { get { return NumberOfColumns; } }

        public int NumberOfSecondaryColumns { get; private set; }

        internal static Tuple<RootObject, RowObject[], ColumnObject[]> CreateEmptyMatrix(int nRows, int nColumns)
        {
            var root = RootObject.Create();
            var rows = Enumerable.Range(0, nRows).Select(i => root.NewRow()).ToArray();
            foreach (var row in rows)
            {
                (root as IColumn).Append(row);
            }
            var columns = Enumerable.Range(0, nColumns).Select(i => root.NewColumn(ColumnCover.Primary)).ToArray();
            foreach(var column in columns)
            {
                (root as IRow).Append(column);
            }
            return Tuple.Create(root, rows, columns);
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

        public override string ToString()
        {
            return String.Format("{0}[{1}x{2}]", Kind, NumberOfRows, NumberOfColumns);
        }

    }
}
