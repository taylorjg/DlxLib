using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DlxLib
{
    /// <summary>
    /// Row Header object - linked into to each Row of the matrix, identifies the Row.
    /// </summary>
    /// <remarks>
    /// Knuth's DLX algorithm didn't have these objects as they're unnecessary for
    /// the DLX algorithm itself.  But they help during matrix setup and printing and
    /// debugging.
    /// </remarks>
    internal class RowObject : HeaderObject, IRow
    {
        protected internal RowObject(RootObject root)
            : base(root)
        {
            _RowIndex = root.NumberOfRows;
            (root as IColumn).Append(this);
        }

        #region IDataObject
        public override IRow RowHeader
        {
            get { return this; }
        }

        public override IColumn ColumnHeader
        {
            get { return Root; }
        }

        private readonly int _RowIndex;
        public override int RowIndex { get { return _RowIndex; } }

        public override int ColumnIndex { get { return -1; } }
        #endregion

        #region IHeader
        public override IEnumerable<IDataObject> Elements
        {
            get
            {
                return NextFromHere(d => d.Right);
            }
        }
        #endregion

        #region IRow
        public int NumberOfColumns
        {
            // No need to keep a count of columns - unless this is called quite often
            get
            {
                int n = 0;
                for (var col = Right; this != col; col = col.Right)
                {
                    n++;
                }
                return n;
            }
        }

        public void Append(DataObject dataObject)
        {
            // TODO: Could validate (again) that dataObject.RowIndex > max in row
            Left.Right = dataObject;
            dataObject.Right = this;
            dataObject.Left = Left;
            Left = dataObject;
        }
        #endregion

        #region DataObject

        #endregion

        /// <summary>
        /// Returns largest columnIndex of row elements (-1 if row has no elements)
        /// </summary>
        public int HighestColumnInRow
        {
            get { return Left.ColumnIndex; }
        }

        public override string ToString()
        {
            return String.Format("{0}[{1}]", Kind, RowIndex);
        }

    }
}

