using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppLauncher
{
    public class DropDownItem : IComparable<DropDownItem>
    {
        public ImageSource ImgSrc { get; set; }
        public string Content { get; set; }
        public string Option { get; set; }
        public string Path { get; set; }
        public DateTime LastUsed { get; set; }
        public int CompareTo(DropDownItem other)
        {
            if (LastUsed.CompareTo(other.LastUsed) < 0)
            {
                return 1;
            }
            else if (LastUsed.CompareTo(other.LastUsed) > 0)
            {
                return -1;
            }
            else
                return 0;
        }
    }
}
