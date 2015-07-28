using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curt.shared.Models
{
    public enum FileType {app = 0, file =1, folder =2}
    public class FileItem
    {       
        private FileType privEnum;
        public int ID { get; set; }
        public string FileName { get; set; }
        public string FileLocation { get; set; }
        public FileType Type { get { return privEnum; } set { privEnum = value; } }       
        public int ExtensionID { get; set; }
        public string DisplayName { get; set; }
        public DateTime LastUsed { get; set; }
        public int TotalUsed { get; set; }

        public FileItem() { }

        public FileItem(FileInfo f)
        {
            {
                FileName = f.Name;
                FileLocation = f.FullName;
                if (f.Extension.Equals(".exe"))
                    Type = FileType.app;
                else if (f.Extension.Equals(""))
                {
                    Type = FileType.folder;
                }
                else
                    Type = FileType.file;
                ExtensionID = ExtensionConverter.ConvertFromString(f.Extension);
                LastUsed = DateTime.MinValue;
                TotalUsed = 0;
            }

        }
    }
}
