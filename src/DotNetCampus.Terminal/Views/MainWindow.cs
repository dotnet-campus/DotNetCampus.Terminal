using Terminal.Gui.Input;

namespace DotNetCampus.Terminal.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            button1.Accepting += Button1OnAccepting;
        }

        private void Button1OnAccepting(object? sender, CommandEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
