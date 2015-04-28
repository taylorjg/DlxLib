using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DlxLib
{
    /// <summary>
    /// Common interface to all Roots.
    /// </summary>
    /// <remarks>
    /// Currently, a marker interface only.
    /// </remarks>
    internal interface IRoot : IHeader, IRow, IColumn
    {
    }
}
