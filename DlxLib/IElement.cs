using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DlxLib
{
    /// <summary>
    /// Common interface to all matrix elements.  If the Element at a given
    /// row/column index is present in the matrix (reachable from Root) then the
    /// value of the matrix at that position is true (1), otherwise it is false (0).
    /// </summary>
    /// <remarks>
    /// Currently, a marker interface only.
    /// </remarks>
    internal interface IElement : IDataObject
    {
    }
}
