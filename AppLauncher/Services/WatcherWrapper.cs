using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curt.shared;
using Curt.shared.Models;
namespace AppLauncher.Models
{
   public class WatcherWrapper
    {
       private FileSystemWatcher watcher = new FileSystemWatcher();
      
       public WatcherWrapper(string dirPath)
        {          
            watcher.IncludeSubdirectories = true;
            watcher.Path = dirPath;
            watcher.EnableRaisingEvents = true;
            watcher.Filter = "*.exe";
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Deleted += new FileSystemEventHandler(OnDeleted);
        }
          private void OnCreated(object source, FileSystemEventArgs e)
        {
            if (!e.FullPath.Contains("Recycle.Bin"))
            {
                FileInfo f = new FileInfo(e.FullPath);
                Executable exe = new Executable(f);
                var mainWindow = WindowWatcher.GetWindowOfType<MainWindow>();
                (mainWindow as MainWindow).AddToSoftware(exe);
               
            } 
            
        }
        private void OnDeleted(object source, FileSystemEventArgs e){
            if (!e.FullPath.Contains("Recycle.Bin"))
            {               
                FileInfo f = new FileInfo(e.FullPath);
                Executable exe = new Executable(f);
                var mainWindow = WindowWatcher.GetWindowOfType<MainWindow>();
                (mainWindow as MainWindow).RemoveFromSoftware(exe);

            } 
        }
    }
}
