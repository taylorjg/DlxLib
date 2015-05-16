using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DlxLib
{
    /// <summary>
    /// Common interface for all Rows (including the Root, acting as a row of
    /// Columns).
    /// </summary>
    internal interface IRow : IHeader
    {
        /// <summary>
        /// Returns the number of columns in this Row that are currently in the
        /// matrix (that is, covered columns are _not_ counted).
        /// </summary>
        int NumberOfColumns { get; }

        /// <summary>
        /// Appends an Element to a Row, or a Column to the Root.
        /// </summary>
        /// <remarks>
        /// Note that elements can only be added to the right of all objects already
        /// in the Row.
        /// </remarks>
        void Append(DataObject dataObject);
    }
}
