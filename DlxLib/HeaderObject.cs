using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DlxLib
{
    internal class HeaderObject : DataObject, IHeader
    {
        public HeaderObject(RootObject root, ColumnObject listHeader, int rowIndex, int columnIndex)
            : base(root, listHeader, rowIndex, columnIndex)
        {

        }

        #region IHeader Members

        public IEnumerable<DataObject> Elements
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public override string Kind
        {
            get { return "Header"; }
        }
    }
}
