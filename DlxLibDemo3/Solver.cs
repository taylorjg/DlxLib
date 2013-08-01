using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

        public Solver(IEnumerable<Piece> pieces, int boardSize)
        {
            _pieces = pieces.ToArray();
            _dictionary = new Dictionary<int, Tuple<RotatedPiece, int, int>>();
            _board = new Board(boardSize);
            _board.ForceColourOfSquareZeroZeroToBeWhite();
            SearchSteps = new ConcurrentQueue<IEnumerable<Tuple<RotatedPiece, int, int>>>();
        }

        private static void InvokeOnUiThread(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        public void Solve()
        {
            var thread = new System.Threading.Thread(SolveOnBackgroundThread);
            thread.Start();
        }

        private void SolveOnBackgroundThread()
        {
            BuildMatrixAndDictionary();

            var dlx = new Dlx();

            var firstSolutionFound = false;

            dlx.Started += (sender, e) => InvokeOnUiThread(() => RaiseStarted(e));
            dlx.Finished += (sender, e) => InvokeOnUiThread(() => RaiseFinished(e));
            dlx.SearchStep += (_, e) =>
                {
                    if (!firstSolutionFound)
                    {
                        var pieceDetails = e.RowIndexes.Select(rowIndex => _dictionary[rowIndex]).ToList();
                        SearchSteps.Enqueue(pieceDetails);
                    }
                    InvokeOnUiThread(() => RaiseSearchStep(e));
                    Logger.Log("Leaving dlx.SearchStep event handler");
                };
            dlx.SolutionFound += (_, e) => InvokeOnUiThread(() =>
                {
                    firstSolutionFound = true;
                    RaiseSolutionFound(e);
                });

            dlx.Solve(_matrix);
        }

        public EventHandler Started;
        public EventHandler Finished;
        public EventHandler<SearchStepEventArgs> SearchStep;
        public EventHandler<SolutionFoundEventArgs> SolutionFound;
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

        private void RaiseStarted(EventArgs e)
        {
            var started = Started;

            if (started != null)
            {
                started(this, e);
            }
        }

        private void RaiseFinished(EventArgs e)
        {
            var finished = Finished;

            if (finished != null)
            {
                finished(this, e);
            }
        }

        private void RaiseSolutionFound(SolutionFoundEventArgs e)
        {
            var solutionFound = SolutionFound;

            if (solutionFound != null)
            {
                solutionFound(this, e);
            }
        }

        private void RaiseSearchStep(SearchStepEventArgs e)
        {
            var searchStep = SearchStep;

            if (searchStep != null)
            {
                searchStep(this, e);
            }
        }
    }
}
