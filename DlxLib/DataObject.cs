namespace DlxLib
{
    internal class DataObject : IDataObject
    {
        public DataObject(ColumnObject listHeader, int rowIndex)
        {
            Left = Right = Up = Down = this;
            ListHeader = listHeader;
            listHeader.AddDataObject(this);
            RowIndex = rowIndex;
        }

        public IDataObject Left { get; set; }
        public IDataObject Right { get; set; }
        public IDataObject Up { get; set; }
        public IDataObject Down { get; set; }
        public ColumnObject ListHeader { get; private set; }
        public int RowIndex { get; private set; }

        public void AppendToRow(DataObject dataObject)
        {
            Left.Right = dataObject;
            dataObject.Right = this;
            dataObject.Left = Left;
            Left = dataObject;
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
