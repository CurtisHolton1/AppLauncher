using NCalc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AppLauncher.Services;
using Curt.shared;
using System.Configuration;
using Curt.shared.Models;
using AppLauncher.Models;
using System.Drawing;
using System.IO;

namespace AppLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        delegate void updateCallback();
        List<DropDownItem> dropDownList = new List<DropDownItem>();
        string mode = "app";
        List<FileItem> software = new List<FileItem>();
        List<FileItem> allFiles = new List<FileItem>();
        DispatcherTimer timer = new DispatcherTimer();
        bool timerFlag;
        bool updateFlag;
        int tickCount;
        //ILookup<string, string> fileTable;
       // Lookup<string, FileItem> allFiles;
        Dictionary<string, BitmapSource> filesIcons;

        public MainWindow()
        {
            InitializeComponent();
            WindowWatcher.AddWindow(this);
            //not ready yet////////////////////
            ExecutableWatcher fileSystemWatcher = new ExecutableWatcher("c:\\");
            ///////////////////////////////////
            //fileTable = FileSearch.GetFilesFromDB(AppDomain.CurrentDomain.BaseDirectory + "FilesData.sqlite");
             //allFiles = FileSearch.GetAllFromDB(AppDomain.CurrentDomain.BaseDirectory + "FilesData.sqlite");
            Start();
            updateFlag = true;
            timerFlag = false;
            timer.Interval = TimeSpan.FromMinutes(5);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        //public void AddToSoftware(Executable executable)
        //{
        //    if (!software.Contains(executable))
        //    {
        //        var index = software.FindIndex(f => f.Name.Equals(char.ToUpper(executable.Name[0]) + executable.Name.Substring(1)));
        //        if (index < 0)
        //        {
        //            software.Add(executable);
        //            FileWriteRead fileObject = new FileWriteRead();
        //            fileObject.ReWriteFile(software);
        //        }
        //    }
        //}

        //public void RemoveFromSoftware(Executable executable)
        //{
        //    FileWriteRead fileObject = new FileWriteRead();
        //    if (software.Contains(executable))
        //    {
        //        software.Remove(executable);
        //        fileObject.ReWriteFile(software);
        //    }
        //    else
        //    {
        //        var index = software.FindIndex(x => x.Name.Equals(executable.Name) && x.Location.Equals(executable.Location));
        //        if (index >= 0)
        //        {
        //            software.Remove(software.ElementAt(index));
        //            fileObject.ReWriteFile(software);
        //        }
        //    }
        //}
        public void AddToFilesList(FileItem f)
        {

        }


        private async Task<string> Start()
        {
            //SharedHelper.KillProcess("CurtInstaller");
            SharedHelper.DeleteDirectory(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\tmp");
            software = await Task.Run(()=> FileSearch.GetExecutables(AppDomain.CurrentDomain.BaseDirectory + "FilesData.sqlite"));
            allFiles = await Task.Run(()=>FileSearch.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "FilesData.sqlite"));
            
            TextBar1.Focus();
            //Startup.RemoveStartup();
            Key key;
            Enum.TryParse(ConfigurationManager.AppSettings["Key"], out key);
            KeyModifier keyMod;
            Enum.TryParse(ConfigurationManager.AppSettings["KeyMod"], out keyMod);
            HotKey _hotKey = new HotKey(key, keyMod, OnHotKeyHandler);
            Startup.SetStartup();
            //FileWriteRead fileObject = new FileWriteRead();
            //software = await Task.Run(() => fileObject.FileDeserialization());
           
            filesIcons = new Dictionary<string, BitmapSource>();
            return "";
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["AutoUpdatesEnabled"]))
            {
                tickCount++;
                if (updateFlag)
                    timerFlag = true;
                else if (tickCount >= 288)//24 hours
                {
                    tickCount = 0;
                    updateFlag = true;
                }
            }
        }
        private async Task<string> CheckVersion()
        {
            try
            {
                var available = await Task.Run(() => VersionCheck.CompareCurrent());
                if (available)
                {
                    updateFlag = VersionCheck.AskForUpdate();
                }
            }
            catch (Exception e)
            {

            }
            return "";
        }
        private void StartSelectedApp()
        {
            DropDownItem item = new DropDownItem();
            item = (DropDownItem)ListView1.SelectedItem;
            try
            {
                Process.Start(item.Path);
                TextBar1.Clear();
            }
            catch (Exception ex)
            {
                var folderPath = item.Path.Substring(0, item.Path.LastIndexOf("\\"));
                Process.Start("explorer.exe", folderPath);
            }
        }
        private void StartSelectedFile()
        {
            DropDownItem item = new DropDownItem();
            item = (DropDownItem)ListView1.SelectedItem;
            try
            {
                Process.Start(item.Path);
            }
            catch (Exception ex)
            {
                var folderPath = item.Path.Substring(0, item.Path.LastIndexOf("\\"));
                Process.Start("explorer.exe", folderPath);
            }
            TextBar1.Clear();
        }
        private void StartSelectedSearch()
        {
            DropDownItem item = new DropDownItem();
            item = (DropDownItem)ListView1.SelectedItem;
            Process.Start(item.Path + item.Content);
            TextBar1.Clear();
        }

        #region OperatingModes
        private async Task<List<DropDownItem>> AppSearch(string text)
        {
            //List<DropDownItem> searchList = new List<DropDownItem>();
            //if (!string.IsNullOrEmpty(text))
            //{
            //    foreach (Executable e in software)
            //    {
            //        if (e.Name.ToLower().StartsWith(text.ToLower()) || (e.Name.ToLower().Contains(text.ToLower()) && text.Length >= 3))
            //        {
            //            string set = e.Location;
            //            if (e.Location.Length >= 40)
            //            {
            //                set = e.Location.Substring(0, 40) + "...";
            //            }
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ContentColumn.Width = 200, DispatcherPriority.Normal);
                Dispatcher.Invoke(() => OptionColumn.Width = 350, DispatcherPriority.Normal);

            }
            else
            {
                ContentColumn.Width = 200;
                OptionColumn.Width = 350;
            }

            //            searchList.Add(new DropDownItem { Content = e.Name.Substring(0, e.Name.Length - 4), Path = e.Location, ImgSrc = e.ImgSrc, LastUsed = e.LastUsed, Option = set });
            //        }
            //    }
            //}
            //searchList.Sort();
            //IEnumerable<DropDownItem> firstSix = searchList.Take(6);
            //searchList = firstSix.ToList();
            //searchList.Sort();
            //return searchList;


            return Search(text, 0);
        }
        

        private async Task<string> Calculator(string text)
        {

            NCalc.Expression exp;
            string setString = text;
            bool flag = false;
            Regex reg = new Regex(@"^\d|\)|\(");
            if (text.Length != 0 && reg.IsMatch(text))
            {
                mode = "calc";
                if (text.Substring(text.Length - 1, 1).Equals("="))
                {
                    text = text.Substring(0, text.Length - 1);
                    flag = true;
                }
                exp = new NCalc.Expression(text, EvaluateOptions.IgnoreCase);
                if (!exp.HasErrors())
                {
                    try
                    {
                        setString = exp.Evaluate().ToString();
                        if (flag)
                        {
                            if (!Dispatcher.CheckAccess())
                            {
                                Dispatcher.Invoke(() => TextBar1.Text = setString, DispatcherPriority.Normal);
                                Dispatcher.Invoke(() => TextBar1.Select(TextBar1.Text.Length, 0), DispatcherPriority.Normal);

                            }
                            else
                            {
                                TextBar1.Text = setString;
                                //sets cursor
                                TextBar1.Select(TextBar1.Text.Length, 0);
                            }

                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                    setString = "Please use a valid expression";
            }
            return setString;
        }

        private async Task<List<DropDownItem>> WebSearch(string text)
        {
            mode = "search";
            List<DropDownItem> searchList = new List<DropDownItem>();
            if (text != string.Empty)
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() => ContentColumn.Width = 300, DispatcherPriority.Normal);
                    Dispatcher.Invoke(() => OptionColumn.Width = 250, DispatcherPriority.Normal);
                }
                else
                {
                    ContentColumn.Width = 300;
                    OptionColumn.Width = 250;
                }
                Uri imageUri = new Uri(@"..\Content\goog.ico", UriKind.Relative);
                BitmapImage imageBitmap = new BitmapImage(imageUri);
                searchList.Add(new DropDownItem { Content = text, Path = "https://www.google.com/#q=", ImgSrc = imageBitmap });
                //imageBitmap.Freeze();
                imageUri = new Uri(@"..\Content\stack.png", UriKind.Relative);
                imageBitmap = new BitmapImage(imageUri);
                searchList.Add(new DropDownItem { Content = text, Path = "http://stackoverflow.com/search?q=", Option = "Stack Overflow", ImgSrc = imageBitmap });
                imageUri = new Uri(@"..\Content\youtube.png", UriKind.Relative);
                //imageBitmap.Freeze();
                imageBitmap = new BitmapImage(imageUri);
                searchList.Add(new DropDownItem { Content = text, Path = "https://www.youtube.com/results?search_query=", ImgSrc = imageBitmap });
                //imageBitmap.Freeze();
            }

            return searchList;
        }

        private async Task<List<DropDownItem>> FileSearcher(string text)
        {
            mode = "file"; 
            List<DropDownItem> searchList = new List<DropDownItem>();
            
                //var filesFound = fileTable.Where(x => x.Key.ToLower().Contains(text.ToLower()) && x.Key.ToLower().StartsWith(text.ToLower())).Take(6);
               	//object aLock = new object();
                    //foreach(var f in filesFound)
                    //{
                    //    foreach (var f2 in f.ToList())
                    //    {
                    //       // if(!filesIcons.ContainsKey(f2.Split('.').Last())){
                    //            Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(f2);
                    //            var img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    //            img.Freeze();
                    //            //filesIcons.Add(f2.Split('.').Last(),img);
                    //        //}
                           
                    //        searchList.Add(new DropDownItem { Path = f2, Content = f.Key, Option = f2, ImgSrc = filesIcons[f2.Split('.').Last()]  });
                    //    }                      
                      //}

            searchList = Search(text, 1);
                     if (!Dispatcher.CheckAccess())
                        {
                            Dispatcher.Invoke(() => ContentColumn.Width = 200, DispatcherPriority.Normal);
                            Dispatcher.Invoke(() =>OptionColumn.Width = 350, DispatcherPriority.Normal);
                        }
                        else
                        {
                            ContentColumn.Width = 200; 
                            OptionColumn.Width = 350;
                        }                         
            return searchList;
        }

        private List<DropDownItem> Search(string text, int type) 
        {
            List<DropDownItem> items = new List<DropDownItem>();
            if (type == 0)
            {
                var files = software.AsParallel().Where(x => x.FileName.ToLower().Contains(text.ToLower())).Take(10).ToList();
               for (int i = 0; i < files.Count; i++ )
               {
                   var f = files[i];
                   if (File.Exists(f.FileLocation))
                   {
                       Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(f.FileLocation);
                       var img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                       img.Freeze();
                       items.Add(new DropDownItem { Content = f.FileName, Path = f.FileLocation, Option = f.FileLocation, ImgSrc = img });
                   }
                   else
                   {
                       software.Remove(software.FirstOrDefault(x => x.FileLocation.Equals(f.FileLocation)));
                       i--;
                   }
               }
            }
            else if (type == 1)
            {
                var files = allFiles.AsParallel().Where(x => x.FileName.ToLower().Contains(text.ToLower())).Take(10).ToList();
                for (int i = 0; i < files.Count; i++)
                {
                    var f = files[i];
                    if (File.Exists(f.FileLocation))
                    {
                        if (!filesIcons.ContainsKey(f.Extension))
                        {
                            Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(f.FileLocation);
                            var img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                            img.Freeze();
                            filesIcons.Add(f.Extension, img);
                        }
                        items.Add(new DropDownItem { Content = f.FileName, Path = f.FileLocation, Option = f.FileLocation, ImgSrc = filesIcons[f.Extension] });
                    }
                    else
                    {
                        allFiles.Remove(allFiles.FirstOrDefault(x => x.FileLocation.Equals(f.FileLocation)));
                        i--;
                    }
                }
                //var files = allFiles.Where(x => x.Key.ToLower().Contains(text.ToLower()) && x.Key.ToLower().StartsWith(text.ToLower()) && (x.Where(y=> y.Type==type)).Count() >0);

                //foreach (var f in files)
                //{
                //    foreach (var f2 in f)
                //    {                  
                //            //if (!filesIcons.ContainsKey(f2.FileLocation.Split('.').Last()))
                //            //{
                //            try
                //            {
                //                if (File.Exists(f2.FileLocation))
                //                {
                //                    //Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(f2.FileLocation);
                //                    //var img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                //                   // img.Freeze();
                //                    //filesIcons.Add(f2.FileLocation.Split('.').Last(), img);
                //                    //}                           
                //                    f2.TotalUsed++;
                //                    items.Add(new DropDownItem { Content = f2.FileName, Path = f2.FileLocation,  /*ImgSrc = img,*/ Option = f2.FileLocation, TotalTimesUsed = f2.TotalUsed});
                //                    var d = f2.TotalUsed;
                //                }
                //            }
                //            catch (Exception e)
                //            {

                //                Console.WriteLine(e.Message);
                //            }        
                //}
                //}
            }
                return new List<DropDownItem>(items.Take(6));
            }
            
        
        private void DropDownAdd(DropDownItem item)
        {
            this.ListView1.Items.Add(item);
            ListView1.SelectedItem = ListView1.Items[0];
            ListView1.Visibility = Visibility.Visible;
        }

        #endregion

        #region UI
        private async void TextBar1_TextChanged(object sender, TextChangedEventArgs e)
        {


            string text = TextBar1.Text;

            if (string.IsNullOrEmpty(TextBar1.Text))
            {
                ListView1.Items.Clear();
                ListView1.Visibility = Visibility.Hidden;
                this.Height = 95;
            }
            text = TextBar1.Text;
            dropDownList = await Task.Run(() => AppSearch(text));
            mode = "app";
            ListView1.Items.Clear();
            if (dropDownList != null && dropDownList.Count > 0)
            {
                for (int i = 0; i < dropDownList.Count; i++)
                {
                    DropDownAdd(dropDownList[i]);
                }
            }
            else
            {
                text = TextBar1.Text;
                TextBar1.Text = await Task.Run(() => Calculator(text));
                if (mode == "calc")
                {
                    ContentColumn.Width = 300;
                    OptionColumn.Width = 250;

                    Uri imageUri = new Uri(@"..\Content\calc.ico", UriKind.Relative);
                    BitmapImage imageBitmap = new BitmapImage(imageUri);
                    DropDownItem item = new DropDownItem { Content = TextBar1.Text, Option = "Copy with enter", ImgSrc = imageBitmap };
                    ListView1.Items.Clear();
                    if (item.Content.Equals("Please use a valid expression"))
                        item.Option = "";
                    DropDownAdd(item);
                }
                else
                {
                    text = TextBar1.Text;
                    dropDownList = await WebSearch(text);
                    ////////////
                    try
                    {
                        if (text.StartsWith("find ") && text.Length > 5)
                        {
                            dropDownList = await Task.Run(() => FileSearcher(text.Substring(5, text.Length - 5)));
                        }
                    }
                    catch (Exception excep)
                    {
                        System.Windows.MessageBox.Show(excep.Message);
                    }
                    //////////////
                    ListView1.Items.Clear();
                    if (dropDownList != null)
                    {
                        for (int i = 0; i < dropDownList.Count; i++)
                        {
                            DropDownAdd(dropDownList[i]); 
                        }
                    }
                }

            
            }
            if (string.IsNullOrEmpty(TextBar1.Text))
            {
                ListView1.Items.Clear();
                ListView1.Visibility = Visibility.Hidden;
                this.Height = 95;
            }
            this.Height = 95 + (ListView1.Items.Count * 40);

        }

        private void TextBar1_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Return && !ListView1.Items.IsEmpty)
            {
                ListView1_PreviewKeyDown(sender, e);
            }
            else if (e.Key == Key.Down && !ListView1.Items.IsEmpty)
            {
                ListView1.UpdateLayout();
                ListViewItem item = ListView1.ItemContainerGenerator.ContainerFromIndex(ListView1.SelectedIndex) as ListViewItem;
                item.IsSelected = true;
                Keyboard.Focus(item);

            }

        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void ListView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mode.Equals("search"))
            {
                StartSelectedSearch();
            }
            else if (mode.Equals("app"))
            {
                StartSelectedApp();
            }
            else if (mode.Equals("file"))
            {
                StartSelectedFile();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Window settingsWin = new SettingsWindow();
            settingsWin.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void OnHotKeyHandler(HotKey hotKey)
        {
            if (this.Visibility == Visibility.Hidden)
            {
                this.Visibility = Visibility.Visible;
                Application.Current.MainWindow.Focus();
                this.Focus();
                ListView1.Focus();
                TextBar1.Focus();
                Keyboard.Focus(TextBar1);
                this.Activate();
            }
            else
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowWatcher.RemoveWindow(this);
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            if (timerFlag && !VersionCheck.windowOpen)
            {
                timerFlag = false;
                await CheckVersion();
            }
        }

        private void ListView1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !ListView1.Items.IsEmpty && mode == "search")
            {
                StartSelectedSearch();
            }
            else if (e.Key == Key.Return && !ListView1.Items.IsEmpty && mode == "calc")
            {
                try
                {
                    DropDownItem item = new DropDownItem();
                    item = (DropDownItem)ListView1.SelectedItem;
                    if (!item.Option.Equals(""))
                        Clipboard.SetDataObject(item.Content);
                    TextBar1.Clear();
                }
                catch (Exception ex)
                {

                }
            }
            else if (e.Key == Key.Return && !ListView1.Items.IsEmpty && mode == "app")
            {
                StartSelectedApp();

            }
            else if (e.Key == Key.Return && !ListView1.Items.IsEmpty && mode == "file")
            {
                StartSelectedFile();
            }
            else if (e.Key == Key.Up && ListView1.SelectedItem == ListView1.Items[0])
            {
                TextBar1.Focus();
            }
            else if (e.Key != Key.Up && e.Key != Key.Down)
            {
                TextBar1.Focus();
            }

        }
        #endregion


    }
}

