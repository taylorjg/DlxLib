using System.Windows;
using DlxLibDemo3.Model;

// ReSharper disable LocalizableElement

namespace DlxLibDemo3
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var solver = new Solver(Model.Pieces.ThePieces, 8);
            var iterations = 0;
            var solutions = 0;

            solver.SolutionFound += (sender, e) =>
                {
                    solutions++;
                };

            solver.SearchStep += (sender, e) =>
                {
                    iterations++;
                };

            solver.Started += (sender, e) => BoardControl.DrawPiece(Pieces.ThePieces[4], 7, 7);

            solver.Finished += (sender, e) => MessageBox.Show(string.Format("solutions = {0}; iterations = {1}", solutions, iterations));

            solver.Solve();
        }
    }
}
