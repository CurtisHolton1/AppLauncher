using NCalc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Drawing;
using AppLauncher.Models;
using System.IO;
using ProtoBuf;
using AppLauncher.Services;
using Curt.shared;
using System.Timers;
using System.Configuration;
using Curt.Helpers;
using System.Threading;

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
        [ProtoMember(1)]
        List<Executable> software = new List<Executable>();
        DispatcherTimer timer = new DispatcherTimer();
        bool timerFlag;
        bool updateFlag;
        int tickCount;
        public MainWindow()
        {
            //check in comment
            InitializeComponent();
            WindowWatcher.AddWindow(this);
            HotKey _hotKey = new HotKey(Key.Z, KeyModifier.Shift | KeyModifier.Win, OnHotKeyHandler);
            Start();
            updateFlag = true ;
            timerFlag = false;
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private async Task<string> Start()
        {
            SharedHelper.KillProcess("CurtInstaller");
            SharedHelper.DeleteDirectory(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\tmp");
            TextBar1.Focus();
            //Startup.RemoveStartup();
            Startup.SetStartup();
            FileWriteRead fileObject = new FileWriteRead();
            software =  fileObject.FileDeserialization();
           //Dispatcher.Invoke(())
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


        #region OperatingModes
        private async Task<List<DropDownItem>> AppSearch(string text)
        {
            List<DropDownItem> searchList = new List<DropDownItem>();
            if (!string.IsNullOrEmpty(text))
            {
                foreach (Executable e in software)
                {
                    if (e.Name.ToLower().StartsWith(text.ToLower()) || (e.Name.ToLower().Contains(text.ToLower()) && text.Length >= 3))
                    {
                        string set = e.Location;
                        if (e.Location.Length >= 40)
                        {
                            set = e.Location.Substring(0, 40) + "...";
                        }
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

                        searchList.Add(new DropDownItem { Content = e.Name.Substring(0, e.Name.Length - 4), Path = e.Location, ImgSrc = e.ImgSrc, LastUsed = e.LastUsed, Option = set });
                    }
                }
            }
            searchList.Sort();
            IEnumerable<DropDownItem> firstSix = searchList.Take(6);
            searchList = firstSix.ToList();
            searchList.Sort();
            return searchList;
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
        #endregion

        #region UI
        private async void TextBar1_TextChanged(object sender, TextChangedEventArgs e)
        {

            String text = TextBar1.Text;
            if (string.IsNullOrEmpty(text))
            {
                ListView1.Items.Clear();
                ListView1.Visibility = Visibility.Hidden;
                this.Height = 95;
            }

            dropDownList = await Task.Run(()=> AppSearch(text));
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
                text = await Task.Run(()=> Calculator(text));
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
                    dropDownList = await WebSearch(text);
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
            if (string.IsNullOrEmpty(text))
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
                ListView1_KeyDown(sender, e);
            }
            else if (e.Key == Key.Down && !ListView1.Items.IsEmpty)
            {
                ListView1.UpdateLayout();
                ListView1.Focus();
                ListView1.SelectedItem = ListView1.Items[0];
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

        private void DropDownAdd(DropDownItem item)
        {
            this.ListView1.Items.Add(item);
            ListView1.SelectedItem = ListView1.Items[0];
            ListView1.Visibility = Visibility.Visible;
        }

        private void ListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !ListView1.Items.IsEmpty && mode == "search")
            {
                DropDownItem item = new DropDownItem();
                item = (DropDownItem)ListView1.SelectedItem;
                Process.Start(item.Path + item.Content);
                TextBar1.Clear();
            }
            else if (e.Key == Key.Return && !ListView1.Items.IsEmpty && mode == "calc")
            {
                DropDownItem item = new DropDownItem();
                item = (DropDownItem)ListView1.SelectedItem;
                if (!item.Option.Equals(""))
                    Clipboard.SetText(item.Content);
                TextBar1.Clear();
            }
            else if (e.Key == Key.Return && !ListView1.Items.IsEmpty && mode == "app")
            {
                try
                {
                    DropDownItem item = new DropDownItem();
                    item = (DropDownItem)ListView1.SelectedItem;
                    Process.Start(item.Path);
                    TextBar1.Clear();
                }
                catch (Exception ex)
                {

                }
            }
            else if (e.Key == Key.Up && ListView1.SelectedItem == ListView1.Items[0])
            {
                TextBar1.Focus();
            }
        }

        private void TextBar1_LostFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("lost focus");
        }

        private void TextBar1_GotFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("got focus");
        }

        private void ListView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DropDownItem item = new DropDownItem();
            item = (DropDownItem)ListView1.SelectedItem;
            Process.Start(item.Path);
            TextBar1.Clear();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Window settingsWin = new SettingsWindow();
            settingsWin.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void OnHotKeyHandler(HotKey hotKey)
        {
            this.Visibility = Visibility.Visible;
            Application.Current.MainWindow.Focus();
            this.Focus();
            ListView1.Focus();
            TextBar1.Focus();
            Keyboard.Focus(TextBar1);
            this.Activate();
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
        #endregion

       


    }
}

