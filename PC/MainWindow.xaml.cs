using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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

        }
        public HWR web = new HWR();

        public HWR.用户信息[] 学生信息 = new HWR.用户信息[2];
        
        static string[] 连接类型 = new string[] { "ipgwopen", "ipgwopenall", "ipgwclose", "ipgwcloseall" };//连接, 收费链接, 断开连接, 断开所有连接
        public Button[] IP按钮 = new Button[2];
        /// <summary>
        /// 利用Button内的Tag确定(断开)连接的类型
        /// </summary>
        /// <param name="sender">点击的Button种类</param>
        /// <param name="e"></param>
        private void 连接(object sender, RoutedEventArgs e)
        {
            //清除文本信息
            textBlock.Inlines.Clear();
            grid.Children.Remove(IP按钮[0]);
            grid.Children.Remove(IP按钮[1]);
            //确定学号和密码
            web.学生 = (HWR.用户信息)comboBox.SelectedItem;
            //确定免费/收费地址
            short Tag;
            Tag = Convert.ToInt16(((Button)sender).Tag);
            if ((bool)radioButton1.IsChecked && Tag == 0) { Tag++; }//收费

            string[] Content = web.连接(连接类型[Tag]);
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
