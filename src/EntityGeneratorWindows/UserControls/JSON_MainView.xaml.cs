using EntityGeneratorWindows.Common;
using EntityGeneratorWindows.Model;
using EntityGeneratorWindows.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Diagnostics;
using EntityGeneratorWindows.Generator;
using System.IO;
using System.Threading;

namespace EntityGeneratorWindows.UserControls
{
    /// <summary>
    /// JSON_MainView.xaml.xaml 的交互逻辑
    /// </summary>
    public partial class JSON_MainView : UserControl
    {
        private static string json = string.Empty;

        private static System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1 = new();

        public JSON_MainView() 
        {
            InitializeComponent();

            this.combobox1.SelectedIndex = 0;

            this.standardType.SelectedIndex = 0;
            this.frameworkType.SelectedIndex = 0;

            this.txtPath.Text = "";
            this.txtProjName.Text = "";
            this.txtModelName.Text = "";

            this.tbModel.IsChecked = true;
            this.tbProj.IsChecked = false;
            this.tbSingle.IsChecked = false;
            this.tbUpper.IsChecked = false;

            //WinCapHelper.Get().ListenBegin();

            TheEvent.JSONInfoChangedEvent += TheEvent_InfoChangedEvent;
        }

        /// <summary>
        /// 格式化json
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTran_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.txtOrigin.Text))
            {
                string origin = this.txtOrigin.Text;
                try
                {
                    string res = "";
                    Preloader.Show(Global.instance, o =>
                    {
                        res = JsonHelper.Format(origin);
                    });
                    this.txtFormat.Text = res;
                }
                catch (Exception)
                {
                    TheEvent.ShowMessageBox("json有错误", "抱歉，出現了一個錯誤");
                }
            }
        }

        /// <summary>
        /// web請求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txturl.Text))
            {
                TheEvent.ShowMessageBox("請輸入url", "提示");
                return;
            }
            try
            {
                string res = "";
                var item = this.combobox1.SelectedItem as ComboBoxItem;
                if (item.Content.ToString() == "POST")
                {
                    var url = this.txturl.Text;
                    var req = this.txtReq.Text;
                    var header = this.txtHeader.Text;
                    Preloader.Show(Global.instance, o =>
                    {
                        res = WebHelper.HttpPost(url, req, header);
                    });
                }
                else
                {
                    var url = this.txturl.Text;
                    Preloader.Show(Global.instance, o =>
                    {
                        res = WebHelper.HttpGet(url);
                    });
                }
                this.txtOrigin.Text = res;
            }
            catch (Exception ex)
            {
                TheEvent.ShowMessageBox("錯誤輸出到左側TextBox，請查看", "糟糕，出現了一個錯誤");
                this.txtOrigin.Text = ex.ToString();
            }
        }

        #region 生成

        private void EnbaleControl(bool flag)
        {
            this.standardType.IsEnabled = flag;
            this.frameworkType.IsEnabled = flag;
            this.txtPath.IsEnabled = flag;
            this.txtProjName.IsEnabled = flag;
            this.txtModelName.IsEnabled = flag;
            this.tbUpper.IsEnabled = flag;
            this.tbSingle.IsEnabled = flag;
            this.tbModel.IsEnabled = flag;
            this.tbProj.IsEnabled = flag;
        }

        /// <summary>
        /// 重置生成信息
        /// </summary>
        public void Reset()
        {
            this.standardType.SelectedIndex = 0;
            this.frameworkType.SelectedIndex = 0;
            this.txtPath.Text = "";
            this.txtProjName.Text = "";
            this.txtModelName.Text = "";
            this.tbUpper.IsChecked = false;
            this.tbSingle.IsChecked = false;
            this.tbModel.IsChecked = true;
            this.tbProj.IsChecked = false;
            this.txtOutput.Text = "";

            Global.pversion = "";

        }

        /// <summary>
        /// 設置初始值
        /// </summary>
        /// <param name="json1"></param>
        public void SetValue(string json1)
        {
            var item = this.standardType.SelectedItem as ComboBoxItem;
            var item1 = this.frameworkType.SelectedItem as ComboBoxItem;
            if (item.Tag.ToString().Contains("standard"))
            {
                Global.pversion = item?.Tag.ToString() + ";" + item1.Tag.ToString();
            }
            else
            {
                Global.pversion = item?.Tag.ToString();
            }
            json = json1;
        }

        /// <summary>
        /// 抽屜開啓事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawerHost_DrawerOpened(object sender, MaterialDesignThemes.Wpf.DrawerOpenedEventArgs e)
        {
            EnbaleControl(true);
            if (string.IsNullOrWhiteSpace(this.txtOrigin.Text) && string.IsNullOrWhiteSpace(this.txtFormat.Text))
            {
                this.DrawerHost.IsRightDrawerOpen = false;
                TheEvent.ShowMessageBox("沒有JSON可供使用", "提示");
                return;
            }
            else
            {
                Reset();
                this.DrawerHost.IsRightDrawerOpen = true;
            }
        }

        /// <summary>
        /// 生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txtPath.Text))
            {
                TheEvent.ShowMessageBox("請填寫或選擇生成路徑", "提示");
                return;
            }

            if (!Directory.Exists(this.txtPath.Text))
            {
                TheEvent.ShowMessageBox("请填寫合法路徑", "提示");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.txtProjName.Text))
            {
                TheEvent.ShowMessageBox("请填寫項目名稱", "提示");
                return;
            }

            if (!string.IsNullOrWhiteSpace(this.txtOrigin.Text))
            {
                try
                {
                    var data = this.txtOrigin.Text;
                    Preloader.Show(Global.instance, o =>
                    {
                        JsonHelper.Format(data);
                    });
                }
                catch (Exception)
                {
                    TheEvent.ShowMessageBox("json有错误", "抱歉，出現了一個錯誤");
                    return;
                }
                SetValue(this.txtOrigin.Text);
            }
            else 
            {
                try
                {
                    var data = this.txtFormat.Text;
                    Preloader.Show(Global.instance, o =>
                    {
                        JsonHelper.Format(data);
                    });
                }
                catch (Exception)
                {
                    TheEvent.ShowMessageBox("json有错误", "抱歉，出現了一個錯誤");
                    return;
                }
                SetValue(this.txtFormat.Text);
            }

            EnbaleControl(false);

            this.txtOutput.Text = "";
            var path = this.txtPath.Text;
            var projname = this.txtModelName.Text;
            var slname = this.txtProjName.Text;
            var single_file = this.tbSingle.IsChecked.Value;
            var csproj = this.tbProj.IsChecked.Value;
            var upper = this.tbUpper.IsChecked.Value;
            Preloader.Show(Global.instance, o =>
            {
                Entrance.JsonGenerate(path, json, projname, slname, single_file, csproj, upper);
            });

            EnbaleControl(true);

            TheEvent.ShowMessageBox("生成成功", "提示");
        }

        /// <summary>
        /// 生成信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void TheEvent_InfoChangedEvent(object sender, InfoEventArgs args)
            => this.Dispatcher.BeginInvoke((Action)delegate () { 
                this.txtOutput.Text += args.info + "\r\n"; 
                this.txtOutput.ScrollToEnd(); 
            });

        /// <summary>
        /// 選擇生成類型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void standardType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.standardType.SelectedItem is not null)
            {
                var item = this.standardType.SelectedItem as ComboBoxItem;
                this.frameworkType.Visibility = item.Tag.ToString().Contains("standard") ? Visibility.Visible : Visibility.Hidden;
            }
        }


        /// <summary>
        /// 打開文件夾
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFolder_Click(object sender, RoutedEventArgs e)
        {
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
            folderBrowserDialog1.ShowNewFolderButton = true;
            if (System.Windows.Forms.DialogResult.OK == folderBrowserDialog1.ShowDialog())
            {
                this.txtPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        /// <summary>
        /// 項目名稱改變
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtProjName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.txtProjName.Text))
            {
                this.txtModelName.Text = this.txtProjName.Text + "Model";
            }
            else
            {
                this.txtModelName.Text = "";
            }
        }

        /// <summary>
        /// 生成項目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbProj_Click(object sender, RoutedEventArgs e)
            => this.tbModel.IsChecked = !this.tbProj.IsChecked;

        /// <summary>
        /// 只生成實體
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbModel_Click(object sender, RoutedEventArgs e)
            => this.tbProj.IsChecked = !this.tbModel.IsChecked;

        /// <summary>
        /// 打開項目生成位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", this.txtPath.Text + "\\" + this.txtModelName.Text);
            }
            catch { }
        }

        #endregion

    }
}
