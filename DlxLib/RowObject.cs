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
    internal class RowObject : HeaderObject, IRow
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
    }
}
