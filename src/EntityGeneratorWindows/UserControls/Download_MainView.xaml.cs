using EntityGeneratorWindows.Common;
using EntityGeneratorWindows.Generator;
using EntityGeneratorWindows.Model;
using EntityGeneratorWindows.Sql;
using EntityGeneratorWindows.Windows;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace EntityGeneratorWindows.UserControls
{
    /// <summary>
    /// DB_MainView.xaml 的交互逻辑
    /// </summary>
    public partial class Download_MainView : UserControl
    {
        /// <summary>
        /// 當前被選擇節點
        /// </summary>
        private TreeViewItem curnode = null;

        /// <summary>
        /// 
        /// </summary>
        private TreeViewItem lastnode = null;

        /// <summary>
        /// 
        /// </summary>
        private static System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();

        public Download_MainView() 
        {
            InitializeComponent();
            TheEvent.DBInfoChangedEvent += TheEvent_InfoChangedEvent;
        }

        /// <summary>
        /// 創建表節點
        /// </summary>
        /// <param name="node"></param>
        private void CreateTableNode(TreeViewItem node)
        {
            lastnode?.Items.Clear();
            lastnode = node;
            var tableinfo = node.Tag as DataBaseInfo;
            Global.tableName = tableinfo.datname;

            IEnumerable<ClassInfo> list = null;
            Preloader.Show(Global.instance, o =>
            {
                var con = SqlType.Instance().Get(Global.sqlType);
                list = con.GetTableList(Global.tableName);
            });

            if (list is not null && list.Count() > 0)
            {
                foreach (var item in list)
                {
                    var checkbox = new CheckBox() { IsChecked = true };
                    checkbox.Click += Checkbox_Click;

                    var stack = new StackPanel() { Orientation = Orientation.Horizontal };
                    stack.Children.Add(checkbox);
                    stack.Children.Add(new TextBlock() { Margin = new Thickness(8, 0, 0, 0), Text = item.className });

                    node.Items.Add(new TreeViewItem() { Header = stack, Tag = item });
                }

                node.ExpandSubtree();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Checkbox_Click(object sender, RoutedEventArgs e)
        {
            //this.checkbox1.IsChecked = false;
        }

        /// <summary>
        /// 顯示表字段信息
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="tablenode"></param>
        private void CreateFieldInfo(string tablename, TreeViewItem tablenode)
        {
            this.datagrid1.ItemsSource = null;
            this.datagrid1.BeginInit();

            var classInfo = tablenode.Tag as ClassInfo;

            IEnumerable<FieldInfo> list = null;
            Preloader.Show(Global.instance, o =>
            {
                var con = SqlType.Instance().Get(Global.sqlType);
                list = con.GetTableFieldList(tablename, classInfo);
            });
            this.datagrid1.ItemsSource = list;
            this.datagrid1.EndInit();
        }

        /// <summary>
        /// 點擊樹節點
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton is MouseButtonState.Pressed && this.treeView1.SelectedItem is not null)
            {
                var node = this.treeView1.SelectedItem as TreeViewItem;
                if (node is not null)
                {
                    var parent = node.Parent as TreeViewItem;
                    if (parent is not null)
                    {
                        this.datagrid1.ItemsSource = null;
                        var parentsparent = parent.Parent as TreeViewItem;
                        if (parentsparent is null)//如果是表節點
                        {
                            this.curnode = node;
                            CreateTableNode(node);
                        }
                        else//如果是字段節點
                        {
                            var table = parent.Tag as DataBaseInfo;
                            CreateFieldInfo(table.datname, node);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 全選
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkbox1_Click(object sender, RoutedEventArgs e)
        {
            if (this.curnode is not null)
            {
                foreach (TreeViewItem item in this.curnode.Items)
                {
                    var panel = item.Header as StackPanel;
                    var check = panel.Children[0] as CheckBox;
                    check.IsChecked = this.checkbox1.IsChecked;
                }
            }
        }

        #region 登陸數據庫

        /// <summary>
        /// 初始化
        /// </summary>
        public void InitData()
        {
            if (DBValid())
            {
                SetValue();

                this.curnode = null;
                this.datagrid1.Items.Clear();
                this.treeView1.Items.Clear();
                this.treeView1.BeginInit();

                //添加根節點
                var stack = new StackPanel() { Orientation = Orientation.Horizontal };
                stack.Children.Add(new PackIcon() { Kind = PackIconKind.TableOfContents });
                stack.Children.Add(new TextBlock() { Margin = new Thickness(8, 0, 0, 0), Text = Global.ip + ":" + Global.port });

                var node = new TreeViewItem() { Header = stack };
                node.ExpandSubtree();

                IEnumerable<DataBaseInfo> list = null;
                Preloader.Show(Global.instance, o =>
                {
                    var con = SqlType.Instance().Get(Global.sqlType);
                    list = con.GetDBList();
                });

                if (list is not null && list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        var stack1 = new StackPanel() { Orientation = Orientation.Horizontal };
                        stack1.Children.Add(new PackIcon() { Kind = PackIconKind.Database });
                        stack1.Children.Add(new TextBlock() {Margin = new Thickness(8, 0, 0, 0), Text = item.datname });

                        node.Items.Add(new TreeViewItem() { Header = stack1, Tag = item });
                    }

                    //添加數據到樹
                    this.treeView1.Items.Add(node);
                }

                this.treeView1.EndInit();
            }
        }

        public bool DBValid()
        {
            if (this.comboSqltype.SelectedItem == null)
            {
                TheEvent.ShowMessageBox("请选择数据库类型", "提示");
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.txtip.Text))
            {
                TheEvent.ShowMessageBox("请输入IP", "提示");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 設置值
        /// </summary>
        public void SetValue()
        {
            Global.ip = this.txtip.Text;
            Global.port = this.txtport.Text;
            Global.user = this.txtuser.Text;
            Global.password = this.txtpwd.Password;
            var item = this.comboSqltype.SelectedItem as ComboBoxItem;
            Global.sqlType = item.Content.ToString();

            UpdateHistory();
        }

        /// <summary>
        /// 更新連接記錄
        /// </summary>
        private void UpdateHistory()
        {
            try
            {
                var json = FileHelper.ReadBin();
                var list = JsonHelper.ToObject<List<Sqlinfo>>(json);
                if (list is null)
                {
                    list = new List<Sqlinfo>();
                }

                var value = list.Where(s => s.ip == Global.ip && s.sqlType == Global.sqlType).FirstOrDefault();
                if (value is not null)
                {
                    list.Remove(value);
                }

                list.Add(new Sqlinfo() { ip = Global.ip, port = Global.port, sqlType = Global.sqlType, user = Global.user, password = Global.password });

                FileHelper.WriteBin(JsonHelper.ToJson(list));
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// 登錄
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_OnClick(object sender, EventArgs e)
        {
            InitData();
        }

        /// <summary>
        /// 測試數據庫能否聯通
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (DBValid())
            {
                SetValue();
                try
                {
                    var sql = SqlType.Instance().Get(Global.sqlType);
                    var con = sql.GetConnection("");
                    if (con is null)
                    {
                        TheEvent.ShowMessageBox("连接数据库失败", "提示");
                    }
                    else
                    {
                        TheEvent.ShowMessageBox("连接数据库成功", "提示");
                    }
                }
                catch (Exception ex)
                {
                    TheEvent.ShowMessageBox(ex.Message, "糟糕，發生了一個錯誤");
                }
            }
        }

        /// <summary>
        /// 加載歷史數據
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopupBox_Opened(object sender, RoutedEventArgs e)
        {
            this.listHistory.Items.Clear();
            Task.Factory.StartNew(() => {
                var json = FileHelper.ReadBin();
                var list = JsonHelper.ToObject<List<Sqlinfo>>(json);
                if (list is not null && list.Count > 0)
                {
                    this.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        foreach (var item in list)
                        {
                            var child = new ListBoxItem();
                            child.Tag = item;
                            child.Content = "數據庫：" + item.sqlType + "；IP：" + item.ip + "；端口：" + item.port + "；用戶名：" + item.user + "。";
                            this.listHistory.Items.Add(child);
                        }
                    });
                }
            });
        }

        /// <summary>
        /// 選中歷史記錄
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = this.listHistory.SelectedItem as ListBoxItem;
            if (item is not null)
            {
                this.popupHistory.IsPopupOpen = false;

                dynamic model = item.Tag;
                Global.ip = model.ip;
                Global.port = model.port;
                Global.sqlType = model.sqlType;
                Global.user = model.user;
                Global.password = model.password;

                this.txtip.Text = model.ip;
                this.txtport.Text = model.port;
                this.txtuser.Text = model.user;
                this.txtpwd.Password = model.password;
                foreach (ComboBoxItem single in this.comboSqltype.Items)
                {
                    if (single.Content.ToString() == (string)model.sqlType)
                    {
                        this.comboSqltype.SelectedItem = single;
                    }
                }
            }
        }

        /// <summary>
        /// 刪除一條歷史記錄
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelHistory_Click(object sender, RoutedEventArgs e)
        {
            this.listHistory.Items.Remove(this.listHistory.SelectedItem);
            var json = FileHelper.ReadBin();
            var list = new List<Sqlinfo>();
            foreach (ListBoxItem item in this.listHistory.Items)
            {
                list.Add(item.Tag as Sqlinfo);
            }
            FileHelper.WriteBin(JsonHelper.ToJson(list));
        }

        /// <summary>
        /// 清空歷史記錄
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.WriteBin("");
            this.listHistory.Items.Clear();
        }

        #endregion

        #region 生成

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
            this.txtDalName.Text = "";
            this.tbmodel.IsChecked = true;
            this.tbproj.IsChecked = false;
            this.tball.IsChecked = false;
            this.txtOutput.Text = "";

            Global.path = "";
            Global.projName = "";
            Global.modelName = "";
            Global.dataName = "";
            Global.btype = 0;
            Global.pversion = "";
        }

        /// <summary>
        /// 判斷是否合法
        /// </summary>
        /// <returns></returns>
        private bool GenerateValid()
        {
            if (string.IsNullOrWhiteSpace(this.txtPath.Text))
            {
                TheEvent.ShowMessageBox("请选择生成位置", "提示");
                return false;
            }

            if (!Directory.Exists(this.txtPath.Text))
            {
                TheEvent.ShowMessageBox("请填寫合法路徑", "提示");
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.txtProjName.Text))
            {
                TheEvent.ShowMessageBox("请输入项目名称，且不要以Model结尾", "提示");
                return false;
            }

            if (!(bool)this.tbmodel.IsChecked && !(bool)this.tbproj.IsChecked && !(bool)this.tball.IsChecked)
            {
                TheEvent.ShowMessageBox("请选择生成方式", "提示");
                return false;
            }

            if (this.tbproj.IsChecked.Value)
            {
                if (string.IsNullOrWhiteSpace(this.txtModelName.Text))
                {
                    TheEvent.ShowMessageBox("实体层名称不能为空", "提示");
                    return false;
                }
                if (this.txtModelName.Text == this.txtProjName.Text)
                {
                    TheEvent.ShowMessageBox("实体层名称不能和项目重名", "提示");
                    return false;
                }
            }

            if (this.tball.IsChecked.Value)
            {
                if (string.IsNullOrWhiteSpace(this.txtModelName.Text))
                {
                    TheEvent.ShowMessageBox("实体层名称不能为空", "提示");
                    return false;
                }
                if (this.txtModelName.Text == this.txtProjName.Text)
                {
                    TheEvent.ShowMessageBox("实体层名称不能和项目重名", "提示");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(this.txtDalName.Text))
                {
                    TheEvent.ShowMessageBox("数据层名称不能为空", "提示");
                    return false;
                }
                if (this.txtDalName.Text == this.txtProjName.Text)
                {
                    TheEvent.ShowMessageBox("数据层名称不能和项目重名", "提示");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 設置值
        /// </summary>
        private void GenerateSetValue()
        {
            Global.path = this.txtPath.Text;
            Global.projName = this.txtProjName.Text;
            Global.modelName = this.txtModelName.Text;
            Global.dataName = this.txtDalName.Text;
            if (this.tbmodel.IsChecked.Value)
            {
                Global.btype = 0;
            }
            if (this.tbproj.IsChecked.Value)
            {
                Global.btype = 1;
            }
            if (this.tball.IsChecked.Value)
            {
                Global.btype = 2;
            }

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
        }

        /// <summary>
        /// 
        /// </summary>
        private void EnbaleControl(bool flag)
        {
            this.standardType.IsEnabled = flag;
            this.frameworkType.IsEnabled = flag;
            this.txtPath.IsEnabled = flag;
            this.txtProjName.IsEnabled = flag;
            this.txtModelName.IsEnabled = flag;
            this.txtDalName.IsEnabled = flag;
            this.tbmodel.IsEnabled = flag;
            this.tbproj.IsEnabled = flag;
            this.tball.IsEnabled = flag;
        }

        private void DrawerHost_DrawerOpened(object sender, DrawerOpenedEventArgs e)
        {
            if (Global.sqlType is null)
            {
                this.DrawerHost.IsRightDrawerOpen = false;
                TheEvent.ShowMessageBox("还未连接数据库", "提示");
                return;
            }

            if (this.curnode.Items.Count.Equals(0))
            {
                this.DrawerHost.IsRightDrawerOpen = false;
                TheEvent.ShowMessageBox("请双击选择一个数据库", "提示");
                return;
            }

            Reset();
            EnbaleControl(true);

            if (this.curnode is not null)
            {
                Global.tables.Clear();
                foreach (TreeViewItem item in this.curnode.Items)
                {
                    var panel = item.Header as StackPanel;
                    var check = panel.Children[0] as CheckBox;
                    if (check.IsChecked.Value)
                    {
                        var info = item.Tag as ClassInfo;
                        Global.tables.Add(info);
                    }
                }
            }

            this.DrawerHost.IsRightDrawerOpen = true;
        }

        /// <summary>
        /// 打開項目所在位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", this.txtPath.Text + "\\" + Global.projName);
            }
            catch { }
        }

        /// <summary>
        /// 生成項目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (GenerateValid())
            {
                EnbaleControl(false);
                try
                {
                    GenerateSetValue();
                    this.txtOutput.Text = "";
                    this.txtOutput.Text += "开始生成，中途如关闭电源或关闭本程序，将会出现未知的错误\r\n";
                    this.txtOutput.Text += "请耐心等待\r\n";

                    var class_upper = this.tbClassUpper.IsChecked.Value;
                    var field_upper = this.tbFieldUpper.IsChecked.Value;
                    Preloader.Show(Global.instance, o =>
                    {
                        Entrance.DBGenerate(Global.path, Global.projName, Global.btype, class_upper, field_upper);
                    });

                    this.txtOutput.Text += "全部操作完毕\r\n";

                    TheEvent.ShowMessageBox("生成成功", "提示");
                }
                catch (Exception ex)
                {
                    this.txtOutput.Text += "生成失败，原因：" + ex.ToString();
                }
                EnbaleControl(true);
            }
        }

        /// <summary>
        /// 生成信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void TheEvent_InfoChangedEvent(object sender, InfoEventArgs args)
            => this.Dispatcher.BeginInvoke((Action)delegate () 
            {
                this.txtOutput.Text += args.info + "\r\n";
                this.txtOutput.ScrollToEnd();
            });

        /// <summary>
        /// 
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
                this.txtDalName.Text = this.txtProjName.Text + "Dal";
            }
            else
            {
                this.txtModelName.Text = "";
                this.txtDalName.Text = "";
            }
        }

        /// <summary>
        /// 勾選只生成實體
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbmodel_Checked(object sender, RoutedEventArgs e)
        {
            this.tbproj.IsChecked = !this.tbmodel.IsChecked;
            this.tball.IsChecked = !this.tbmodel.IsChecked;

            this.txtDalName.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 勾選只生成實體項目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbproj_Checked(object sender, RoutedEventArgs e)
        {
            this.tbmodel.IsChecked = !this.tbproj.IsChecked;
            this.tball.IsChecked = !this.tbproj.IsChecked;

            this.txtDalName.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 勾選只生成實體+數據層項目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tball_Checked(object sender, RoutedEventArgs e)
        {
            this.tbmodel.IsChecked = !this.tball.IsChecked;
            this.tbproj.IsChecked = !this.tball.IsChecked;

            this.txtDalName.Visibility = Visibility.Visible;
        }

        private void comboSqltype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = this.comboSqltype.SelectedItem as ComboBoxItem;
            var value = item.Content.ToString();
            if (value is "mysql")
            {
                this.txtport.Text = "3306";
            }
            else if (value is "oracle")
            {
                this.txtport.Text = "1521";
            }
            else if (value is "sqlserver")
            {
                this.txtport.Text = "1433";
            }
            else if (value is "postgres")
            {
                this.txtport.Text = "5432";
            }
            else if (value is "mongodb")
            {
                this.txtport.Text = "27017";
            }
        }

        #endregion

    }
}
