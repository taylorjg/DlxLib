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
        private readonly Solver _solver = new Solver(Pieces.ThePieces, 8);

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

        public MainWindow()
        {
            InitializeComponent();

            ContentRendered += (_, __) => BoardControl.DrawGrid();

            DataContext = this;

            Closing += (_, __) => _solver.Cancel();

            var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(100)};
            timer.Tick += (_, __) =>
                {
                    IEnumerable<Tuple<RotatedPiece, int, int>> pieceDetails;
                    if (_solver.SearchSteps.TryDequeue(out pieceDetails))
                    {
                        var pieceDetailsList = pieceDetails.ToList();
                        ProcessSearchStep(pieceDetailsList);
                        if (pieceDetailsList.Count == Pieces.ThePieces.Count())
                        {
                            timer.Stop();
                            _solver.Cancel();
                        }
                    }
                };
            timer.Start();

            _solver.Solve();
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
