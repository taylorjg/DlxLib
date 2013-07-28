namespace DlxLib
{
    internal class ColumnHeader : Node
    {
        public ColumnHeader()
        {
            PreviousColumnHeader = NextColumnHeader = this;
        }

        public ColumnHeader PreviousColumnHeader { get; private set; }
        public ColumnHeader NextColumnHeader { get; private set; }
        public int Size { get; private set; }

        public void AppendColumnHeader(ColumnHeader columnHeader)
        {
            PreviousColumnHeader.NextColumnHeader = columnHeader;
            columnHeader.NextColumnHeader = this;
            columnHeader.PreviousColumnHeader = PreviousColumnHeader;
            PreviousColumnHeader = columnHeader;
        }

        public void UnlinkColumnHeader()
        {
            NextColumnHeader.PreviousColumnHeader = PreviousColumnHeader;
            PreviousColumnHeader.NextColumnHeader = NextColumnHeader;
        }

        public void RelinkColumnHeader()
        {
            NextColumnHeader.PreviousColumnHeader = this;
            PreviousColumnHeader.NextColumnHeader = this;
        }

        public void AppendNode(Node node)
        {
            AppendColumnNode(node);
            Size++;
        }

        public void UnlinkNode(Node node)
        {
            node.UnlinkColumnNode();
            Size--;
        }

        public void RelinkNode(Node node)
        {
            node.RelinkColumnNode();
            Size++;
        }
    }
}
