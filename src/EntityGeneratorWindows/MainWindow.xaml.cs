using EntityGeneratorWindows.Common;
using EntityGeneratorWindows.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EntityGeneratorWindows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        WindowState ws;
        WindowState wsl;

        NotifyIcon notifyIcon = null;

        public MainWindow()
        {
            InitializeComponent();

            Global.instance = this;

            Task.Factory.StartNew(() => Thread.Sleep(2500)).ContinueWith(t =>
            {
                this.MainSnackbar.MessageQueue?.Enqueue("歡迎使用實體生成工具");
            }, TaskScheduler.FromCurrentSynchronizationContext());

            var json = FileHelper.ReadConfig();
            var config = JsonHelper.ToObject<Configs>(json);
            this.NotifyIconToggleButton.IsChecked = config is null ? false : config.isCloseButtonExit;

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.BalloonTipText = "运行中...";//托盘气泡显示内容
            notifyIcon.Text = "实体生成工具";
            notifyIcon.Visible = true;//托盘按钮是否可见
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.ShowBalloonTip(2000);//托盘气泡显示时间
            notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;

            var show = new ToolStripMenuItem("显示");
            show.Click += m1_Click;
            var quit = new ToolStripMenuItem("退出");
            quit.Click += m2_Click;
            var contextMenuStrip1 = new ContextMenuStrip();
            contextMenuStrip1.Items.Add(show);
            contextMenuStrip1.Items.Add(quit);
            this.notifyIcon.ContextMenuStrip = contextMenuStrip1;

            //保证窗体显示在上方。
            wsl = WindowState;

            this.SizeChanged += MainWindow_SizeChanged;
            this.Closing += MainWindow_Closing;

            TheEvent.ShowMessageEvent += TheEvent_ShowMessageEvent;
            TheEvent.ShowProgressEvent += TheEvent_ShowProgressEvent;
        }

        #region 事件

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!this.NotifyIconToggleButton.IsChecked.Value)
            {
                if (ws == WindowState.Minimized)
                {
                    this.Hide();
                    this.notifyIcon.Visible = true;
                }
            }
        }
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.NotifyIconToggleButton.IsChecked.Value)
            {
                Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
                this.Hide();
                ws = WindowState.Minimized;
                this.notifyIcon.Visible = true;
                this.notifyIcon.ShowBalloonTip(10, "注意", "工具已隐藏到托盘", ToolTipIcon.Info);
            }
        }

        void m2_Click(object sender, EventArgs e) => Environment.Exit(0);

        void m1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
        }

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
        }

        private void MenuOpen_OnClick(object sender, EventArgs e)
        {
            //this.Visibility = Visibility.Visible;
        }

        private void MenuExit_OnClick(object sender, EventArgs e)
        {
            
        }

        private void TabItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => this.labelMain.Content = "數據庫模式";

        private void TabItem_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e) => this.labelMain.Content = "Json模式";

        private void TabItem_MouseLeftButtonUp_3(object sender, MouseButtonEventArgs e) => this.labelMain.Content = "从数据库字段生成表单模式";

        private void TabItem_MouseLeftButtonUp_2(object sender, MouseButtonEventArgs e) => this.labelMain.Content = "查看信息";

        private void MenuDarkModeButton_Click(object sender, RoutedEventArgs e)
            => ModifyTheme(DarkModeToggleButton.IsChecked == true);

        private static void ModifyTheme(bool isDarkTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);
        }

        private async void TheEvent_ShowMessageEvent(object sender, MessageEventArgs e) => await this.ShowMessageAsync(e.title, e.msg, e.style);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TheEvent_ShowProgressEvent(object sender, MessageEventArgs e) => await this.ShowProgressAsync(e.title, e.msg);

        private async void Help(object sender, EventArgs e) => await this.ShowMessageAsync("提示", "不给帮");

        private void Close(object sender, EventArgs e)
        {
            //Environment.Exit(0);
        }

        #endregion

        private void NotifyIconToggleButton_Click_1(object sender, RoutedEventArgs e)
        {
            var json = JsonHelper.ToJson(new Configs() { isCloseButtonExit = this.NotifyIconToggleButton.IsChecked.Value });
            FileHelper.WriteConfig(json);
        }
    }
}
