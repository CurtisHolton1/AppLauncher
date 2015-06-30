using Curt.shared;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Curt.Helpers
{
    public class FileWriteRead
    {
        public async Task<bool> WriteFile(List<DirSearchItem> locations, string locationToWrite)
        {          
            var software = await Task.Run(()=>Startup.GetInstalledSoftware(locations));
           
            using (var file = File.OpenWrite(locationToWrite + "\\InstalledSoftware.bin"))
            {
                file.Position = file.Length;
                Serializer.Serialize<List<Executable>>(file, software);
                file.Dispose();
            }
            return true;
        }

        public async void FileDeserialization()
        {
            List<Executable> software;
            using (var file = File.OpenRead("InstalledSoftware.bin"))
            {
                file.Position = 0;
                software = Serializer.Deserialize<List<Executable>>(file);
            }
            //get icons
            foreach (Executable e in software)
            {
                try
                {
                    Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(e.Location.ToString());
                    e.ImgSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    e.ImgSrc.Freeze();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
