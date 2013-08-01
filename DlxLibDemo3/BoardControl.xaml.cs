using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DlxLibDemo3.Model;
using Orientation = DlxLibDemo3.Model.Orientation;

namespace DlxLibDemo3
{
    public partial class BoardControl
    {
        private readonly IDictionary<char, Tuple<Orientation, int, int, Rectangle[]>> _pieceDetails = new Dictionary<char, Tuple<Orientation, int, int, Rectangle[]>>();

        public BoardControl()
        {
            InitializeComponent();
        }

        public void AddPiece(RotatedPiece rotatedPiece, int x, int y)
        {
            if (IsPieceOnBoard(rotatedPiece.Piece.Name))
            {
                throw new InvalidOperationException(string.Format("Attempt to add a piece that is already on the board - \"{0}\"!", rotatedPiece.Piece.Name));
            }

            var aw = ActualWidth;
            var ah = ActualHeight;
            var sw = aw / 8;
            var sh = ah / 8;

            var rects = new List<Rectangle>();

            for (var px = 0; px < rotatedPiece.Width; px++)
            {
                for (var py = 0; py < rotatedPiece.Height; py++)
                {
                    var square = rotatedPiece.SquareAt(px, py);
                    if (square != null)
                    {
                        var rect = new Rectangle { Width = sw, Height = sh };
                        Canvas.SetLeft(rect, (x + px) * sw);
                        Canvas.SetBottom(rect, (y + py) * sh);
                        rect.Fill = new SolidColorBrush(square.Colour == Colour.Black ? Colors.Black : Colors.White);
                        BoardCanvas.Children.Add(rect);
                        rects.Add(rect);
                    }
                }
            }

            _pieceDetails[rotatedPiece.Piece.Name] = Tuple.Create(rotatedPiece.Orientation, x, y, rects.ToArray());
        }

        public void RemovePiece(char pieceName)
        {
            if (_pieceDetails.ContainsKey(pieceName))
            {
                foreach (var rect in _pieceDetails[pieceName].Item4)
                {
                    BoardCanvas.Children.Remove(rect);
                }
                _pieceDetails.Remove(pieceName);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Attempt to remove a piece that is not on the board - \"{0}\"!", pieceName));
            }
        }

        public void RemovePiecesOtherThan(IList<char> pieceNames)
        {
            var pieceNamesToRemove = new List<char>();

            foreach (var pieceName in _pieceDetails.Keys)
            {
                if (pieceNames.All(pn => pn != pieceName))
                {
                    foreach (var rect in _pieceDetails[pieceName].Item4)
                    {
                        BoardCanvas.Children.Remove(rect);
                    }
                    pieceNamesToRemove.Add(pieceName);
                }
            }

            foreach (var pieceName in pieceNamesToRemove)
            {
                _pieceDetails.Remove(pieceName);
            }
        }

        public bool IsPieceOnBoard(char pieceName)
        {
            return _pieceDetails.ContainsKey(pieceName);
        }

        public bool IsPieceOnBoardWithOrientationandLocation(char pieceName, Orientation orientation, int x, int y)
        {
            if (_pieceDetails.ContainsKey(pieceName))
            {
                var tuple = _pieceDetails[pieceName];
                if (tuple.Item1 == orientation &&
                    tuple.Item2 == x &&
                    tuple.Item3 == y)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
