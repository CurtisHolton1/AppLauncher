using System;
using System.Windows.Media;

namespace Curt.shared.Models
{
    public class DropDownItem : IComparable<DropDownItem>
    {
        public int ID { get; set; }
        public ImageSource ImgSrc { get; set; }
        public string Content { get; set; }
        public string Option { get; set; }
        public string Path { get; set; }
        public DateTime LastUsed { get; set; }
        public int TotalTimesUsed { get; set; }
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
