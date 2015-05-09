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

        public abstract IEnumerable<DataObject> Elements { get; }

        #endregion

        protected override void ValidateRowIndexInRange(RootObject root, int rowIndex)
        {
            if (-1 > rowIndex)
                throw new ArgumentOutOfRangeException("on Header must be > -1", "rowIndex");
        }

        protected override void ValidateColumnIndexInRange(RootObject root, int columnIndex)
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
