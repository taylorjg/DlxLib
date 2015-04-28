using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DlxLib
{
    /// <summary>
    /// Common interface to all matrix objects: root, row, column, and element.
    /// </summary>
    /// <remarks>
    /// Also, all matrix object - root, row, column, and element - derive from
    /// DataObject.  So the really frequently used methods/properties/fields which
    /// are common to all matrix objects are _not_ in this interface - because if
    /// they were they'd need to be virtual (a small potential slowdown).  (Note
    /// that because of the way C# interfaces work, anything in the interface is
    /// virtual but also all implementations are sealed - so that any call to
    /// something defined in an interface to an object typed to the class, not
    /// the interface, is direct.  Only calls through the interface type are
    /// actually indirect through the virtual table (or so the books say).
    /// </remarks>
    internal interface IDataObject
    {
        /// <summary>
        /// Returns the Root of this matrix.
        /// </summary>
        RootObject Root { get; }

        /// <summary>
        /// Returns the row index (0-based) of this matrix object.  Column objects
        /// and Root objects return -1.
        /// </summary>
        /// <remarks>
        /// The index is fixed at matrix creation time and is unchanged even as
        /// rows are covered and removed from the matrix while solving the cover
        /// problem.
        /// </remarks>
        int RowIndex { get; }

        /// <summary>
        /// Returns the column index (0-based) of this matrix object.  Row objects
        /// and Root objects return -1.
        /// </summary>
        /// <remarks>
        /// The index is fixed at matrix creation time and is unchanged even as
        /// columns are covered and removed from the matrix while solving the cover
        /// problem.
        /// </remarks>
        int ColumnIndex { get; }

        /// <summary>
        /// Simple name for the kind of object (used for string representations).
        /// </summary>
        string Kind { get; }
    }
}
