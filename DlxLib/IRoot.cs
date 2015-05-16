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

        /// <summary>
        /// Return the element at the given coordinate.  Throws exception if no such element.
        /// </summary>
        IElement GetElement(int rowIndex, int columnIndex);

        /// <summary>
        /// Cover a column (a step of the DLX algorithm).
        /// </summary>
        void Cover(int columnIndex);

        /// <summary>
        /// Uncover a column (a step of the DLX algorithm).
        /// </summary>
        void Uncover(int columnIndex);
    }
}
