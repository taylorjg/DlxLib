using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DlxLib
{
    /// <summary>
    /// Represents a true (1) value in a matrix (it is in a matrix iff it is
    /// reachable from the Root).
    /// </summary>
    internal class ElementObject : DataObject, IElement
    {
        public ElementObject(RootObject root, RowObject rowHeader, ColumnObject columnHeader)
            : base(root, rowHeader.RowIndex, columnHeader.ColumnIndex)
        {
            root.MustNotBeNull("ElementObject constructor param root");
            rowHeader.MustNotBeNull("ElementObject constructor param rowHeader");
            columnHeader.MustNotBeNull("ElementObject constructor param columnHeader");

            _RowHeader = rowHeader;
            _ColumnHeader = columnHeader;
        }

        #region IDataObject members
        public override string Kind
        {
            get { return "Element"; }
        }
        #endregion

        #region DataObject members
        protected internal override void ValidateRowIndexAvailableInColumn(RootObject root, int rowIndex, int columnIndex)
        {
            var column = root.GetColumn(columnIndex);
            var maxRowInColumn = column.HighestRowInColumn;
            if (maxRowInColumn >= rowIndex)
                throw new ArgumentOutOfRangeException("rowIndex", "Row index too low");
        }

        protected internal override void ValidateColumnIndexAvailableInRow(RootObject root, int rowIndex, int columnIndex)
        {
            var row = root.GetRow(rowIndex);
            var maxColumnInRow = row.HighestColumnInRow;
            if (maxColumnInRow >= columnIndex)
                throw new ArgumentOutOfRangeException("columnIndex", "Column index too low");
        }

        /// <summary>
        /// Backing field for public property ColumnHeader.
        /// </summary>
        private readonly IColumn _ColumnHeader;

        /// <summary>
        /// Return the column header for this element.
        /// </summary>
        public override IColumn ColumnHeader
        {
            get { return _ColumnHeader; }
        }

        /// <summary>
        /// Backing field for public property RowHeader.
        /// </summary>
        private readonly IRow _RowHeader;

        /// <summary>
        /// Return the row header for this element.
        /// </summary>
        public override IRow RowHeader
        {
            get { return _RowHeader; }
        }
        #endregion
    }
}
