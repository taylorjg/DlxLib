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
        protected internal DataObject()
        {
            Left = Right = Up = Down = this;
        }

        #region IDataObject members
        public virtual RootObject Root { get; protected set;}

        /// <summary>
        /// Returns this object's row's header (a RowObject, or, in the case of a
        /// ColumnObject, the Root).
        /// </summary>
        /// <remarks>
        /// Note that this object will not be in the Elements of its row header
        /// if its column is covered.
        /// </remarks>
        public abstract IRow RowHeader { get; }

        /// <summary>
        /// Returns this object's column's header (a ColumnObjecct, or in the case
        /// of a RowObject, the Root).
        /// </summary>
        /// <remarks>
        /// Note that this object will not be in the Elements of its column header
        /// if its row is covered.
        /// </remarks>
        public abstract IColumn ColumnHeader { get; }

        /// <summary>
        /// Returns the row index (0-based) for this object in the matrix.
        /// (Returns -1 for Root and Column objects.)  The row index is fixed
        /// when the matrix is created (when the object is added to the matrix)
        /// so doesn't change even if this object's row or column is covered.
        /// </summary>
        public abstract int RowIndex { get; }

        /// <summary>
        /// Returns the column index (0-based) for this object in the matrix.
        /// (Returns -1 for Root and Row objects.)  The column index is fixed
        /// when the matrix is created (when the object is added to the matrix)
        /// so doesn't change even if this objects' row or column is covered.
        /// </summary>
        public abstract int ColumnIndex { get; }
        #endregion

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
        /// Returns the kind (subclass name) of this object, suitable for a
        /// ToString() self-description.
        /// </summary>
        public string Kind {
            get
            {
                return GetType().Name.Replace("Object", "");
            }
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
