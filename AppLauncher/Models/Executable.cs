using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media;
using ProtoBuf;

namespace AppLauncher
{
    [ProtoContract]
   public class Executable
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Location { get; set; }
      
        public ImageSource ImgSrc { get; set; }
        [ProtoMember(3)]
        public DateTime LastUsed { get; set; }
    }   
}
