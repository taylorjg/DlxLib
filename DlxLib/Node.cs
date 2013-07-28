namespace DlxLib
{
    internal class Node
    {
        public Node(ColumnHeader columnHeader, int rowIndex)
        {
            Left = Right = Up = Down = this;
            ColumnHeader = columnHeader;
            RowIndex = rowIndex;

            if (columnHeader != null)
            {
                columnHeader.AppendNode(this);
            }
        }

        // TODO: this is a bit ugly...where is this default constructor used anyway ?
        protected Node()
            : this(null, -1)
        {
        }

        public Node Left { get; private set; }
        public Node Right { get; private set; }
        public Node Up { get; private set; }
        public Node Down { get; private set; }
        public ColumnHeader ColumnHeader { get; private set; }
        public int RowIndex { get; private set; }

        public void AppendRowNode(Node node)
        {
            Left.Right = node;
            node.Right = this;
            node.Left = Left;
            Left = node;
        }

        public void AppendColumnNode(Node node)
        {
            Up.Down = node;
            node.Down = this;
            node.Up = Up;
            Up = node;
        }

        public void UnlinkColumnNode()
        {
            Down.Up = Up;
            Up.Down = Down;
        }

        public void RelinkColumnNode()
        {
            Down.Up = this;
            Up.Down = this;
        }
    }
}
