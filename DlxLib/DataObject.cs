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
        public DataObject(RootObject root, ColumnObject listHeader, int rowIndex, int columnIndex)
        {
            ValidateRowIndexInRange(root, rowIndex);
            ValidateColumnIndexInRange(root, columnIndex);

            Left = Right = Up = Down = this;
            _Root = root;
            _ColumnHeader = listHeader;
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;

            if (listHeader != null)
            {
                listHeader.AddDataObject(this);
            }
        }

        /// <summary>
        /// Constructor for ColumnObjects only: They don't pass in the ListHeader
        /// because they're their _own_ ListHeader (and they can't reference
        /// themselves in the constructor.  They also don't pass in the rowIndex
        /// because the rowIndex is always -1.
        /// </summary>
        /// <remarks>
        /// I don't know a way, in C# to limit the caller of this constructor to
        /// ColumnObjects only (since I can't reference _this_ in the constructor,
        /// which is the whole problem...)
        /// </remarks>
        public DataObject(RootObject root, int columnIndex)
            : this(root, null, -1, columnIndex)
        {

        }

        /// <summary>
        /// Constructor for RootObjects only: They don't pass in the Root or the
        /// ListHeader because they're their own Root and ListHeader, plus the
        /// row and column indexes are both known (to be -1).
        /// </summary>
        public DataObject()
            : this(null, null, -1, -1)
        {

        }

        /// <summary>
        /// Validate that the supplied rowIndex is in the valid range (which depends
        /// on the Kind).
        /// </summary>
        protected abstract void ValidateRowIndexInRange(RootObject root, int rowIndex);

        /// <summary>
        /// Validate that the supplied columnIndex is in the valid range (which
        /// depends on the Kind).
        /// </summary>
        protected abstract void ValidateColumnIndexInRange(RootObject root, int columnIndex);


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
                if (null != _Root)
                {
                    return _Root;
                }
                // TODO: Awaiting the hook up of RootObject.
                //if (this is RootObject)
                //{
                //    return this as RootObject;
                //}
                // This is a rather late notification that a constructor error
                // occurred:  happens when Root is called, not in constructor.
                throw new InvalidOperationException("Root must not be null except for Root");
            }
        }

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
        /// Backing field for public property ListHeader.
        /// </summary>
        private readonly ColumnObject _ColumnHeader;

        /// <summary>
        /// Returns the column header for this object in the matrix.  (If this object
        /// is a Row then the column header is the Root.)
        /// </summary>
        public ColumnObject ColumnHeader
        {
            get
            {
                if (null != _ColumnHeader)
                {
                    return _ColumnHeader;
                }
                if (this is IColumn)
                {
                    return this as ColumnObject;
                }
                // This is a rather late notification that a constructor error
                // occurred:  happens when ListHeader is called, not in constructor.
                throw new InvalidOperationException("ListHeader must not be null except for Root or Column");
            }
        }

        /// <summary>
        /// Returns the row index (0-based) for this object in the matrix.
        /// (Returns -1 for Root and Column objects.)  The row index is fixed
        /// when the matrix is created (when the object is added to the matrix)
        /// so doesn't change even if this object's row or column is covered.
        /// </summary>
        public int RowIndex { get; private set; }

        /// <summary>
        /// Returns the column index (0-based) for this object in the matrix.
        /// (Returns -1 for Root and Row objects.)  The column index is fixed
        /// when the matrix is created (when the object is added to the matrix)
        /// so doesn't change even if this objects' row or column is covered.
        /// </summary>
        public int ColumnIndex { get; private set; }

        /// <summary>
        /// Obsolete! To be removed!
        /// </summary>
        public void AppendToRow(DataObject dataObject)
        {
            Left.Right = dataObject;
            dataObject.Right = this;
            dataObject.Left = Left;
            Left = dataObject;
        }

        public void UnlinkFromColumn()
        {
            Down.Up = Up;
            Up.Down = Down;
        }

        public void RelinkIntoColumn()
        {
            Down.Up = this;
            Up.Down = this;
        }

        /// <summary>
        /// Returns the kind (subclass name) of this object, suitable for a
        /// ToString() self-description.
        /// </summary>
        public virtual string Kind
        {
            get { return "DataObject"; }
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
