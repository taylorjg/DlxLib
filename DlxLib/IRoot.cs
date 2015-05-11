using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DlxLib
{
    /// <summary>
    /// Common interface to all Roots.
    /// </summary>
    internal interface IRoot : IHeader, IRow, IColumn
    {
        /// <summary>
        /// Return the row for the given rowIndex.  Throws exception if no such row.
        /// </summary>
        IRow GetRow(int rowIndex);

        /// <summary>
        /// Return the column for the given columnIndex.  Throws exception if no such column.
        /// </summary>
        IColumn GetColumn(int columnIndex);
    }
}
