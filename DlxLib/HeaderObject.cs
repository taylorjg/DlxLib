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
        public HeaderObject(RootObject root, int rowIndex, int columnIndex)
            : base(root, rowIndex, columnIndex)
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

        public override string Kind
        {
            get { throw new NotImplementedException("HeaderObject is abstract class: should never ask for the Kind"); }
        }
    }
}
