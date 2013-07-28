namespace DlxLib
{
    internal interface IDataObject
    {
        IDataObject Left { get; set; }
        IDataObject Right { get; set; }
        IDataObject Up { get; set; }
        IDataObject Down { get; set; }
        ColumnObject ListHeader { get; }
        int RowIndex { get; }
        void UnlinkFromColumn();
        void RelinkIntoColumn();
    }
}
