using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncher.Models
{
    class WatcherWrapper
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
           
            var path = e.FullPath;
            Console.WriteLine(path);
             

            
        }
        private void OnDeleted(object source, FileSystemEventArgs e){

        }
    }
}
