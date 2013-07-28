namespace DlxLib
{
    internal class ColumnObject : IDataObject
    {
        public ColumnObject()
        {
            Left = Right = Up = Down = this;
            ListHeader = this;
            RowIndex = -1;

            PreviousColumnObject = NextColumnObject = this;
            NumberOfRows = 0;
        }

        public IDataObject Left { get; set; }
        public IDataObject Right { get; set; }
        public IDataObject Up { get; set; }
        public IDataObject Down { get; set; }
        public ColumnObject ListHeader { get; private set; }
        public int RowIndex { get; private set; }
        public void UnlinkFromColumn() {}
        public void RelinkIntoColumn() {}

        public ColumnObject PreviousColumnObject { get; private set; }
        public ColumnObject NextColumnObject { get; private set; }
        public int NumberOfRows { get; private set; }

        public void AppendColumnHeader(ColumnObject columnObject)
        {
            PreviousColumnObject.NextColumnObject = columnObject;
            columnObject.NextColumnObject = this;
            columnObject.PreviousColumnObject = PreviousColumnObject;
            PreviousColumnObject = columnObject;
        }

        public void UnlinkColumnHeader()
        {
            NextColumnObject.PreviousColumnObject = PreviousColumnObject;
            PreviousColumnObject.NextColumnObject = NextColumnObject;
        }

        public void RelinkColumnHeader()
        {
            NextColumnObject.PreviousColumnObject = this;
            PreviousColumnObject.NextColumnObject = this;
        }

        public void AddDataObject(IDataObject dataObject)
        {
            Up.Down = dataObject;
            dataObject.Down = this;
            dataObject.Up = Up;
            Up = dataObject;
            NumberOfRows++;
        }

        public void UnlinkDataObject(IDataObject dataObject)
        {
            dataObject.UnlinkFromColumn();
            NumberOfRows--;
        }

        public void RelinkDataObject(IDataObject dataObject)
        {
            dataObject.RelinkIntoColumn();
            NumberOfRows++;
        }
    }
}
