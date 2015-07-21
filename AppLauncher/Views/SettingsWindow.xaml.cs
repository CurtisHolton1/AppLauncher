using AppLauncher.Services;
using Curt.shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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

namespace AppLauncher
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private bool checkBoxChecked;
        public bool CheckBoxChecked { get { return checkBoxChecked; } set { checkBoxChecked = value; this.OnPropertyChanged("CheckBoxChecked"); } }
        private List<Extension> extensionList;
        public List<Extension> ExtensionList
        {
            get { return extensionList; }
            set { extensionList = value; this.OnPropertyChanged("Extension"); }
        }
        public SettingsWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            WindowWatcher.AddWindow(this);
            if(Convert.ToBoolean(ConfigurationManager.AppSettings["AutoUpdatesEnabled"]))
                CheckBoxChecked = true;
            else
                checkBoxChecked = false;
            ExtensionList = new List<Extension>();
     
           
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowWatcher.RemoveWindow(this);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Configurer config = new Configurer();
            config.UpdateConfig(new KeyValuePair<string, string>("AutoUpdatesEnabled", "true"));
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Configurer config = new Configurer();
            config.UpdateConfig(new KeyValuePair<string, string>("AutoUpdatesEnabled", "false"));
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await CheckVersion();
        }
        private async Task<string> CheckVersion()
        {
            try
            {
                var available = await Task.Run(() => VersionCheck.CompareCurrent());
                if (available)
                {
                   VersionCheck.AskForUpdate();
                }
            }
            catch (Exception e)
            {

            }
            return "";
        }






        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
