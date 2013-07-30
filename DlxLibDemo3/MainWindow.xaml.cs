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
                    //var solution = "[" + string.Join(", ", e.Solution.RowIndexes) + "]";
                    //Console.WriteLine("SolutionFound - Solution: {0}; SolutionIndex: {1}", solution, e.SolutionIndex);
                    solutions++;
                };

            solver.SearchStep += (sender, e) =>
                {
                    //var rowIndexes = "[" + string.Join(", ", e.RowIndexes) + "]";
                    //Console.WriteLine("SearchStep - Depth: {0}; Iteration: {1}; RowIndexes: {2}", e.Depth, e.Iteration, rowIndexes);
                    iterations++;
                };

            solver.Finished += (sender, e) =>
                {
                    var _ = MessageBox.Show(string.Format("solutions = {0}; iterations = {1}", solutions, iterations));
                };

            solver.Solve();
        }
    }
}
