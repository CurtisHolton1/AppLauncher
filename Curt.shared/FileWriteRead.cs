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
      
        public async Task<bool> WriteFile(List<DirSearchItem> locations, string locationToWrite,System.IProgress<double> progressIndicator)
        {
              
            Task<List<Executable>> software = Task.Run(()=>Startup.GetInstalledSoftware(locations,progressIndicator));
            var thing = await software;
            using (var file = File.OpenWrite(locationToWrite + "//AppLauncher//AppLauncher//InstalledSoftware.bin"))
            {
                file.Position = file.Length;
                Serializer.Serialize<List<Executable>>(file, thing);
                file.Dispose();
            }

            return true;
        }

        public async Task<List<Executable>> FileDeserialization()
        {
            List<Executable> software;
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "InstalledSoftware.bin"))
            {
                MessageBox.Show("Could not find InstalledSoftware.bin file. Writing new file, this could take a while...");
                SharedHelper.StartInstaller("WriteFile");
                
            }
            using (var file = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "InstalledSoftware.bin"))
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
                
                }

            }
            return software;
        }
       
    }
}
