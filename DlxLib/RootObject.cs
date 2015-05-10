using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DlxLib
{
    /// <summary>
    /// PLACEHOLDER
    /// </summary>
    internal class RootObject : HeaderObject, IRoot
    {
        public RootObject()
            : base(null, -1, -1)
        {

        }

        public override IRow RowHeader
        {
            get { return this; }
        }

        internal static Tuple<RootObject, RowObject[], ColumnObject[]> CreateEmptyMatrix(int nRows, int nColumns)
        {
            var root = new RootObject();
            var rows = Enumerable.Range(0, nRows).Select(i => new RowObject(root, i)).ToArray();
            foreach (var row in rows)
            {
                (root as IColumn).Append(row);
            }
            var columns = Enumerable.Range(0, nColumns).Select(i => new ColumnObject(root, i, ColumnCover.Primary)).ToArray();
            foreach(var column in columns)
            {
                (root as IRow).Append(column);
            }
            return Tuple.Create(root, rows, columns);
        }

        #region IRow Members

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

        #endregion

        #region IColumn Members

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

        #endregion

        public override IEnumerable<DataObject> Elements
        {
            get
            {
                for (var column = Right; this != column; column = column.Right)
                    yield return column;
                for (var row = Down; this != row; row = row.Down)
                {
                    yield return row;
                    for (var element = row.Right; row != element; element = element.Right)
                        yield return element;
                }
            }
        }

        #region IRoot Members

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

        IColumn IRoot.GetColumn(int columnIndex)
        {
            return GetColumn(columnIndex);
        }

        IRow IRoot.GetRow(int rowIndex)
        {
            return GetRow(rowIndex);
        }

        #endregion

        #region IDataObject Members

        public override int RowIndex
        {
            get { return -1; }
        }

        public override int ColumnIndex
        {
            get { return -1; }
        }

        public override string Kind
        {
            get { return "Root"; }
        }

        #endregion

        #region IRow Members

        void IRow.Append(DataObject dataObject)
        {
            Left.Right = dataObject;
            dataObject.Right = this;
            dataObject.Left = Left;
            Left = dataObject;
        }

        #endregion

        #region IColumn Members

        void IColumn.Append(DataObject dataObject)
        {
            Up.Down = dataObject;
            dataObject.Down = this;
            dataObject.Up = Up;
            Up = dataObject;
        }

        #endregion

        protected internal override void ValidateRowIndexAvailableInColumn(RootObject root, int rowIndex, int columnIndex)
        {
            if (-1 != rowIndex)
                throw new ArgumentOutOfRangeException("rowIndex", "Must be -1");
        }

        protected internal override void ValidateColumnIndexAvailableInRow(RootObject root, int rowIndex, int columnIndex)
        {
            if (-1 != columnIndex)
                throw new ArgumentOutOfRangeException("columnIndex", "Must be -1");
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

        public override IColumn ColumnHeader
        {
            get { return this; }
        }
    }
}
