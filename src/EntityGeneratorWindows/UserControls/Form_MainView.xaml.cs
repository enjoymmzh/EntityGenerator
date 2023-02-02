using EntityGeneratorWindows.Common;
using EntityGeneratorWindows.Model;
using EntityGeneratorWindows.Sql;
using EntityGeneratorWindows.Windows;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EntityGeneratorWindows.UserControls
{
    /// <summary>
    /// DB_MainView.xaml 的交互逻辑
    /// </summary>
    public partial class Form_MainView : UserControl
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

        public Form_MainView() 
        {
            InitializeComponent();
        }

        /// <summary>
        /// 創建表節點
        /// </summary>
        /// <param name="node"></param>
        private void CreateTableNode(TreeViewItem node)
        {
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
                    var stack = new StackPanel() { Orientation = Orientation.Horizontal };
                    stack.Children.Add(new TextBlock() { Margin = new Thickness(8, 0, 0, 0), Text = item.className });
                    node.Items.Add(new TreeViewItem() { Header = stack, Tag = item });
                }
                node.ExpandSubtree();
            }
        }

        private void CreateFieldsNode(string tablename, TreeViewItem node)
        {
            lastnode?.Items.Clear();
            lastnode = node;
            var classInfo = node.Tag as ClassInfo;

            IEnumerable<FieldInfo> list = null;
            Preloader.Show(Global.instance, o =>
            {
                var con = SqlType.Instance().Get(Global.sqlType);
                list = con.GetTableFieldList(tablename, classInfo);
            });

            if (list is not null && list.Count() > 0)
            {
                foreach (var item in list)
                {
                    var checkbox = new CheckBox() { IsChecked = true };
                    checkbox.Click += Checkbox_Click;

                    var stack = new StackPanel() { Orientation = Orientation.Horizontal };
                    stack.Children.Add(checkbox);
                    stack.Children.Add(new TextBlock() { Margin = new Thickness(8, 0, 0, 0), Text = item.filedName });

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
            foreach (TreeViewItem item in this.curnode.Items)
            {
                var panel = item.Header as StackPanel;
                var check = panel.Children[0] as CheckBox;
                if (check.IsChecked.Value)
                {
                    var aa = e.OriginalSource;
                    break;
                }
            }
        }

        /// <summary>
        /// 顯示表字段信息
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="tablenode"></param>
        private void CreateFieldInfo(string tablename, TreeViewItem tablenode)
        {
            var classInfo = tablenode.Tag as ClassInfo;

            IEnumerable<FieldInfo> list = null;
            Preloader.Show(Global.instance, o =>
            {
                var con = SqlType.Instance().Get(Global.sqlType);
                list = con.GetTableFieldList(tablename, classInfo);
            });
        }

        /// <summary>
        /// 點擊樹節點
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && this.treeView1.SelectedItem is not null)
            {
                var node = this.treeView1.SelectedItem as TreeViewItem;
                if (node is not null)
                {
                    var parent = node.Parent as TreeViewItem;
                    if (parent is not null)
                    {
                        var parentsparent = parent.Parent as TreeViewItem;
                        if (parentsparent == null)//如果是表節點
                        {
                            CreateTableNode(node);
                        }
                        else//如果是字段節點
                        {
                            this.curnode = node;
                            var table = parent.Tag as DataBaseInfo;
                            CreateFieldsNode(table.datname, node);
                        }
                    }
                }
            }
        }

        private bool Valid()
        {
            if (this.curnode is null)
            {
                TheEvent.ShowMessageBox("请选择一个表", "提示");
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.txtOrigin.Text))
            {
                TheEvent.ShowMessageBox("请输入模板，并用{0}标示参数，{1}标示参数注释", "提示");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Valid())
            {
                this.txtNew.Text = "";
                foreach (TreeViewItem item in this.curnode.Items)
                {
                    var panel = item.Header as StackPanel;
                    var check = panel.Children[0] as CheckBox;
                    if (check.IsChecked.Value)
                    {
                        var field = item.Tag as FieldInfo;
                        this.txtNew.Text += string.Format(this.txtOrigin.Text, field.filedName, field.filedComment) + "\r\n";
                    }
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

                if (list?.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        var stack1 = new StackPanel() { Orientation = Orientation.Horizontal };
                        stack1.Children.Add(new PackIcon() { Kind = PackIconKind.Database });
                        stack1.Children.Add(new TextBlock() { Margin = new Thickness(8, 0, 0, 0), Text = item.datname });

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
                if (list == null)
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
                    if (con == null)
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
