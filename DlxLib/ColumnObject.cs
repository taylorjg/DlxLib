using System;
using System.Collections.Generic;

namespace DlxLib
{
    internal class ColumnObject : HeaderObject, IColumn
    {
        public ColumnObject(RootObject root, int columnIndex, ColumnCover columnCover)
            : base(root, columnIndex)
        {
            ColumnCover = columnCover;
        }

        public ColumnCover ColumnCover { get; private set; }
        public int NumberOfRows { get; private set; }

        #region IColumn Members


        public void Append(DataObject dataObject)
        {
            Up.Down = dataObject;
            dataObject.Down = this;
            dataObject.Up = Up;
            Up = dataObject;
            NumberOfRows++;
        }

        #endregion

        #region IHeader Members

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

        public void AppendColumnHeader(ColumnObject columnObject)
        {
            Left.Right = columnObject;
            columnObject.Right = this;
            columnObject.Left = Left;
            Left = columnObject;
        }

        public void UnlinkColumnHeader()
        {
            Right.Left = Left;
            Left.Right = Right;
        }

        public void RelinkColumnHeader()
        {
            Right.Left = this;
            Left.Right = this;
        }

        public void AddDataObject(DataObject dataObject)
        {
            Append(dataObject);
        }

        public void UnlinkDataObject(DataObject dataObject)
        {
            dataObject.UnlinkFromColumn();
            NumberOfRows--;
        }

        public void RelinkDataObject(DataObject dataObject)
        {
            dataObject.RelinkIntoColumn();
            NumberOfRows++;
        }

        protected internal override void ValidateRowIndexAvailable(RootObject root, int rowIndex)
        {
            if (-1 != rowIndex)
                throw new ArgumentOutOfRangeException("rowIndex", "Must be -1");
        }

        protected internal override void ValidateColumnIndexAvailable(RootObject root, int columnIndex)
        {
            var maxColumn = root.HighestColumn;
            if (maxColumn >= columnIndex)
                throw new ArgumentOutOfRangeException("columnIndex", "Column index too low");
        }
        public override string Kind
        {
            get { return "Column"; }
        }

        public override string ToString()
        {
            return String.Format("{0}[{1},{2},{3}]", Kind, RowIndex, ColumnIndex, ColumnCover);
        }

    }
}
