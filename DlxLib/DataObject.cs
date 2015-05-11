using System;

namespace DlxLib
{
    /// <summary>
    /// Base class for all matrix object: Root, Row, Column, and Element.  Knows
    /// its row/column index in the matrix.  Knows its Left/Right/Up/Down neighbors
    /// (these properties are _not_ defined in an interface, and they're not virtual,
    /// so they're faster for all users (by an indirect call).  Knows the Root of
    /// the data matrix.  Knows its "head of column", the ListHeader.
    /// </summary>
    /// <remarks>
    /// Directly knowing the Root and its own row/column index is important for
    /// diagnostics and printing when the object is in a covered row/column. It
    /// isn't necessary for computing the cover set.  Which is why Knuth's version
    /// doesn't include this information in each matrix element.
    /// </remarks>
    internal abstract class DataObject : IDataObject
    {
        protected DataObject(RootObject root, int rowIndex, int columnIndex)
        {
            ValidateRowIndexAvailableInColumn(root, rowIndex, columnIndex);
            ValidateColumnIndexAvailableInRow(root, rowIndex, columnIndex);

            Left = Right = Up = Down = this;
            _Root = root;
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
        }

        #region IDataObject members
        /// <summary>
        /// Backing field for public property Root.
        /// </summary>
        private readonly RootObject _Root;
        /// <summary>
        /// Returns the Root object of the matrix that this DataObject is part of.
        /// </summary>
        public RootObject Root
        {
            get
            {
                if (this is IRoot)
                    return this as RootObject;
                return _Root;
            }
        }

        /// <summary>
        /// Returns the row index (0-based) for this object in the matrix.
        /// (Returns -1 for Root and Column objects.)  The row index is fixed
        /// when the matrix is created (when the object is added to the matrix)
        /// so doesn't change even if this object's row or column is covered.
        /// </summary>
        public virtual int RowIndex { get; private set; }

        /// <summary>
        /// Returns the column index (0-based) for this object in the matrix.
        /// (Returns -1 for Root and Row objects.)  The column index is fixed
        /// when the matrix is created (when the object is added to the matrix)
        /// so doesn't change even if this objects' row or column is covered.
        /// </summary>
        public virtual int ColumnIndex { get; private set; }

        /// <summary>
        /// Returns the kind (subclass name) of this object, suitable for a
        /// ToString() self-description.
        /// </summary>
        public abstract string Kind { get; }

        #endregion

        /// <summary>
        /// Validate that the supplied rowIndex is available in the row for a new
        /// matrix data object (which depends on the Kind).
        /// </summary>
        protected internal abstract void ValidateRowIndexAvailableInColumn(RootObject root, int rowIndex, int columnIndex);

        /// <summary>
        /// Validate that the supplied columnIndex is available in the column for
        /// a new matrix data object (which depends on the Kind).
        /// </summary>
        protected internal abstract void ValidateColumnIndexAvailableInRow(RootObject root, int rowIndex, int columnIndex);


        /// <summary>
        /// Returns the left-wise object from this object.  (List is circular.)  Links rows.
        /// </summary>
        public DataObject Left { get; internal set; }

        /// <summary>
        /// Returns the right-wise object from this object.  (List is circular.) Links rows.
        /// </summary>
        public DataObject Right { get; internal set; }

        /// <summary>
        /// Returns the up-ward object from this object.  (List is circular.) Links columns.
        /// </summary>
        public DataObject Up { get; internal set; }

        /// <summary>
        /// Returns the down-ward object from this object.  (List is circular.) Links columns.
        /// </summary>
        public DataObject Down { get; internal set; }

        /// <summary>
        /// Returns the row header for this object in the matrix.  (If this object
        /// is a Column then the column header is the Root.)
        /// </summary>
        /// Note that this object will not be in the Elements of its row header
        /// if a) its column is covered or b) it is a column header of a Secondary
        /// column.
        public abstract IRow RowHeader { get; }

        /// <summary>
        /// Returns the column header for this object in the matrix.  (If this object
        /// is a Row then the column header is the Root.)
        /// </summary>
        /// <remarks>
        /// Note that this object will not be in the Elements of its column header
        /// if its row is covered.
        /// </remarks>
        public abstract IColumn ColumnHeader { get; }

        [Obsolete]
        public void AppendToRow(DataObject dataObject)
        {
            IRow row = Root.GetRow(RowIndex);
            row.Append(dataObject);
        }

        [Obsolete]
        public void UnlinkFromColumn()
        {
            Down.Up = Up;
            Up.Down = Down;
        }

        [Obsolete]
        public void RelinkIntoColumn()
        {
            Down.Up = this;
            Up.Down = this;
        }


        /// <summary>
        /// A DataObject self-displays as its Kind (Root, Row, Column, Element)
        /// and its 2-D location in the matrix (row, column).  But subclasses can
        /// override for a better self-description.
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0}[{1},{2}]", Kind, RowIndex, ColumnIndex);
        }
    }
}
