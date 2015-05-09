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
        public ElementObject(RootObject root, ColumnObject listHeader, int rowIndex, int columnIndex)
            : base(root, listHeader, rowIndex, columnIndex)
        {
            root.MustNotBeNull("ElementObject constructor param root");
            listHeader.MustNotBeNull("ElementObject constructor param listHeader");
        }

        protected override void ValidateRowIndexInRange(RootObject root, int rowIndex)
        {
            var row = root.GetRow(rowIndex);
            var maxInRow = row.HighestColumnInRow;
            if (maxInRow >= rowIndex)
                throw new ArgumentOutOfRangeException("Must be >= 0", "rowIndex");
        }

        protected override void ValidateColumnIndexInRange(RootObject root, int columnIndex)
        {
            var col = root.GetColumn(columnIndex);
            var maxInCol = col.HighestRowInColumn;
            if (maxInCol > columnIndex)
                throw new ArgumentOutOfRangeException("Must be >= 0", "columnIndex");
        }

        public override string Kind
        {
            get { return "Element"; }
        }
    }
}
