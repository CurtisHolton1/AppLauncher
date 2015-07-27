using System.Windows;

namespace CurtInstaller.Views
{
    /// <summary>
    /// Interaction logic for HiddenView.xaml
    /// </summary>
    public partial class HiddenView : Window
    {
        public HiddenView()
        {
           
            var startupMode = (Application.Current as App).StartupMode;
            ////////////////////////////
            //startupMode = "Update";
            ///////////////////////////
            if (!string.IsNullOrEmpty(startupMode) && startupMode.Equals("Update"))
            {
                var wnd = new InstallView(startupMode);
                wnd.Show();
            }
            else 
            {           
                var wnd = new PickLocation();
                wnd.Show();
            }

            InitializeComponent();
            this.Close();
        }
    }
}
