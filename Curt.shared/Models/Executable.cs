using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.IO;

namespace Curt.shared.Models
{

   public class Executable
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public ImageSource ImgSrc { get; set; }
        public DateTime LastUsed { get; set; }
        public Executable()
        {

        }
        public Executable(FileInfo f)
        {
            this.Location = f.FullName;
            this.Name = char.ToUpper(f.Name[0]) + f.Name.Substring(1);
            this.LastUsed = f.LastAccessTime;
            
        }
    }   
    
}
