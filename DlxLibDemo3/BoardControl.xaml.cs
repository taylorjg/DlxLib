using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DlxLibDemo3.Model;

namespace DlxLibDemo3
{
    public partial class BoardControl
    {
        public BoardControl()
        {
            InitializeComponent();
        }

        public void DrawPiece(Piece piece, int x, int y)
        {
            var aw = ActualWidth;
            var ah = ActualHeight;
            var sw = aw / 8;
            var sh = ah / 8;

            for (var px = 0; px < piece.Width; px++)
            {
                for (var py = 0; py < piece.Height; py++)
                {
                    var square = piece.SquareAt(px, py);
                    if (square != null)
                    {
                        var rect = new Rectangle { Width = sw, Height = sh };
                        Canvas.SetLeft(rect, px * sw);
                        Canvas.SetBottom(rect, py * sh);
                        rect.Fill = new SolidColorBrush(square.Colour == Colour.Black ? Colors.Black : Colors.White);
                        BoardCanvas.Children.Add(rect);
                    }
                }
            }
        }
    }
}
