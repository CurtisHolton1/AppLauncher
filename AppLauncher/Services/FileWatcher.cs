using Curt.shared;
using Curt.shared.Models;
using System;
using System.IO;

namespace AppLauncher.Services
{
    public class FileWatcher
    {
        private FileSystemWatcher watcher = new FileSystemWatcher();

        public FileWatcher(string dirPath)
        {          
            watcher.IncludeSubdirectories = true;
            watcher.Path = dirPath;
            watcher.EnableRaisingEvents = true;
            watcher.Filter = "*";
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Deleted += new FileSystemEventHandler(OnDeleted);
        }
          private void OnCreated(object source, FileSystemEventArgs e)
        {
            if (!e.FullPath.Contains("Recycle.Bin") && !e.FullPath.Contains("sqlite-journal")&& !e.FullPath.Contains("Windows\\Temp"))
            {
                FileInfo f = new FileInfo(e.FullPath);                
                //var mainWindow = WindowWatcher.GetWindowOfType<MainWindow>();  
                FileItem item = new FileItem(f);
                Console.WriteLine("CREATED: " + item.ID + " " + item.FileName);
                if(item.ExtensionID >=0) 
                DatabaseManager.InsertIntoFilesTable(item);
            }             
        }
        private void OnDeleted(object source, FileSystemEventArgs e){
            if (!e.FullPath.Contains("Recycle.Bin"))
            {               
                FileInfo f = new FileInfo(e.FullPath);
                
                var toDelete = DatabaseManager.FindFileFromFileInfo(f);
                Console.WriteLine("DELETED: " + toDelete.ID + " " + toDelete.FileName);
                if(!string.IsNullOrEmpty(toDelete.FileName) && !string.IsNullOrEmpty(toDelete.FileLocation))
                DatabaseManager.DeleteFromFilesTable(toDelete.ID);
                //var mainWindow = WindowWatcher.GetWindowOfType<MainWindow>();
            } 
    }
}
}
