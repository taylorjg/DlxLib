using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DlxLib
{
    /// <summary>
    /// Common interface to all matrix header elements: Root, Row, and Column.
    /// </summary>
    internal interface IHeader : IDataObject
    {
        /// <summary>
        /// Returns all elements of the given header that are currently in the
        /// matrix (that is, covered elements are _not_ returned) - but never
        /// returns itself.  For the Root returns all Rows, Primary Columns, and
        /// Elements.
        /// </summary>
        IEnumerable<DataObject> Elements { get; } // TODO: Since this is an external interface: return IEnumerable<IDataObject>?
    }
}
