using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DesktopClock
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    ///

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_EX_STYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

        void FormLoaded(object sender, RoutedEventArgs args)
        {
            //Variable to hold the handle for the form
            var helper = new WindowInteropHelper(this).Handle;
            //Performing some magic to hide the form from Alt+Tab
            SetWindowLong(helper, GWL_EX_STYLE, (GetWindowLong(helper, GWL_EX_STYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);

            SetSize(this);
        }

        //保存窗口位置和大小
        private static readonly string _regPath = @"Software/MyApp/WindowBounds/";

        public static void SaveSize(Window window) => Registry.CurrentUser.CreateSubKey(_regPath + window.Name).SetValue("Bounds", window.RestoreBounds);
        
        public static void SetSize(Window window)
        {
            if (window.SizeToContent != SizeToContent.Manual)
            {
                return;
            }

            RegistryKey key = Registry.CurrentUser.OpenSubKey(_regPath + window.Name);

            if (key is null)
            {
                return;
            }
            var windowBounds = Rect.Parse($"{key.GetValue("Bounds")}");
            window.Top = windowBounds.Top;
            window.Left = windowBounds.Left;
            window.Width = windowBounds.Width;
            window.Height = windowBounds.Height;
        }
        private void Window_Closing(object _, System.ComponentModel.CancelEventArgs __) => MainWindow.SaveSize(this);

        //主窗口
        DispatcherTimer _Timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            icon();

            //this.WindowStyle = WindowStyle.ToolWindow;
            this.ShowInTaskbar = false;
            _Timer.Interval = TimeSpan.FromSeconds(1);
            _Timer.Tick += new EventHandler(Update_Time);
            _Timer.Start();
        }

        //系统托盘
        System.Windows.Forms.NotifyIcon notifyIcon = null;

        private void icon()
        {
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.Text = "DigitalClock";//最小化到托盘时，鼠标点击时显示的文本
            this.notifyIcon.Icon = new System.Drawing.Icon(@"logo_white.ico");//程序图标
            this.notifyIcon.Visible = true;

            //右键菜单--打开菜单项
            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("Ontop");
            open.Click += new EventHandler(ShowWindow);
            //右键菜单--退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("Exit");
            exit.Click += new EventHandler(CloseWindow);
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

        }
        
        private void ShowWindow(object sender, EventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Visible;
            //this.ShowInTaskbar = true;
            this.Activate();
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }        


        //时间更新
        private void Update_Time(object sender, EventArgs e)
        {
            string dayOfWeek = DateTime.Now.DayOfWeek.ToString();
            string day = DateTime.Now.Day.ToString();
            string monthOfYear = "";
            switch(DateTime.Now.Month.ToString())
            {
                case "1": monthOfYear = "January"; break;
                case "2": monthOfYear = "February"; break;
                case "3": monthOfYear = "March"; break;
                case "4": monthOfYear = "April"; break;
                case "5": monthOfYear = "May"; break;
                case "6": monthOfYear = "June"; break;
                case "7": monthOfYear = "July"; break;
                case "8": monthOfYear = "August"; break;
                case "9": monthOfYear = "September"; break;
                case "10": monthOfYear = "October"; break;
                case "11": monthOfYear = "November"; break;
                case "12": monthOfYear = "December"; break;
                default:
                    break;
            }
            string a = "  ";
            string b = string.Format("{0}{1}{2}, {3}", monthOfYear, a, day, dayOfWeek);
            this.Label_Time.Content = $"{DateTime.Now.Hour.ToString("00")}:{DateTime.Now.Minute.ToString("00")}:{DateTime.Now.Second.ToString("00")}";
            this.Text_SolarCalendar.Text = b;
        }

        //窗口鼠标拖动
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


    }

   

}
