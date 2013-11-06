using System;
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
        private readonly Piece[] _pieces;
        private readonly Board _board;

        // Maps from matrix row number (zero-based) to a tuple containing RotatedPiece + (x,y) board location.
        private readonly IDictionary<int, Tuple<RotatedPiece, int, int>> _dictionary;

        private bool[,] _matrix;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Dlx _dlx;

        public Solver(IEnumerable<Piece> pieces, int boardSize)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _dlx = new Dlx(_cancellationTokenSource.Token);
            _pieces = pieces.ToArray();
            _dictionary = new Dictionary<int, Tuple<RotatedPiece, int, int>>();
            _board = new Board(boardSize);
            _board.ForceColourOfSquareZeroZeroToBeWhite();
            SearchSteps = new ConcurrentQueue<IEnumerable<Tuple<RotatedPiece, int, int>>>();
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

            _dlx.SearchStep += (_, e) =>
                {
                    var pieceDetails = e.RowIndexes.Select(rowIndex => _dictionary[rowIndex]).ToList();
                    SearchSteps.Enqueue(pieceDetails);
                };

            _dlx.SolutionFound += (_, __) => _cancellationTokenSource.Cancel();

            _dlx.Solve(_matrix);
        }

        private Thread _thread;
        public ConcurrentQueue<IEnumerable<Tuple<RotatedPiece, int, int>>> SearchSteps { get; private set; }

        private void BuildMatrixAndDictionary()
        {
            IList<IList<bool>> data = new List<IList<bool>>();

            for (var pieceIndex = 0; pieceIndex < _pieces.Length; pieceIndex++)
            {
                var piece = _pieces[pieceIndex];
                AddDataItemsForPieceWithSpecificOrientation(data, pieceIndex, piece, Orientation.North);
                var isFirstPiece = (pieceIndex == 0);
                if (!isFirstPiece)
                {
                    AddDataItemsForPieceWithSpecificOrientation(data, pieceIndex, piece, Orientation.South);
                    AddDataItemsForPieceWithSpecificOrientation(data, pieceIndex, piece, Orientation.East);
                    AddDataItemsForPieceWithSpecificOrientation(data, pieceIndex, piece, Orientation.West);
                }
            }

            var numColumns = _pieces.Length + _board.BoardSize * _board.BoardSize;
            _matrix = new bool[data.Count, numColumns];
            for (var row = 0; row < data.Count; row++)
            {
                for (var col = 0; col < numColumns; col++)
                {
                    _matrix[row, col] = data[row][col];
                }
            }
        }

        private void AddDataItemsForPieceWithSpecificOrientation(ICollection<IList<bool>> data, int pieceIndex, Piece piece, Orientation orientation)
        {
            var rotatedPiece = new RotatedPiece(piece, orientation);

            for (var x = 0; x < _board.BoardSize; x++)
            {
                for (var y = 0; y < _board.BoardSize; y++)
                {
                    _board.Reset();
                    _board.ForceColourOfSquareZeroZeroToBeWhite();
                    if (!_board.PlacePieceAt(rotatedPiece, x, y)) continue;
                    var dataItem = BuildDataItem(pieceIndex, rotatedPiece, x, y);
                    data.Add(dataItem);
                    _dictionary.Add(data.Count - 1, Tuple.Create(rotatedPiece, x, y));
                }
            }
        }

        private IList<bool> BuildDataItem(int pieceIndex, RotatedPiece rotatedPiece, int x, int y)
        {
            var numColumns = _pieces.Length + _board.BoardSize * _board.BoardSize;
            var dataItem = new bool[numColumns];

            dataItem[pieceIndex] = true;

            var w = rotatedPiece.Width;
            var h = rotatedPiece.Height;

            for (var pieceX = 0; pieceX < w; pieceX++)
            {
                for (var pieceY = 0; pieceY < h; pieceY++)
                {
                    if (rotatedPiece.SquareAt(pieceX, pieceY) == null) continue;
                    var boardX = x + pieceX;
                    var boardY = y + pieceY;
                    var boardLocationColumnIndex = _pieces.Length + (_board.BoardSize * boardX) + boardY;
                    dataItem[boardLocationColumnIndex] = true;
                }
            }

            return dataItem;
        }
    }
}
