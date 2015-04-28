namespace DlxLib
{
    internal class ColumnObject : DataObject, IColumn
    {
        public ColumnObject(RootObject root, int columnIndex, ColumnCover columnCover)
            : base(null /*TODO:Root*/, null, -1, columnIndex)
        {
            Init(this);
            ColumnCover = columnCover;
            PreviousColumnObject = NextColumnObject = this;
        }

        public ColumnCover ColumnCover { get; private set; }
        public ColumnObject PreviousColumnObject { get; private set; }
        public ColumnObject NextColumnObject { get; private set; }
        public int NumberOfRows { get; private set; }

        #region IColumn Members


        public void Append(DataObject dataObject)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region IHeader Members

        public System.Collections.Generic.IEnumerable<DataObject> Elements
        {
            get { throw new System.NotImplementedException(); }
        }

        #endregion

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

        public override string Kind
        {
            get { return "Column"; }
        }

    }
}
