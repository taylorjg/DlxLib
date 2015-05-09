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
        #region IRow Members

        public int NumberOfColumns
        {
            get { throw new NotImplementedException(); }
        }

        public void Append(DataObject dataObject)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IColumn Members

        public ColumnCover ColumnCover
        {
            get { throw new NotImplementedException(); }
        }

        public int NumberOfRows
        {
            get { throw new NotImplementedException(); }
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

        #region IHeader Members

        IEnumerable<DataObject> IHeader.Elements
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDataObject Members

        RootObject IDataObject.Root
        {
            get { throw new NotImplementedException(); }
        }

        int IDataObject.RowIndex
        {
            get { throw new NotImplementedException(); }
        }

        int IDataObject.ColumnIndex
        {
            get { throw new NotImplementedException(); }
        }

        string IDataObject.Kind
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IRow Members

        int IRow.NumberOfColumns
        {
            get { throw new NotImplementedException(); }
        }

        void IRow.Append(DataObject dataObject)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IColumn Members

        ColumnCover IColumn.ColumnCover
        {
            get { throw new NotImplementedException(); }
        }

        int IColumn.NumberOfRows
        {
            get { throw new NotImplementedException(); }
        }

        void IColumn.Append(DataObject dataObject)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
