using System.ComponentModel;

namespace Curt.shared.Models
{
    public class Extension : INotifyPropertyChanged
    {
        public string Type { get; set; }
        private bool isChecked;
        public bool IsChecked { get { return isChecked; } set { isChecked = value; this.OnPropertyChanged("IsChecked"); } }
        public int ID {get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
