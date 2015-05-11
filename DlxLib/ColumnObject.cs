using System;
using System.Collections.Generic;

namespace DlxLib
{
    /// <summary>
    /// Column header object: Linked into each column, identifies the column.
    /// </summary>
    /// <remarks>
    /// Used in the DLX algorithm itself so that there's a way to figure out which
    /// column has the least number of rows uncovered, so as to pick that one to
    /// cover next.
    /// </remarks>
    internal class ColumnObject : HeaderObject, IColumn
    {
        protected internal ColumnObject(RootObject root, ColumnCover columnCover)
            : base(root)
        {
            ColumnCover = columnCover;
            _ColumnIndex = root.NumberOfPrimaryColumns + root.NumberOfSecondaryColumns;
            if (ColumnCover.Primary == columnCover)
            {
                (root as IRow).Append(this);
            }
        }

        #region IDataObject members
        public override IRow RowHeader
        {
            get { return Root; }
        }

        public override IColumn ColumnHeader
        {
            get { return this; }
        }

        public override int RowIndex { get { return -1; } }

        private readonly int _ColumnIndex;
        public override int ColumnIndex { get { return _ColumnIndex; } }
        #endregion

        #region IHeader members
        public override IEnumerable<DataObject> Elements
        {
            get
            {
                for (var element = Down; this != element; element = element.Down)
                {
                    yield return element;
                }
            }
        }
        #endregion

        #region IColumn members
        public ColumnCover ColumnCover { get; private set; }

        public int NumberOfRows { get; private set; }

        public void Append(DataObject dataObject)
        {
            Up.Down = dataObject;
            dataObject.Down = this;
            dataObject.Up = Up;
            Up = dataObject;
            NumberOfRows++;
        }
        #endregion

        /// <summary>
        /// Returns largest rowIndex of column elements (-1 if column has no elements)
        /// </summary>
        public int HighestRowInColumn
        {
            get
            {
                return Up.RowIndex;
            }
        }

        public override string ToString()
        {
            return String.Format("{0}[{1},{2}]", Kind, ColumnIndex, ColumnCover);
        }

        #region Obsolete
        [Obsolete]
        public void AppendColumnHeader(ColumnObject columnObject)
        {
            Left.Right = columnObject;
            columnObject.Right = this;
            columnObject.Left = Left;
            Left = columnObject;
        }

        [Obsolete]
        public void UnlinkColumnHeader()
        {
            Right.Left = Left;
            Left.Right = Right;
        }

        [Obsolete]
        public void RelinkColumnHeader()
        {
            Right.Left = this;
            Left.Right = this;
        }

        [Obsolete]
        public void AddDataObject(DataObject dataObject)
        {
            Append(dataObject);
        }

        [Obsolete]
        public void UnlinkDataObject(DataObject dataObject)
        {
            dataObject.UnlinkFromColumn();
            NumberOfRows--;
        }

        [Obsolete]
        public void RelinkDataObject(DataObject dataObject)
        {
            dataObject.RelinkIntoColumn();
            NumberOfRows++;
        }
        #endregion
    }
}
