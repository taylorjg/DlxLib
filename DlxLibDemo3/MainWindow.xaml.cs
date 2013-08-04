using DlxLibDemo3.ViewModel;

namespace DlxLibDemo3
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // Obviously, we don't want to pass BoardControl to MainWindowViewModel.
            // Need to figure out a proper MVVM solution to this.
            // We could add a BoardControlViewModel and pass that in instead ?
            // Perhaps BoardControlViewModel could expose a PieceDetails property which we could set.
            // Then, in the setter for the PieceDetails property in BoardControlViewModel, we could
            // perform the code that is currently in MainWindowViewModel.ProcessSearchStep ?
            DataContext = new MainWindowViewModel(BoardControl);

            ContentRendered += (_, __) => BoardControl.DrawGrid();

            Closing += (_, __) => ((MainWindowViewModel) DataContext).Closing();
        }
    }
}
