using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DlxLib
{
    /// <summary>
    /// Represents a true (1) value in a matrix (it is in a matrix iff it is
    /// reachable from the Root).
    /// </summary>
    internal class ElementObject : DataObject, IElement
    {
        public ElementObject(RootObject root, ColumnObject listHeader, int rowIndex, int columnIndex)
            : base(root, listHeader, rowIndex, columnIndex)
        {

        }

        public override string Kind
        {
            get { return "Element"; }
        }
    }
}
