namespace DlxLib
{
    internal class DataObject
    {
        public DataObject(ColumnObject listHeader, int rowIndex)
        {
            Left = Right = Up = Down = this;
            ListHeader = listHeader;
            RowIndex = rowIndex;

            listHeader?.AddDataObject(this);
        }

        // TODO: this is a bit ugly...
        // LSP problem here ?
        protected DataObject()
            : this(null, -1)
        {
        }

        public DataObject Left { get; private set; }
        public DataObject Right { get; private set; }
        public DataObject Up { get; private set; }
        public DataObject Down { get; private set; }
        public ColumnObject ListHeader { get; private set; }
        public int RowIndex { get; private set; }

        public void AppendToRow(DataObject dataObject)
        {
            Left.Right = dataObject;
            dataObject.Right = this;
            dataObject.Left = Left;
            Left = dataObject;
        }

        protected void AppendToColumn(DataObject dataObject)
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
    }
}
