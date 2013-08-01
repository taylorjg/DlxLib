using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DlxLibDemo3.Model;

namespace DlxLibDemo3
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var solver = new Solver(Pieces.ThePieces, 8);
            Closing += (_, __) =>
                {
                    Logger.Log("inside event handler for MainWindow.Closing");
                    solver.Cancel();
                };
            //var iterations = 0;
            //var solutions = 0;
            //solver.SearchStep += (sender, args) => iterations++;
            //solver.SolutionFound += (sender, args) => solutions++;
            //solver.Finished += (sender, args) => MessageBox.Show(string.Format("Finished! (iterations: {0}; solutions: {1})", iterations, solutions));
            //solver.Cancelled += (sender, args) => MessageBox.Show(string.Format("Cancelled! (iterations: {0}; solutions: {1})", iterations, solutions));
            var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(1)};
            timer.Tick += (_, __) =>
                {
                    Logger.Log("timer.Tick()");
                    IEnumerable<Tuple<RotatedPiece, int, int>> pieceDetails;
                    if (solver.SearchSteps.TryDequeue(out pieceDetails))
                    {
                        var pieceDetailsList = pieceDetails.ToList();
                        ProcessSearchStep(pieceDetailsList);
                        if (pieceDetailsList.Count == Pieces.ThePieces.Count())
                        {
                            timer.Stop();
                            solver.Cancel();
                        }
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
