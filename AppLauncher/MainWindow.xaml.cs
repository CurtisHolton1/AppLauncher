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


namespace AppLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        delegate void updateCallback();
        List<DropDownItem> dropDownList = new List<DropDownItem>();
        string mode = "app";
        [ProtoMember(1)]
        List<Executable> software = new List<Executable>();
       
        public MainWindow()
        {
            InitializeComponent();
            HotKey _hotKey = new HotKey(Key.Z, KeyModifier.Shift | KeyModifier.Win, OnHotKeyHandler);
            TextBar1.Focus();
            //Startup.RemoveStartup();
            if ( Startup.SetStartup())
                WriteFile(Startup.GetInitialLocations());
            FileDeserialization();
            
        }


        public async void WriteFile(List<DirSearchItem> locations)
        {
            software = await Startup.GetInstalledSoftware(locations);
            using (var file = File.OpenWrite("InstalledSoftware.bin"))
            {
                file.Position = file.Length;
                Serializer.Serialize<List<Executable>>(file, software);      
            }          
        }

        private async void FileDeserialization()
        {
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
                catch (Exception) { }
            }
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
                        searchList.Add(new DropDownItem { Content = e.Name.Substring(0, e.Name.Length - 4), Path = e.Location, ImgSrc = e.ImgSrc, LastUsed = e.LastUsed, Option = e.LastUsed.ToShortDateString() });
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
                    text = text.Substring(0, text.Length-1);
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
                            TextBar1.Text = setString;
                            //sets cursor
                            TextBar1.Select(TextBar1.Text.Length, 0);
                        }
                    }
                    catch { }
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
            if (TextBar1.Text != string.Empty)
            {
                Uri imageUri = new Uri(@"Content\goog.ico", UriKind.Relative);
                BitmapImage imageBitmap = new BitmapImage(imageUri);
                searchList.Add(new DropDownItem { Content = text, Path = "https://www.google.com/#q=", ImgSrc = imageBitmap });
                imageUri = new Uri(@"Content\stack.png", UriKind.Relative);
                imageBitmap = new BitmapImage(imageUri);
                searchList.Add(new DropDownItem { Content = text, Path = "http://stackoverflow.com/search?q=", Option = "Stack Overflow", ImgSrc = imageBitmap });
                imageUri = new Uri(@"Content\youtube.png", UriKind.Relative);
                imageBitmap = new BitmapImage(imageUri);
                searchList.Add(new DropDownItem { Content = text, Path = "https://www.youtube.com/results?search_query=", ImgSrc = imageBitmap });
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

            dropDownList = await AppSearch(text);
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
                text = await Calculator(text);
                if (mode == "calc")
                {
                    Uri imageUri = new Uri(@"Content\calc.ico", UriKind.Relative);
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
            //this.Close();
            //this.Visibility = Visibility.Hidden;
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
                DropDownItem item = new DropDownItem();
                item = (DropDownItem)ListView1.SelectedItem;
                Process.Start(item.Path);
                TextBar1.Clear();
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
            this.Close();
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
        #endregion

       

        

      
    }
}
