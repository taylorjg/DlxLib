using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DlxLibDemo3.Model;

namespace DlxLibDemo3
{
    public partial class PieceControl
    {
        private readonly RotatedPiece _rotatedPiece;
        private readonly double _squareSize;

        public PieceControl(RotatedPiece rotatedPiece, double squareSize)
        {
            _rotatedPiece = rotatedPiece;
            _squareSize = squareSize;
            InitializeComponent();

            Width = squareSize * rotatedPiece.Width;
            Height = squareSize * rotatedPiece.Height;

            for (var px = 0; px < rotatedPiece.Width; px++)
            {
                for (var py = 0; py < rotatedPiece.Height; py++)
                {
                    var square = rotatedPiece.SquareAt(px, py);
                    if (square != null)
                    {
                        var rectangle = new Rectangle { Width = squareSize, Height = squareSize };
                        Canvas.SetLeft(rectangle, px * squareSize);
                        Canvas.SetBottom(rectangle, py * squareSize);
                        rectangle.Fill = new SolidColorBrush(square.Colour == Colour.Black ? Colors.Black : Colors.White);
                        PieceCanvas.Children.Add(rectangle);
                    }
                }
            }
        }
    }
}
