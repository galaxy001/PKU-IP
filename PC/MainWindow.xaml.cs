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

using System.Net;
using System.IO;

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
        }
        public HWR web = new HWR();
        static string[] 连接类型 = new string[] { "ipgwopen", "ipgwclose", "ipgwcloseall" };//连接, 断开连接, 断开所有连接
        public Button[] buttons = new Button[2];
        private void 连接(object sender, RoutedEventArgs e)
        {
            
            textBlock.Inlines.Clear();
            
            string[] Content = web.连接(连接类型[Convert.ToInt16(((Button)sender).Tag)]);
            判断(Content);
        }



        private void 指定连接(object sender, RoutedEventArgs e)
        {
            textBlock.Inlines.Clear();
            grid.Children.Remove(buttons[0]);
            grid.Children.Remove(buttons[1]);
            string[] Content = web.断开指定连接(((Button)sender).Tag.ToString());
            判断(Content);
        }
        private void 判断(string[] Content)
        {
            if (Content[0].Contains("YES"))
            {
                for (int i = 1; i < Content.Count(); i++)
                {
                    textBlock.Inlines.Add(new Run(Content[i]));
                    textBlock.Inlines.Add(new LineBreak());
                }
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {

                    buttons[i] = new Button();

                    buttons[i].Content = "断开IP" + (i + 1);
                    buttons[i].HorizontalAlignment = HorizontalAlignment.Center;
                    buttons[i].Width = 82;
                    buttons[i].Height = 27;
                    buttons[i].VerticalAlignment = VerticalAlignment.Bottom;
                    buttons[i].Tag = Content[i];
                    buttons[i].Margin = new Thickness((2 * i - 1) * 100, 0, 0, 94);
                    buttons[i].Click += 指定连接;
                    grid.Children.Add(buttons[i]);
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
