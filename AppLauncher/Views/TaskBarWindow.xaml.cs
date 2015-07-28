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

namespace AppLauncher.Views
{
    /// <summary>
    /// Interaction logic for TaskBarWindow.xaml
    /// </summary>
    public partial class TaskBarWindow : Window
    {
        public TaskBarWindow()
        {
            InitializeComponent();
        }


        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Click");
            var m = (MainWindow)WindowWatcher.GetWindowOfType<MainWindow>();
            if (m.Visibility == Visibility.Hidden)
            {
                m.Visibility = Visibility.Visible;
                Application.Current.MainWindow.Focus();
                m.Focus();
                m.ListView1.Focus();
                m.TextBar1.Focus();
                Keyboard.Focus(m.TextBar1);
                m.Activate();
            }
            else
            {
                this.Visibility = Visibility.Hidden;
            }
        }
    }
}
