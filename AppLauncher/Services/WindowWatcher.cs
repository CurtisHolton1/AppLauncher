using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AppLauncher
{
    public static class WindowWatcher
    {
       private static List<Window> allWindows = new List<Window>();


        public static void Startup()
        {
            allWindows = System.Windows.Application.Current.Windows.Cast<Window>().ToList<Window>();
        }

        public static void AddWindow(Window w)
        {
            allWindows.Add(w);
        }
        public static void RemoveWindow(Window w)
        {
            allWindows.Remove(w);
          
        }
        public static bool Contains(Window w)
        {
            return allWindows.Contains(w);
        }

        public static Window GetWindowOfType<T>() where T:Window
        {
            var result = allWindows.OfType<T>();
            return result.FirstOrDefault();
        }
        

    }
}