using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DlxLib
{
    /// <summary>
    /// Base class for all header objects (Root, Row, Column).
    /// </summary>
    internal abstract class HeaderObject : DataObject, IHeader
    {
        protected HeaderObject(RootObject root, int rowIndex, int columnIndex)
            : base(root, rowIndex, columnIndex)
        {

        }

        #region IHeader Members

        public abstract IEnumerable<DataObject> Elements { get; }

        #endregion
    }
}
