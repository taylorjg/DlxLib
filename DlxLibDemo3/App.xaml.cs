namespace DlxLibDemo3
{
    public partial class App
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            System.Threading.Thread.CurrentThread.Name = "UI";
            base.OnStartup(e);
        }
    }
}
