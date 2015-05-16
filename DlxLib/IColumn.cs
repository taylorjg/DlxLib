using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DlxLib
{
    /// <summary>
    /// Specifies whether a Column is Primary or Secondary.  Primary columns must
    /// be covered by exactly one row.  Secondary columns are covered by at most
    /// one row.
    /// </summary>
    /// <remarks>Secondary columns are never linked into the data matrix, thus,
    /// secondary Columns are never returned when from the Elements property on a
    /// Root, and elements in a secondary column are never returned from Elements
    /// on Root or Row.  For more information on secondary vs primary, see Knuth's
    /// paper page 17 where he describes a "generalized cover problem".
    /// </remarks>
    internal enum ColumnCover { Primary, Secondary };

    /// <summary>
    /// Common interface for all Columns (including the Root, acting as a column
    /// of Rows).
    /// </summary>
    internal interface IColumn : IHeader
    {
        /// <summary>
        /// Returns whether this Column is Primary or Secondary.
        /// </summary>
        ColumnCover ColumnCover { get; }

        /// <summary>
        /// Returns the number of rows in this Column that are currently in the
        /// matrix (that is, covered rows are _not_ counted).
        /// </summary>
        int NumberOfRows { get; }

        /// <summary>
        /// Appends an Element to a Column, or a Row to the Root.
        /// </summary>
        /// <remarks>
        /// Note that elements can only be added at the bottom of all objects
        /// already in the Column.
        /// </remarks>
        void Append(DataObject dataObject);

    }
}
