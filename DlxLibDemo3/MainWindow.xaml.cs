using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using DlxLibDemo3.Annotations;
using DlxLibDemo3.Model;

namespace DlxLibDemo3
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private int _iterations;
        private int _interval;
        private readonly Solver _solver = new Solver(Pieces.ThePieces, 8);
        private readonly DispatcherTimer _timer = new DispatcherTimer();

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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Iterations = 0;
            Interval = 10;

            ContentRendered += (_, __) => BoardControl.DrawGrid();
            Closing += (_, __) => _solver.Cancel();

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

                if (BoardControl.IsPieceOnBoard(pieceName))
                {
                    if (!BoardControl.IsPieceOnBoardWithOrientationandLocation(pieceName, orientation, x, y))
                    {
                        BoardControl.RemovePiece(pieceName);
                        BoardControl.AddPiece(rotatedPiece, x, y);
                    }
                }
                else
                {
                    BoardControl.AddPiece(rotatedPiece, x, y);
                }
            }

            BoardControl.RemovePiecesOtherThan(pieceDetails.Select(pd => pd.Item1.Piece.Name).ToList());
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
