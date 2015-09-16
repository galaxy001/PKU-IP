using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//using System.Windows.Forms;

namespace PC
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //增加用户信息列表
            学生信息[0] = new HWR.用户信息 { 学号 = "1301110110", 密码 = "oudanyi6456" };
            学生信息[1] = new HWR.用户信息 { 学号 = "gcuspea", 密码 = "phyfsc508" };

            comboBox.ItemsSource = 学生信息;
            comboBox.DisplayMemberPath = "学号";
            comboBox.SelectedValue = 学生信息[0];//默认
            
            系统托盘();

        }
        System.Windows.Forms.NotifyIcon 图标 = new System.Windows.Forms.NotifyIcon();
        private void 系统托盘()
        {

            图标.Icon = System.Drawing.SystemIcons.Exclamation;
            
            图标.Visible = true;
            图标.ContextMenu = new System.Windows.Forms.ContextMenu();
            图标.ContextMenu.MenuItems.Add("连接", 连接);
            图标.ContextMenu.MenuItems.Add("断开连接", 连接);
            图标.ContextMenu.MenuItems.Add("断开所有连接", 连接);
            for (int i = 0; i < 3; i++)
            {
                图标.ContextMenu.MenuItems[i].Tag = i;
            }
          
            图标.ContextMenu.MenuItems.Add("退出", 退出);
            


            图标.DoubleClick += delegate (object sender, EventArgs args)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };
            //lambda表达式, 窗口关闭时notifyicon关闭
            Closed += 退出;
            StateChanged += (sender, e) =>
            {
                if (WindowState == WindowState.Minimized) ShowInTaskbar = false;
                else if (WindowState == WindowState.Normal) ShowInTaskbar = true;
            };

        }
        private void 退出(object sender,EventArgs args)
        {
            if (图标 != null)
            {
                //图标.Visible = false;
                图标.Dispose();
                图标 = null;
            }
            Close();
        }
        public HWR web = new HWR();

        public HWR.用户信息[] 学生信息 = new HWR.用户信息[2];

        static string[] 连接类型 = new string[] { "ipgwopen", "ipgwclose", "ipgwcloseall", "ipgwopenall" };//连接, 收费链接, 断开连接, 断开所有连接
        short 连接Tag = 0;//连接类型的index

        public Button[] IP按钮 = new Button[2];


        /// <summary>
        /// 利用Button内的Tag确定(断开)连接的类型
        /// </summary>
        /// <param name="sender">点击的Button种类</param>
        /// <param name="e"></param>
        private void 连接(object sender, EventArgs e)
        {
            //清除文本信息
            textBlock.Inlines.Clear();
            grid.Children.Remove(IP按钮[0]);
            grid.Children.Remove(IP按钮[1]);
            //确定学号和密码
            web.学生 = (HWR.用户信息)comboBox.SelectedItem;
            //确定免费/收费地址
            try
            {
                连接Tag = Convert.ToInt16(((Button)sender).Tag);
            }
            catch (System.InvalidCastException)
            {
                连接Tag = Convert.ToInt16(((System.Windows.Forms.MenuItem)sender).Tag);
            }
            //连接Tag = Convert.ToInt16(((Button)sender).Tag);
            if ((bool)radioButton1.IsChecked && 连接Tag == 0) 连接Tag = 3; //收费

            string[] Content = web.连接(连接类型[连接Tag]);
            判断(Content);
        }


        /// <summary>
        /// 通过点击Button断开指定的IP地址网关
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 指定连接(object sender, RoutedEventArgs e)
        {
            textBlock.Inlines.Clear();
            grid.Children.Remove(IP按钮[0]);
            grid.Children.Remove(IP按钮[1]);
            string[] Content = web.断开指定连接(((Button)sender).Tag.ToString());
            判断(Content);
        }
        private void 判断(string[] Content)
        {
            if (Content[0].Contains("YES"))//(断开)连接成功, 显示信息
            {
                for (int i = 1; i < Content.Count(); i++)
                {
                    textBlock.Inlines.Add(new Run(Content[i]));
                    textBlock.Inlines.Add(new LineBreak());
                    if (连接Tag == 3) 图标.Icon = new System.Drawing.Icon(@"E:\Code\Visual Studio\网关\PC\Resources\图标.ico");//收费连接更改图标
                    else 图标.Icon = System.Drawing.SystemIcons.Exclamation;
                }
            }
            else//连接失败
            {
                for (int i = 0; i < 2; i++)//新建选IP地址断开的按钮
                {

                    IP按钮[i] = new Button();

                    IP按钮[i].Content = "断开IP" + (i + 1);
                    IP按钮[i].HorizontalAlignment = HorizontalAlignment.Center;
                    IP按钮[i].Width = 82;
                    IP按钮[i].Height = 27;
                    IP按钮[i].VerticalAlignment = VerticalAlignment.Bottom;
                    IP按钮[i].Tag = Content[i];
                    IP按钮[i].Margin = new Thickness((2 * i - 1) * 100, 0, 0, 94);
                    IP按钮[i].Click += 指定连接;
                    grid.Children.Add(IP按钮[i]);
                }

                textBlock.Inlines.Add(new Run("您当前已经打开2个网络连接, 请断开指定连接"));
                for (int i = 0; i < 2; i++)
                {
                    textBlock.Inlines.Add(new LineBreak());
                    textBlock.Inlines.Add(new Run("IP" + (i + 1) + ":" + Content[i] + "\t" + Content[i + 2]));
                }
            }
        }

    }
}
