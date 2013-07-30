using System.Windows;

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
            //solver.Started += (sender, e) => MessageBox.Show("Started!");
            solver.Finished += (sender, e) => MessageBox.Show(string.Format("solutions = {0}; iterations = {1}", solutions, iterations));

            solver.Solve();
        }
    }
}
