using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            else if (!string.IsNullOrEmpty(startupMode) && startupMode.Equals("WriteFile"))
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
