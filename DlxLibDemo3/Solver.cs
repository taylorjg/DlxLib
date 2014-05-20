using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DlxLib;
using DlxLibDemo3.Model;

namespace DlxLibDemo3
{
    internal class Solver
    {
        private class InternalRow
        {
            public InternalRow(bool[] matrixRow, PiecePlacement piecePlacement)
            {
                MatrixRow = matrixRow;
                PiecePlacement = piecePlacement;
            }

            public bool[] MatrixRow { get; private set; }
            public PiecePlacement PiecePlacement { get; private set; }
        }

        private readonly Piece[] _pieces;
        private readonly Board _board;
        private readonly IList<InternalRow> _data = new List<InternalRow>();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Dlx _dlx;
        private Thread _thread;

        public ConcurrentQueue<SearchStep> SearchSteps { get; private set; }

        public Solver(IEnumerable<Piece> pieces, int boardSize)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _dlx = new Dlx(_cancellationTokenSource.Token);
            _pieces = pieces.ToArray();
            _board = new Board(boardSize);
            _board.ForceColourOfSquareZeroZeroToBeWhite();
            SearchSteps = new ConcurrentQueue<SearchStep>();
        }

        public void Solve()
        {
            _thread = new Thread(SolveOnBackgroundThread);
            _thread.Start();
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
            _thread.Join();
        }

        private void SolveOnBackgroundThread()
        {
            Thread.CurrentThread.Name = "Dlx";

            BuildMatrixAndDictionary();

            _dlx.SearchStep += (_, e) => SearchSteps.Enqueue(new SearchStep(e.RowIndexes.Select(rowIndex => _data[rowIndex].PiecePlacement)));

            //_dlx.SolutionFound += (_, __) => _cancellationTokenSource.Cancel();

            var firstSolution = _dlx.Solve<IList<InternalRow>, InternalRow, bool>(
                    _data,
                    (d, f) => { foreach (var r in d) f(r); },
                    (r, f) => { foreach (var c in r.MatrixRow) f(c); },
                    c => c).First();
        }

        private void BuildMatrixAndDictionary()
        {
            for (var pieceIndex = 0; pieceIndex < _pieces.Length; pieceIndex++)
            {
                var piece = _pieces[pieceIndex];
                AddDataItemsForPieceWithSpecificOrientation(pieceIndex, piece, Orientation.North);
                var isFirstPiece = (pieceIndex == 0);
                if (!isFirstPiece)
                {
                    AddDataItemsForPieceWithSpecificOrientation(pieceIndex, piece, Orientation.South);
                    AddDataItemsForPieceWithSpecificOrientation(pieceIndex, piece, Orientation.East);
                    AddDataItemsForPieceWithSpecificOrientation(pieceIndex, piece, Orientation.West);
                }
            }
        }

        private void AddDataItemsForPieceWithSpecificOrientation(int pieceIndex, Piece piece, Orientation orientation)
        {
            var rotatedPiece = new RotatedPiece(piece, orientation);

            for (var x = 0; x < _board.BoardSize; x++)
            {
                for (var y = 0; y < _board.BoardSize; y++)
                {
                    _board.Reset();
                    _board.ForceColourOfSquareZeroZeroToBeWhite();
                    if (!_board.PlacePieceAt(rotatedPiece, x, y)) continue;
                    var dataItem = BuildDataItem(pieceIndex, rotatedPiece, new Coords(x, y));
                    _data.Add(dataItem);
                }
            }
        }

        private InternalRow BuildDataItem(int pieceIndex, RotatedPiece rotatedPiece, Coords coords)
        {
            var numColumns = _pieces.Length + _board.BoardSize * _board.BoardSize;
            var matrixRow = new bool[numColumns];

            matrixRow[pieceIndex] = true;

            var w = rotatedPiece.Width;
            var h = rotatedPiece.Height;

            for (var pieceX = 0; pieceX < w; pieceX++)
            {
                for (var pieceY = 0; pieceY < h; pieceY++)
                {
                    if (rotatedPiece.SquareAt(pieceX, pieceY) == null) continue;
                    var boardX = coords.X + pieceX;
                    var boardY = coords.Y + pieceY;
                    var boardLocationColumnIndex = _pieces.Length + (_board.BoardSize * boardX) + boardY;
                    matrixRow[boardLocationColumnIndex] = true;
                }
            }

            return new InternalRow(matrixRow, new PiecePlacement(rotatedPiece, coords));
        }
    }
}
