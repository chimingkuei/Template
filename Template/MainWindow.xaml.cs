using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using static Template.BaseLogRecord;
using DataNexus;
using CamNexus;
using System.Windows.Media.Media3D;
using VisionNexus;
using DimmerNexus;
using System.Drawing;
using ControlsNexus;
using DataNexus.ComNet;
using System.Drawing.Imaging;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Serilog;


namespace Template
{
    #region Config Class
    public class SerialNumber
    {
        [JsonProperty("Parameter1_val")]
        public string Parameter1_val { get; set; }
        [JsonProperty("Parameter2_val")]
        public string Parameter2_val { get; set; }
    }

    public class Model
    {
        [JsonProperty("SerialNumbers")]
        public SerialNumber SerialNumbers { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("Models")]
        public List<Model> Models { get; set; }
    }
    #endregion

    public partial class MainWindow : System.Windows.Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Function
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("請問是否要關閉？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        #region Config
        private SerialNumber SerialNumberClass()
        {
            SerialNumber serialnumber_ = new SerialNumber
            {
                Parameter1_val = Parameter1.Text,
                Parameter2_val = Parameter2.Text
            };
            return serialnumber_;
        }

        private void LoadConfig(int model, int serialnumber, bool isEncryption = false)
        {
            List<RootObject> Parameter_info = Config.Load(isEncryption);
            if (Parameter_info != null)
            {
                Parameter1.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Parameter1_val;
                Parameter2.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Parameter2_val;
                Log.Information("導入參數。");
            }
            else
            {
                // 結構:2個Models、Models下在各2個SerialNumbers
                SerialNumber serialnumber_ = SerialNumberClass();
                List<Model> models = new List<Model>
                {
                    new Model { SerialNumbers = serialnumber_ },
                    new Model { SerialNumbers = serialnumber_ }
                };
                List<RootObject> rootObjects = new List<RootObject>
                {
                    new RootObject { Models = models },
                    new RootObject { Models = models }
                };
                Config.SaveInit(rootObjects, isEncryption);
            }
        }

        private void SaveConfig(int model, int serialnumber, bool isBackup = true, bool isEncryption = false)
        {
            Config.Save(model, serialnumber, SerialNumberClass(), isBackup, isEncryption);
            Log.Information("儲存參數。");
        }
        #endregion

        #region Dispatcher Invoke 
        public string DispatcherGetValue(TextBox control)
        {
            string content = "";
            this.Dispatcher.Invoke(() =>
            {
                content = control.Text;
            });
            return content;
        }

        public void DispatcherSetValue(string content, TextBox control)
        {
            this.Dispatcher.Invoke(() =>
            {
                control.Text = content;
            });
        }

        #region IntegerUpDown Invoke
        //public int? DispatcherIntegerUpDownGetValue(Xceed.Wpf.Toolkit.IntegerUpDown control)
        //{
        //    int? content = null;
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        if (int.TryParse(control.Text, out int result))
        //        {
        //            content = result;
        //        }
        //        else
        //        {
        //            content = null;
        //        }
        //    });
        //    return content;
        //}
        #endregion
        #endregion

        /// <summary>
        /// Log.Information("Application started at {time}", DateTime.Now);<br/>
        /// Log.Warning("Low disk space on drive C:");<br/>
        /// Log.Error("Unhandled exception: {exception}", new Exception("Test error"));<br/>
        /// Log.Debug("Debug 訊息");<br/>
        /// </summary>
        private void LoggerInit()
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.File("LogRecord/Log-.txt", rollingInterval: RollingInterval.Day)
               .WriteTo.Sink(new RichTextBoxSink(richTextBoxDebug, richTextBoxGeneral, richTextBoxWarning, richTextBoxError, LogRecord))
               .CreateLogger();
        }

