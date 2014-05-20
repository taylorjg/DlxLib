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
                SearchStep searchStep;
                if (_solver.SearchSteps.TryDequeue(out searchStep))
                {
                    var piecePlacements = searchStep.PiecePlacements.ToList();
                    ProcessSearchStep(piecePlacements);
                    if (piecePlacements.Count == Pieces.ThePieces.Count())
                    {
                        _timer.Stop();
                    }
                }
            };

            _solver.Solve();
            _timer.Start();
        }

        private void ProcessSearchStep(IList<PiecePlacement> piecePlacements)
        {
            Iterations++;

            foreach (var piecePlacement in piecePlacements)
            {
                var rotatedPiece = piecePlacement.RotatedPiece;
                var pieceName = rotatedPiece.Piece.Name;
                var orientation = rotatedPiece.Orientation;
                var x = piecePlacement.Coords.X;
                var y = piecePlacement.Coords.Y;

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

            _boardControl.RemovePiecesOtherThan(piecePlacements.Select(pd => pd.RotatedPiece.Piece.Name).ToList());
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
            set
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
