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

            FileWatcher watcher = new FileWatcher("C:\\");

            Start();
            updateFlag = true;
            timerFlag = false;
            timer.Interval = TimeSpan.FromMinutes(30);
            timer.Tick += timer_Tick;
            timer.Start();
        }
      

        private async Task<string> Start()
        {
            //SharedHelper.KillProcess("CurtInstaller");
            SharedHelper.DeleteDirectory(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\tmp");
            //software = await Task.Run(()=> DatabaseManager.GetExecutables(AppDomain.CurrentDomain.BaseDirectory + "FilesData.sqlite"));
           // allFiles = await Task.Run(()=>DatabaseManager.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "FilesData.sqlite"));           
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
        private async void timer_Tick(object sender, EventArgs e)
        {
          //await Task.Run(()=>  DatabaseManager.ReWriteDatabase(AppDomain.CurrentDomain.BaseDirectory, "FilesData.sqlite", software, allFiles));
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["AutoUpdatesEnabled"]))
            {
                tickCount++;
                if (updateFlag)
                    timerFlag = true;
                else if (tickCount >= 48)//24 hours
                {
                    tickCount = 0;
                    updateFlag = true;
                }
            }
        }
        //if the timer update flag has been set, will be called on window open
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
        private async void StartSelectedApp()
        {
            DropDownItem item = new DropDownItem();
            item = (DropDownItem)ListView1.SelectedItem;
            
            try
            {
                if (SharedHelper.BringProcessToFront(item.Content.Substring(0, item.Content.LastIndexOf('.')), item.Path))
                {
                    this.Hide();
                }
                else
                {
                    Process.Start(item.Path);
                }
                TextBar1.Clear();
            }
            catch (Exception ex)
            {
                var folderPath = item.Path.Substring(0, item.Path.LastIndexOf("\\"));
                Process.Start("explorer.exe", folderPath);
            }
            item.LastUsed = DateTime.Now;
            item.TotalTimesUsed++;
            await Task.Run(() =>DatabaseManager.UpdateDB(item));
            TextBar1.Clear();
        }    
        private async void StartSelectedFile()
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
            item.LastUsed = DateTime.Now; 
            item.TotalTimesUsed++;
            await Task.Run(() => DatabaseManager.UpdateDB(item));
            TextBar1.Clear();
        }
        private void StartSelectedSearch()
        {
            DropDownItem item = new DropDownItem();
            item = (DropDownItem)ListView1.SelectedItem;
            Process.Start(item.Path + item.Content);
            TextBar1.Clear();
        }

        private async Task<List<DropDownItem>> Search(string text, int type)
        {
            List<DropDownItem> items = new List<DropDownItem>();

            //if (type == 0)
            //{
            //    var files = software.AsParallel().OrderByDescending(x => x.TotalUsed).Where(x => x.FileName.ToLower().Contains(text.ToLower()) && x.FileName.ToLower().StartsWith(text.ToLower())).Take(10).ToList();
            //    for (int i = 0; i < files.Count; i++)
            //    {
            //        var f = files[i];
            //        if (File.Exists(f.FileLocation))
            //        {
            //            Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(f.FileLocation);
            //            var img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            //            img.Freeze();
            //            items.Add(new DropDownItem { Content = f.FileName, Path = f.FileLocation, Option = f.FileLocation, ImgSrc = img });
            //        }
            //        else
            //        {
            //            software.Remove(software.FirstOrDefault(x => x.FileLocation.Equals(f.FileLocation)));
            //            i--;
            //        }
            //    }
            //}
            //else if (type == 1)
            //{
            //    var files = allFiles.AsParallel().OrderByDescending(x => x.TotalUsed).Where(x => x.FileName.ToLower().Contains(text.ToLower()) && x.FileName.ToLower().StartsWith(text.ToLower())).Take(10).ToList();
            //    for (int i = 0; i < files.Count; i++)
            //    {
            //        var f = files[i];
            //        if (File.Exists(f.FileLocation))
            //        {
            //            if (!filesIcons.ContainsKey(f.Extension))
            //            {
            //                Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(f.FileLocation);
            //                var img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            //                img.Freeze();
            //                filesIcons.Add(f.Extension, img);
            //            }
            //            items.Add(new DropDownItem { Content = f.FileName, Path = f.FileLocation, Option = f.FileLocation, ImgSrc = filesIcons[f.Extension], TotalTimesUsed = f.TotalUsed, LastUsed = f.LastUsed });
            //        }
            //        else
            //        {
            //            allFiles.Remove(allFiles.FirstOrDefault(x => x.FileLocation.Equals(f.FileLocation)));
            //            i--;
            //        }
            //    }
            //}

            if (string.IsNullOrEmpty(text))
            {
                return new List<DropDownItem>();
            }
            List<FileItem> all = new List<FileItem>();
            if (type == 0)
            {
                 all = await Task.Run(() => DatabaseManager.SelectFromDB(text, type));
            }
            if (type == 1)
            {
                all = await Task.Run(() => DatabaseManager.SelectFromDBGreaterEqualType(text, type));
            }
           all = all.OrderByDescending(x => x.TotalUsed).Take(15).ToList();
           foreach (var f in all)
           {
               
               System.Windows.Media.Imaging.BitmapSource img;

               if ((f.Type == FileType.file) && !filesIcons.ContainsKey(f.Extension))
               {
                   Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(f.FileLocation);
                   var tmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                   tmp.Freeze();
                   img = tmp;
                   filesIcons.Add(f.Extension, img);
                   
               }
               else if ((FileType)type == FileType.app)
               {
                   Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(f.FileLocation);
                   var tmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                   tmp.Freeze();
                   img = tmp;
               }
               else if (f.Type == FileType.folder && !filesIcons.ContainsKey(f.Extension))
               {
                   Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(Environment.GetEnvironmentVariable("windir")  + "\\explorer.exe");
                   var tmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                   tmp.Freeze();
                   img = tmp;
                   filesIcons.Add(f.Extension, img);
               }
               else
                   img = filesIcons[f.Extension];
        


               var content = char.ToUpper(f.FileName[0]) + f.FileName.Substring(1);
               if (!string.IsNullOrEmpty(f.DisplayName))
                   content = char.ToUpper(f.DisplayName[0]) + f.DisplayName.Substring(1);
               items.Add(new DropDownItem {ID = f.ID, Content = content , Path = f.FileLocation, Option = f.FileLocation, ImgSrc = img , TotalTimesUsed = f.TotalUsed, LastUsed = f.LastUsed });
           }
           return items;
        }


        #region OperatingModes
        private async Task<List<DropDownItem>> AppSearch(string text)
        {             
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
            return await Search(text, 0);
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
            searchList = await Search(text, (int)FileType.file);
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
                text = await Task.Run(() => Calculator(text));
                if (mode == "calc")
                {
                    ContentColumn.Width = 300;
                    OptionColumn.Width = 250;

                    Uri imageUri = new Uri(@"..\Content\calc.ico", UriKind.Relative);
                    BitmapImage imageBitmap = new BitmapImage(imageUri);
                    DropDownItem item = new DropDownItem { Content = text, Option = "Copy with enter", ImgSrc = imageBitmap };
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

