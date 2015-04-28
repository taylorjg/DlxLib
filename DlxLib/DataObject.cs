using System;

namespace DlxLib
{
    internal class DataObject : IDataObject
    {
        public DataObject(RootObject root, ColumnObject listHeader, int rowIndex, int columnIndex)
        {
            // TODO: Root must not be null
            // TODO: Second arg to move to type IColumn when IColumn is implemented

            if (null == listHeader && (!(this is RootObject)) && (!(this is ColumnObject)))
                throw new ArgumentNullException("Must not be null except in case of Root or Column", "listHeader");
            // TODO: Following type tests should be against IElement when IElement is hooked up (and they should test for -1 in that case!)
            if (0 > rowIndex && (!(this is RootObject)) && (!(this is ColumnObject)) && (!(this is RowObject)))
                throw new ArgumentOutOfRangeException("Must be >= 0", "rowIndex");
            if (0 > columnIndex && (!(this is RootObject)) && (!(this is ColumnObject)) && (!(this is RowObject)))
                throw new ArgumentOutOfRangeException("Must be >= 0", "columnIndex");

            Left = Right = Up = Down = this;
            Root = root;
            ListHeader = listHeader;
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;

            if (listHeader != null)
            {
                listHeader.AddDataObject(this);
            }
        }

        public void Init(ColumnObject listHeader)
        {
            if (null != ListHeader)
                throw new InvalidOperationException("Can't Init() a ColumnObject if ListHeader already set");
            ListHeader = listHeader;
        }


        public RootObject Root { get; private set; }
        public DataObject Left { get; private set; }
        public DataObject Right { get; private set; }
        public DataObject Up { get; private set; }
        public DataObject Down { get; private set; }
        public ColumnObject ListHeader { get; private set; }
        public int RowIndex { get; private set; }
        public int ColumnIndex { get; private set; }

        public void AppendToRow(DataObject dataObject)
        {
            Left.Right = dataObject;
            dataObject.Right = this;
            dataObject.Left = Left;
            Left = dataObject;
        }

        public void AppendToColumn(DataObject dataObject)
        {
            Up.Down = dataObject;
            dataObject.Down = this;
            dataObject.Up = Up;
            Up = dataObject;
        }

        public void UnlinkFromColumn()
        {
            Down.Up = Up;
            Up.Down = Down;
        }

        public void RelinkIntoColumn()
        {
            Down.Up = this;
            Up.Down = this;
        }

        public virtual string Kind
        {
            get { return "DataObject"; }
        }

        public override string ToString()
        {
            return String.Format("{0}[{1},{2}]", Kind, RowIndex, ColumnIndex);
        }
    }
}
