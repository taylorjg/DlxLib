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
        /// <summary>
        /// Construct a HeaderObject (Row or Column) supplying the root.
        /// </summary>
        protected internal HeaderObject(RootObject root)
            : base()
        {
            Root = root;
        }

        /// <summary>
        /// Zero-argument constructor reserved for RootObject (since can't pass
        /// 'this' in to constructor to fill field Root).
        /// </summary>
        protected HeaderObject()
            : base()
        {
            if (!(this is IRoot))
                throw new ArgumentException("HeaderObject 0-arg constructor reserved for RootObject");
        }

        #region IHeader Members
        /// <summary>
        /// Returns all elements of the given header that are currently in the
        /// matrix (that is, covered elements are _not_ returned) - but never
        /// returns itself.  For the Root returns all Rows, Columns, and Elements.
        /// </summary>
        public abstract IEnumerable<IDataObject> Elements { get; }
        #endregion
    }
}
