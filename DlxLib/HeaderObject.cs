using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DlxLib
{
    internal abstract class HeaderObject : DataObject, IHeader
    {
        public HeaderObject(RootObject root, ColumnObject listHeader, int rowIndex, int columnIndex)
            : base(root, listHeader, rowIndex, columnIndex)
        {

        }

        public HeaderObject(RootObject root, int columnIndex)
            :base(root, columnIndex)
        {

        }

        public HeaderObject()
            :base()
        {

        }

        #region IHeader Members

        public IEnumerable<DataObject> Elements
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        protected override void ValidateRowIndexInRange(int rowIndex)
        {
            if (-1 > rowIndex)
                throw new ArgumentOutOfRangeException("on Header must be > -1", "rowIndex");
        }

        protected override void ValidateColumnIndexInRange(int columnIndex)
        {
            if (-1 > columnIndex)
                throw new ArgumentOutOfRangeException("on Header must be > -1", "columnIndex");
        }

        public override string Kind
        {
            get { return "Header"; }
        }
    }
}
