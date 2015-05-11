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
        protected internal ElementObject(RootObject root, RowObject rowHeader, ColumnObject columnHeader)
            : base()
        {
            root.MustNotBeNull("ElementObject constructor param root");
            rowHeader.MustNotBeNull("ElementObject constructor param rowHeader");
            columnHeader.MustNotBeNull("ElementObject constructor param columnHeader");

            Root = root;
            _RowHeader = rowHeader;
            _ColumnHeader = columnHeader;
        }

        #region IDataObject members
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
        /// Return the row index for this element.
        /// </summary>
        public override int RowIndex { get { return RowHeader.RowIndex; } }

        /// <summary>
        /// Return the column index for this element;
        /// </summary>
        public override int ColumnIndex { get { return ColumnHeader.ColumnIndex; } }
        #endregion
    }
}
