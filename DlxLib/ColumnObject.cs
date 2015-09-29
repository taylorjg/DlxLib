namespace DlxLib
{
    internal class ColumnObject : DataObject
    {
        public ColumnObject()
        {
            PreviousColumnObject = NextColumnObject = this;
        }

        private ColumnObject PreviousColumnObject { get; set; }
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

        public void AddDataObject(DataObject dataObject)
        {
            AppendToColumn(dataObject);
            NumberOfRows++;
        }

        public void UnlinkDataObject(DataObject dataObject)
        {
            dataObject.UnlinkFromColumn();
            NumberOfRows--;
        }

        public void RelinkDataObject(DataObject dataObject)
        {
            dataObject.RelinkIntoColumn();
            NumberOfRows++;
        }
    }
}
