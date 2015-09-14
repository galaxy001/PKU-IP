using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PC
{
    class WB
    {
        struct 学生
        {
            public string 学号;
            public string 密码;
            private static string code = "|;kiDrqvfi7d$v0p5Fg72Vwbv2;|";//固定字符串
            public string postData() { return "fwrd=free&username1=" + 学号 + "&password=" + 密码 + "&username=" + 学号 + code + 密码 + code + "12"; }//组合出的POST表单内容
        }

        学生 欧伟科;
        
        private WebBrowser webBrowser = new WebBrowser();
        public WB()
        {
            欧伟科.学号 = "1301110110";
            欧伟科.密码 = "oudanyi6456";
            webBrowser.LoadCompleted += new System.Windows.Navigation.LoadCompletedEventHandler(WebBrowser_LoadCompleted);
            //webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
        }

        private void WebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            mshtml.IHTMLDocument2 htmlDoc = (mshtml.IHTMLDocument2)webBrowser.Document;
            Console.WriteLine(htmlDoc.body.innerText);
            //throw new NotImplementedException();
        }



        public void Connect()
        {
            webBrowser.Focus();
            webBrowser.Navigate("https://its.pku.edu.cn/cas/login", "", Encoding.UTF8.GetBytes(欧伟科.postData()), "Content-Type: application/x-www-form-urlencoded\n");

            //Console.WriteLine(webBrowser.Document.innerHTML);

        }
    }

}
