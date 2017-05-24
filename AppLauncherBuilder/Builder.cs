using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;



namespace AppLauncherBuilder
{
    class Builder
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Build");
            List<string> filePaths = new List<string>();
            List<string> dirPaths = new List<string>();
            string appLauncherBase = Directory.GetParent(Directory.GetCurrentDirectory()) + "\\AppLauncher\\bin\\release";
            string installBase = Directory.GetParent(Directory.GetCurrentDirectory()) + "\\CurtInstaller\\bin\\release";
            string contentBase = Directory.GetParent(Directory.GetCurrentDirectory()) + "\\AppLauncher";

            filePaths.Add("AppLauncher.exe");
            filePaths.Add("AppLauncher.exe.config");
            filePaths.Add("Curt.shared.dll");
            filePaths.Add("Hardcodet.Wpf.TaskbarNotification.dll");
            filePaths.Add("NCalc.dll");
            filePaths.Add("System.Data.SQLite.dll");
            dirPaths.Add("x64");
            dirPaths.Add("x86"); 

            buildComponent("AppLauncher", appLauncherBase, filePaths, dirPaths);

            filePaths.Clear();
            dirPaths.Clear();

            filePaths.Add("CurtInstaller.exe");
            filePaths.Add("Curt.shared.dll");
            filePaths.Add("CurtInstaller.exe.config");
            filePaths.Add("System.Data.SQLite.dll");
            filePaths.Add("WhiteListTmp.txt");
            dirPaths.Add("x64");
            dirPaths.Add("x86");
            dirPaths.Add("Resources");
            buildComponent("CurtInstaller", installBase, filePaths, dirPaths);

            filePaths.Clear();
            dirPaths.Clear();

            dirPaths.Add("Content");
            buildComponent("", contentBase, filePaths, dirPaths);

            ZipFile.CreateFromDirectory(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\built\\AppLauncher", Directory.GetParent(Directory.GetCurrentDirectory()) + "\\AppLauncher.zip");
             Directory.Delete(Directory.GetParent(Directory.GetCurrentDirectory()) + "\\built", true);
        }

        private static void buildComponent(string componentName, string componentPath, List<string> filePaths, List<string>dirPaths)
        {
            string destinationPath = Directory.GetParent(Directory.GetCurrentDirectory()) + "\\built\\AppLauncher";

            if (!Directory.Exists(destinationPath + "\\"  + componentName + "\\"))
            {
                Directory.CreateDirectory(destinationPath + "\\" + componentName + "\\");
            }
            foreach (string path in filePaths)
            {
                 File.Copy(componentPath + "\\" + path, destinationPath + "\\" + componentName + "\\" + path, true);
            }
            foreach (string dir in dirPaths)
            {
                string fullDest = destinationPath + "\\" + componentName + "\\" + dir;
                string fullSource = componentPath + "\\" + dir;
                if (!Directory.Exists(fullDest))
                {
                    Directory.CreateDirectory(fullDest);
                }
                foreach (string newPath in Directory.GetFiles(fullSource, "*.*", SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(fullSource, fullDest), true);
            }
        }

      
}
}