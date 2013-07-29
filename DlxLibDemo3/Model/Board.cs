using System;

namespace DlxLibDemo3.Model
{
    public class Board
    {
        private class PieceHolder
        {
            public RotatedPiece RotatedPiece { get; set; }
            public Square Square { get; set; }
        }

        private readonly PieceHolder[,] _pieces;

        public Board(int boardSize)
        {
            BoardSize = boardSize;
            _pieces = new PieceHolder[BoardSize, BoardSize];
        }

        public int BoardSize { get; private set; }
        public Colour? ColourOfSquareZeroZero { get; private set; }

        public void ForceColourOfSquareZeroZeroToBeBlack()
        {
            ColourOfSquareZeroZero = Colour.Black;
        }

        public void ForceColourOfSquareZeroZeroToBeWhite()
        {
            ColourOfSquareZeroZero = Colour.White;
        }

        public bool PlacePieceAt(RotatedPiece rotatedPiece, int x, int y)
        {
            if (x < 0 || x >= BoardSize) throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= BoardSize) throw new ArgumentOutOfRangeException("y");

            if (x + rotatedPiece.Width > BoardSize) return false;
            if (y + rotatedPiece.Height > BoardSize) return false;

            // Check for any overlap with existing pieces before setting any squares.
            for (var pieceX = 0; pieceX < rotatedPiece.Width; pieceX++) {
                for (var pieceY = 0; pieceY < rotatedPiece.Height; pieceY++) {
                    var square = rotatedPiece.SquareAt(pieceX, pieceY);
                    if (square != null) {
                        if (_pieces[x + pieceX, y + pieceY] != null)
                            return false;
                    }
                }
            }

            if (ColourOfSquareZeroZero.HasValue) {

                var colourOfSquareZeroZero = ColourOfSquareZeroZero.Value;

                // Check that each square of the piece to be placed has
                // the appropriate colour for its intended board position.
                for (var pieceX = 0; pieceX < rotatedPiece.Width; pieceX++) {
                    for (var pieceY = 0; pieceY < rotatedPiece.Height; pieceY++) {
                        var square = rotatedPiece.SquareAt(pieceX, pieceY);
                        if (square != null) {
                            var boardX = x + pieceX;
                            var boardY = y + pieceY;
                            var expectedColour = colourOfSquareZeroZero.RelativeColour(boardX, boardY);
                            if (square.Colour != expectedColour)
                                return false;
                        }
                    }
                }
            }

            Square firstSquare = null;

            // It's now OK to go ahead and set the squares for the new piece.
            for (var pieceX = 0; pieceX < rotatedPiece.Width; pieceX++) {
                for (var pieceY = 0; pieceY < rotatedPiece.Height; pieceY++) {
                    var square = rotatedPiece.SquareAt(pieceX, pieceY);
                    if (square != null) {
                        // Remember the first square in case we haven't set _colourOfSquareZeroZero yet.
                        if (firstSquare == null)
                            firstSquare = square;
                        _pieces[x + pieceX, y + pieceY] = new PieceHolder { RotatedPiece = rotatedPiece, Square = square };
                    }
                }
            }

            if (!ColourOfSquareZeroZero.HasValue && firstSquare != null) {
                var boardX = x + firstSquare.X;
                var boardY = y + firstSquare.Y;
                ColourOfSquareZeroZero = firstSquare.Colour.RelativeColour(boardX, boardY);
            }

            return true;
        }

        public bool PlacePieceAt(Piece piece, int x, int y)
        {
            return PlacePieceAt(new RotatedPiece(piece, Orientation.North), x, y);
        }

        public bool LayoutNextPiece(RotatedPiece rotatedPiece)
        {
            int firstEmptyX;
            int firstEmptyY;

            var foundAnEmptySquare = FindFirstEmptySquare(out firstEmptyX, out firstEmptyY);

            if (!foundAnEmptySquare)
                throw new InvalidOperationException("The puzzle is already solved!");

            return PlacePieceAt(rotatedPiece, firstEmptyX, firstEmptyY);
        }

        private bool FindFirstEmptySquare(out int firstEmptyX, out int firstEmptyY)
        {
            firstEmptyX = -1;
            firstEmptyY = -1;

            for (var y = 0; y < BoardSize; y++)
            {
                for (var x = 0; x < BoardSize; x++)
                {
                    var square = SquareAt(x, y);
                    if (square == null)
                    {
                        firstEmptyX = x;
                        firstEmptyY = y;
                        return true;
                    }
                }
            }

            return false;
        }

        public Piece PieceAt(int x, int y)
        {
            PieceHolder pieceHolder = PieceHolderAt(x, y);
            return (pieceHolder != null) ? pieceHolder.RotatedPiece.Piece : null;
        }

        public Square SquareAt(int x, int y)
        {
            PieceHolder pieceHolder = PieceHolderAt(x, y);
            return (pieceHolder != null) ? pieceHolder.Square : null;
        }

        private PieceHolder PieceHolderAt(int x, int y)
        {
            if (x < 0 || x >= BoardSize) throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= BoardSize) throw new ArgumentOutOfRangeException("y");

            return _pieces[x, y];
        }

        public void Reset()
        {
            for (var x = 0; x < BoardSize; x++) {
                for (var y = 0; y < BoardSize; y++) {
                    _pieces[x, y] = null;
                }
            }

            ColourOfSquareZeroZero = null;
        }

        public bool IsSolved()
        {
            for (var x = 0; x < BoardSize; x++) {
                for (var y = 0; y < BoardSize; y++) {
                    if (_pieces[x, y] == null)
                        return false;
                }
            }

            return true;
        }
    }
}
