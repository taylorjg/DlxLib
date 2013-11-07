using DlxLibDemo3.Model;

namespace DlxLibDemo3
{
    public class PiecePlacement
    {
        public PiecePlacement(RotatedPiece rotatedPiece, Coords coords)
        {
            RotatedPiece = rotatedPiece;
            Coords = coords;
        }

        public RotatedPiece RotatedPiece { get; private set; }
        public Coords Coords { get; private set; }
    }
}
