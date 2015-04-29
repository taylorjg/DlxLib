using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DlxLib
{
    /// <summary>
    /// PLACEHOLDER
    /// </summary>
    internal class RootObject : HeaderObject, IRoot
    {
        #region IRow Members

        public int NumberOfColumns
        {
            get { throw new NotImplementedException(); }
        }

        public void Append(DataObject dataObject)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IColumn Members

        public ColumnCover ColumnCover
        {
            get { throw new NotImplementedException(); }
        }

        public int NumberOfRows
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