        #region NotifyIcon
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        System.Windows.Forms.ContextMenu nIconMenu = new System.Windows.Forms.ContextMenu();
        System.Windows.Forms.MenuItem nIconMenuItem1 = new System.Windows.Forms.MenuItem();
        System.Windows.Forms.MenuItem nIconMenuItem2 = new System.Windows.Forms.MenuItem();
        private void NotifyIconInit()
        {
            string projectName = Assembly.GetEntryAssembly()?.GetName().Name;
            notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon(@"Icon/Deepwise.ico"),
                Text = projectName,
                Visible = true,
                ContextMenu = nIconMenu
            };
            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(NotifyIcon_MouseClick);
            this.StateChanged += new EventHandler(UI_StateChanged);
            // 設定小圖示選單
            nIconMenuItem1.Text = "結束1";
            nIconMenuItem1.Click += new System.EventHandler(nIconMenuItem1_Click);
            nIconMenu.MenuItems.Add(nIconMenuItem1);
            nIconMenuItem2.Text = "結束2";
            nIconMenuItem2.Click += new System.EventHandler(nIconMenuItem2_Click);
            nIconMenu.MenuItems.Add(nIconMenuItem2);
        }
        private void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (this.Visibility == Visibility.Visible)
                {
                    this.Visibility = Visibility.Hidden;
                }
                else
                {
                    this.Show();
                    this.WindowState = (WindowState)System.Windows.Forms.FormWindowState.Normal;
                }

            }
        }
        private void UI_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Visibility = Visibility.Hidden;
            }
        }
        private void nIconMenuItem1_Click(object sender, System.EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void nIconMenuItem2_Click(object sender, System.EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        #endregion

        #region Language Switch
        #region Control Usage
        //<ComboBox Height = "60" Margin="5" Width="200" SelectionChanged="Language_Switch">
        //    <ComboBoxItem IsSelected = "True" >
        //        < StackPanel Orientation="Horizontal" Tag="en-US">
        //            <Image Source = "/Icon/DeepWise.png" Width="32" Height="32" Margin="10"/>
        //            <TextBlock Text = "{DynamicResource US}" VerticalAlignment="Center"/>
        //        </StackPanel>
        //    </ComboBoxItem>
        //    <ComboBoxItem>
        //        <StackPanel Orientation = "Horizontal" Tag="zh-CN">
        //            <Image Source = "/Icon/DeepWise.png" Width="32" Height="32" Margin="10"/>
        //            <TextBlock Text = "{DynamicResource CN}" VerticalAlignment="Center"/>
        //        </StackPanel>
        //    </ComboBoxItem>
        //    <ComboBoxItem>
        //        <StackPanel Orientation = "Horizontal" Tag="zh-TW">
        //            <Image Source = "/Icon/DeepWise.png" Width="32" Height="32" Margin="10"/>
        //            <TextBlock Text = "{DynamicResource TW}" VerticalAlignment="Center"/>
        //        </StackPanel>
        //    </ComboBoxItem>
        //</ComboBox>
        #endregion
        #region Code Usage
        //private void Language_Switch(object sender, SelectionChangedEventArgs e)
        //{
        //    if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
        //    {
        //        if (selectedItem.Content is StackPanel stackPanel)
        //        {
        //            string langCode = stackPanel.Tag.ToString();
        //            ResourceDictionary dict = new ResourceDictionary();
        //            switch (langCode)
        //            {
        //                case "en-US":
        //                    {
        //                        dict.Source = new Uri("pack://SiteOfOrigin:,,,/Lang/en-US.xaml", UriKind.Absolute);
        //                        break;
        //                    }
        //                case "zh-TW":
        //                    {
        //                        dict.Source = new Uri("pack://SiteOfOrigin:,,,/Lang/zh-TW.xaml", UriKind.Absolute);
        //                        break;
        //                    }
        //                case "zh-CN":
        //                    {
        //                        dict.Source = new Uri("pack://SiteOfOrigin:,,,/Lang/zh-CN.xaml", UriKind.Absolute);
        //                        break;
        //                    }
        //            }
        //            System.Windows.Application.Current.Resources.MergedDictionaries[11] = dict;
        //        }
        //    }
        //}
        #endregion
        #endregion

        #region ToggleButton
        //private void ToggleButton_CheckedUnchecked(object sender, RoutedEventArgs e)
        //{
        //    var toggleButton = sender as ToggleButton;
        //    if (toggleButton.IsChecked == true)
        //    {
        //        //Do something!
        //    }
        //    else
        //    {
        //        //Do something!
        //    }
        //}
        #endregion

        private void OpenFolder(string description, System.Windows.Controls.TextBox textbox)
        {
            System.Windows.Forms.FolderBrowserDialog path = new System.Windows.Forms.FolderBrowserDialog();
            path.Description = description;
            path.ShowDialog();
            textbox.Text = path.SelectedPath;
        }

        private void WriteVersionToXml()
        {
            // 取得程式名稱（不含副檔名）
            string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownApp";
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;  // 執行檔目錄
            string assemblyInfoPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, @"..\..\..\Properties\AssemblyInfo.cs"));
            if (File.Exists(assemblyInfoPath))
            {
                // 讀取 AssemblyInfo.cs
                string content = File.ReadAllText(assemblyInfoPath);
                // 使用正則抓取 AssemblyFileVersion
                Regex regex = new Regex(@"\[assembly:\s*AssemblyFileVersion\s*\(\s*""(?<version>[\d\.]+)""\s*\)\s*\]");
                Match match = regex.Match(content);
                if (match.Success)
                {
                    string versionStr = match.Groups["version"].Value; // 例如 "1.2.3.45"
                    // 分割版本號
                    string[] parts = versionStr.Split('.');
                    string major = parts.Length > 0 ? parts[0] : "0";
                    string minor = parts.Length > 1 ? parts[1] : "0";
                    string patch = parts.Length > 2 ? parts[2] : "0";
                    string build = parts.Length > 3 ? parts[3] : "0";
                    // 建立 XML
                    XDocument doc = new XDocument(
                        new XDeclaration("1.0", "utf-8", null),
                        new XElement("VersionInfo",
                            new XElement("Application",
                                new XAttribute("name", appName),
                                new XElement("Version",
                                    new XAttribute("major", major),
                                    new XAttribute("minor", minor),
                                    new XAttribute("patch", patch),
                                    new XAttribute("build", build)
                                )
                            )
                        )
                    );
                    // 寫入 XML 檔案
                    string outputPath = "AssemblyVersion.xml";
                    doc.Save(outputPath);
                }
            }
        }
        #endregion

        #region Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoggerInit();
            WriteVersionToXml();
            LoadConfig(0, 0);
        }
        BaseConfig<RootObject> Config = new BaseConfig<RootObject>();
        Webcam Cam = new Webcam();
        #endregion

        #region Main Window
        private void Main_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(Demo):
                    {
                        //Cam.OpenCamera(0, Display_Windows);
                        DiskClean win = new DiskClean();
                        win.ShowDialog();
                        break;
                    }
            }
        }

        private void About_Click(object sender, MouseButtonEventArgs e)
        {
            string filePath = "AssemblyVersion.xml";
            if (File.Exists(filePath))
            {
                try
                {
                    XDocument doc = XDocument.Load(filePath);
                    XElement versionElement = doc.Root?.Element("Application")?.Element("Version");
                    if (versionElement != null)
                    {
                        string major = versionElement.Attribute("major")?.Value ?? "0";
                        string minor = versionElement.Attribute("minor")?.Value ?? "0";
                        string patch = versionElement.Attribute("patch")?.Value ?? "0";
                        string build = versionElement.Attribute("build")?.Value ?? "0";
                        string version = $"{major}.{minor}.{patch}.{build}";
                        MessageBox.Show($"版本號︰{version}", "版本", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("XML 中未找到版本號!", "版本", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"讀取版本號失敗: {ex.Message}", "版本", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("未找到版本號 XML!", "版本", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            e.Handled = true; // 阻止切換到這個 Tab 的內容
        }

        #region Shortcut Key
        private void CommandBinding_ShortcutKey(object sender, ExecutedRoutedEventArgs e)
        {

        }
        #endregion
        #endregion

    }
}
