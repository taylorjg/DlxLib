﻿using System;
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
    internal class RowObject : HeaderObject, IRow
    {
        public RowObject(RootObject root, int rowIndex)
            : base(root, rowIndex, -1)
        {
        }

        public override IRow RowHeader
        {
            get { return this; }
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

        public override IEnumerable<DataObject> Elements
        {
            get
            {
                for (var element = Right; this != element; element = element.Right)
                    yield return element;
            }
        }

        /// <summary>
        /// Returns largest columnIndex of row elements (-1 if row has no elements)
        /// </summary>
        public int HighestColumnInRow
        {
            get
            {
                return Left.ColumnIndex;
            }
        }

        public void Append(DataObject dataObject)
        {
            Left.Right = dataObject;
            dataObject.Right = this;
            dataObject.Left = Left;
            Left = dataObject;
        }

        #endregion

        protected internal override void ValidateRowIndexAvailableInColumn(RootObject root, int rowIndex, int columnIndex)
        {
            var maxRow = root.HighestRow;
            if (maxRow >= rowIndex)
                throw new ArgumentOutOfRangeException("rowIndex", "Row index too low");
        }

        protected internal override void ValidateColumnIndexAvailableInRow(RootObject root, int rowIndex, int columnIndex)
        {
            if (-1 != columnIndex)
                throw new ArgumentOutOfRangeException("columnIndex", "Must be -1");
        }

        public override string Kind
        {
            get
            {
                return "Row";
            }
        }

        public override string ToString()
        {
            return String.Format("{0}[{1}]", Kind, RowIndex);
        }

        public override IColumn ColumnHeader
        {
            get { return Root; }
        }
    }
}
