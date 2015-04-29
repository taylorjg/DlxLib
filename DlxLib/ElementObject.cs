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

        }

        protected override void ValidateRowIndexInRange(int rowIndex)
        {
            if (0 > rowIndex)
                throw new ArgumentOutOfRangeException("Must be >= 0", "rowIndex");
        }

        protected override void ValidateColumnIndexInRange(int columnIndex)
        {
            if (0 > columnIndex)
                throw new ArgumentOutOfRangeException("Must be >= 0", "columnIndex");
        }

        public override string Kind
        {
            get { return "Element"; }
        }
    }
}
