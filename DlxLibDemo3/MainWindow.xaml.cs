using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using DlxLibDemo3.Model;

namespace DlxLibDemo3
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Logger.Log("MainWindow");
            var solver = new Solver(Pieces.ThePieces, 8);
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            timer.Tick += (sender, e) =>
                {
                    Logger.Log("timer.Tick()");
                    IEnumerable<Tuple<RotatedPiece, int, int>> pieceDetails;
                    if (solver.SearchSteps.TryDequeue(out pieceDetails))
                    {
                        ProcessSearchStep(pieceDetails.ToList());
                    }
                };
            timer.Start();
            solver.Solve();
        }

        private void ProcessSearchStep(IList<Tuple<RotatedPiece, int, int>> pieceDetails)
        {
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
    }
}
