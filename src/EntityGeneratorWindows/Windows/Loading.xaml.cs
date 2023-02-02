using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EntityGeneratorWindows.Windows
{
    /// <summary>
    /// Loading.xaml 的交互逻辑
    /// </summary>
    public partial class Loading : Window
    {
        [Description("工作是否完成")]
        public bool IsWorkCompleted;

        [Description("工作动作")]
        private ParameterizedThreadStart _workAction;

        [Description("工作动作参数")]
        private object _workActionArg;

        [Description("工作线程")]
        private Thread _workThread;

        [Description("工作异常")]
        public Exception WorkException { get; private set; }

        [Browsable(true), Category("Appearance"), Description("设置前景色")]
        public Color Color = Color.FromArgb(255, 255, 128, 0);

        /// <summary>
        /// 
        /// </summary>
        private DispatcherTimer timer;

        private static Loading instance = null;
        public static Loading Instance()
        {
            if (instance == null)
            {
                instance = new Loading();
            }
            instance.timer.Start();//设置定时器 
            return instance;
        }

        public Loading()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(1000000);   //时间间隔为一秒
            timer.Tick += new EventHandler(timer_Tick);
        }

        /// <summary>
        /// 设置工作动作
        /// </summary>
        /// <param name="workAction"></param>
        /// <param name="arg"></param>
        public void SetWorkAction(ParameterizedThreadStart workAction, object arg)
        {
            _workAction = workAction;
            _workActionArg = arg;

            if (_workAction is not null)
            {
                _workThread = new Thread(new ThreadStart(ExecWorkAction));
                _workThread.SetApartmentState(ApartmentState.STA);
                _workThread.IsBackground = true;
                _workThread.Start();
            }
        }

        /// <summary>
        /// 执行工作动作
        /// </summary>
        private void ExecWorkAction()
        {
            try
            {
                var workTask = new Task(arg =>
                {
                    _workAction(arg);
                }, _workActionArg);
                workTask.Start();
                Task.WaitAll(workTask);
            }
            catch (Exception exception)
            {
                WorkException = exception;
            }
            finally
            {
                IsWorkCompleted = true;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (IsWorkCompleted)
            {
                IsWorkCompleted = false;
                timer.Stop();
                this.Hide();
            }
        }

    }

    /// <summary>
    /// 加载
    /// </summary>
    public class Preloader
    {
        /// <summary>
        /// 开始加载
        /// </summary>
        /// <param name="work">待执行工作</param>
        /// <param name="workArg">工作参数</param>
        public static void Show(Window window, ParameterizedThreadStart work, object workArg = null)
        {
            var loading = Loading.Instance();
            dynamic expandoObject = new ExpandoObject();
            expandoObject.Form = loading;
            expandoObject.WorkArg = workArg;
            loading.SetWorkAction(work, expandoObject);
            loading.WindowStartupLocation = WindowStartupLocation.Manual;
            loading.Top = window.Top + window.Height / 2;
            loading.Left = window.Left + window.Width / 2;
            loading.ShowDialog();
            if (loading.WorkException is not null)
            {
                throw loading.WorkException;
            }
        }
    }

    ///// <summary>
    ///// Loading.xaml 的交互逻辑
    ///// </summary>
    //public partial class Loading : Window
    //{
    //    [Description("工作是否完成")]
    //    public bool IsWorkCompleted;

    //    [Description("工作动作")]
    //    private ParameterizedThreadStart _workAction;

    //    [Description("工作动作参数")]
    //    private object _workActionArg;

    //    [Description("工作线程")]
    //    private Thread _workThread;

    //    [Description("工作异常")]
    //    public Exception WorkException { get; private set; }

    //    [Browsable(true), Category("Appearance"), Description("设置前景色")]
    //    public Color Color = Color.FromArgb(255, 255, 128, 0);

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    private DispatcherTimer timer;

    //    private static Loading instance = null;
    //    public static Loading Instance()
    //    {
    //        if (instance == null)
    //        {
    //            instance = new Loading();
    //        }
    //        return instance;
    //    }

    //    public Loading()
    //    {
    //        InitializeComponent();
    //        InitData();
    //    }

    //    public void InitData()
    //    {
    //        //设置定时器          
    //        timer = new DispatcherTimer();
    //        timer.Interval = new TimeSpan(1000000);   //时间间隔为一秒
    //        timer.Tick += new EventHandler(timer_Tick);
    //        timer.Start();
    //    }

    //    /// <summary>
    //    /// 设置工作动作
    //    /// </summary>
    //    /// <param name="workAction"></param>
    //    /// <param name="arg"></param>
    //    public void SetWorkAction(ParameterizedThreadStart workAction, object arg)
    //    {
    //        _workAction = workAction;
    //        _workActionArg = arg;

    //        if (_workAction != null)
    //        {
    //            _workThread = new Thread(new ThreadStart(ExecWorkAction));
    //            _workThread.SetApartmentState(ApartmentState.STA);
    //            _workThread.IsBackground = true;
    //            _workThread.Start();
    //        }
    //    }

    //    /// <summary>
    //    /// 执行工作动作
    //    /// </summary>
    //    private void ExecWorkAction()
    //    {
    //        try
    //        {
    //            var workTask = new Task(arg =>
    //            {
    //                _workAction(arg);
    //            }, _workActionArg);
    //            workTask.Start();
    //            Task.WaitAll(workTask);
    //        }
    //        catch (Exception exception)
    //        {
    //            WorkException = exception;
    //        }
    //        finally
    //        {
    //            IsWorkCompleted = true;
    //        }
    //    }

    //    private void timer_Tick(object sender, EventArgs e)
    //    {
    //        if (IsWorkCompleted)
    //        {
    //            IsWorkCompleted = false;
    //            timer.Stop();
    //            this.Hide();
    //        }
    //    }

    //}

    ///// <summary>
    ///// 加载
    ///// </summary>
    //public class Preloader
    //{
    //    /// <summary>
    //    /// 开始加载
    //    /// </summary>
    //    /// <param name="work">待执行工作</param>
    //    /// <param name="workArg">工作参数</param>
    //    public static void Show(Window window, ParameterizedThreadStart work, object workArg = null)
    //    {
    //        var loading = Loading.Instance();
    //        dynamic expandoObject = new ExpandoObject();
    //        expandoObject.Form = loading;
    //        expandoObject.WorkArg = workArg;
    //        loading.SetWorkAction(work, expandoObject);
    //        loading.WindowStartupLocation = WindowStartupLocation.Manual;
    //        loading.Top = window.Top + window.Height / 2;
    //        loading.Left = window.Left + window.Width / 2;
    //        loading.ShowDialog();
    //        if (loading.WorkException != null)
    //        {
    //            throw loading.WorkException;
    //        }
    //    }
    //}
}
