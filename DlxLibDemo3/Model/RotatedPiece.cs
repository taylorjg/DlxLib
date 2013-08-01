namespace DlxLibDemo3.Model
{
    public class RotatedPiece
    {
        public RotatedPiece(Piece piece, Orientation orientation)
        {
            Piece = piece;
            Orientation = orientation;
        }

        public Piece Piece { get; private set; }
        public Orientation Orientation { get; private set; }

        public int Width
        {
            get
            {
                if (Orientation == Orientation.North || Orientation == Orientation.South)
                    return Piece.Width;

                return Piece.Height;
            }
        }

        public int Height
        {
            get
            {
                if (Orientation == Orientation.North || Orientation == Orientation.South)
                    return Piece.Height;

                return Piece.Width;
            }
        }

        public Square SquareAt(int x, int y)
        {
            switch (Orientation)
            {
                case Orientation.North:
                    return Piece.SquareAt(x, y);

                case Orientation.South:
                    {
                        var square = Piece.SquareAt(Width - x - 1, Height - y - 1);
                        if (square != null)
                            square = new Square(x, y, square.Colour);
                        return square;
                    }

                case Orientation.East:
                    {
                        var square = Piece.SquareAt(Height - y - 1, x);
                        if (square != null)
                            square = new Square(x, y, square.Colour);
                        return square;
                    }

                case Orientation.West:
                    {
                        var square = Piece.SquareAt(y, Width - x - 1);
                        if (square != null)
                            square = new Square(x, y, square.Colour);
                        return square;
                    }

                default:
                    return null;
            }
        }

        public override string ToString()
        {
            return string.Format("Piece Name: {0}; Orientation: {1}", Piece.Name, Orientation);
        }
    }
}
