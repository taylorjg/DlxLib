using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using DlxLibDemo3.Annotations;
using DlxLibDemo3.Model;

namespace DlxLibDemo3.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly BoardControl _boardControl;
        private int _iterations;
        private int _interval;
        private readonly Solver _solver = new Solver(Pieces.ThePieces, 8);
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public MainWindowViewModel(BoardControl boardControl)
        {
            _boardControl = boardControl;
            Iterations = 0;
            Interval = 10;

            _timer.Tick += (_, __) =>
            {
                IEnumerable<Tuple<RotatedPiece, int, int>> pieceDetails;
                if (_solver.SearchSteps.TryDequeue(out pieceDetails))
                {
                    var pieceDetailsList = pieceDetails.ToList();
                    ProcessSearchStep(pieceDetailsList);
                    if (pieceDetailsList.Count == Pieces.ThePieces.Count())
                    {
                        _timer.Stop();
                    }
                }
            };

            _solver.Solve();
            _timer.Start();
        }

        private void ProcessSearchStep(IList<Tuple<RotatedPiece, int, int>> pieceDetails)
        {
            Iterations++;

            foreach (var pd in pieceDetails)
            {
                var rotatedPiece = pd.Item1;
                var pieceName = rotatedPiece.Piece.Name;
                var orientation = rotatedPiece.Orientation;
                var x = pd.Item2;
                var y = pd.Item3;

                if (_boardControl.IsPieceOnBoard(pieceName))
                {
                    if (!_boardControl.IsPieceOnBoardWithOrientationandLocation(pieceName, orientation, x, y))
                    {
                        _boardControl.RemovePiece(pieceName);
                        _boardControl.AddPiece(rotatedPiece, x, y);
                    }
                }
                else
                {
                    _boardControl.AddPiece(rotatedPiece, x, y);
                }
            }

            _boardControl.RemovePiecesOtherThan(pieceDetails.Select(pd => pd.Item1.Piece.Name).ToList());
        }

        public void Closing()
        {
            _solver.Cancel();
        }

        public int Iterations
        {
            get { return _iterations; }
            private set
            {
                if (value == _iterations) return;
                _iterations = value;
                OnPropertyChanged("Iterations");
            }
        }

        public int Interval
        {
            get { return _interval; }
            private set
            {
                if (value == _interval) return;
                _interval = value;
                OnPropertyChanged("Interval");
                _timer.Interval = TimeSpan.FromMilliseconds(value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
