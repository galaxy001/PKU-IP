using System;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
namespace PC
{

    public class HWR
    {
        public struct 用户信息
        {
            public string 学号 { set; get; }
            public string 密码 { set; get; }
            private static fwrd 类型 = fwrd.free;
            public string value;//断开指定连接用
            private static string code = "|;kiDrqvfi7d$v0p5Fg72Vwbv2;|";//固定字符串
            public string postData() { return "fwrd=" + 类型 + "&username1=" + 学号 + "&password=" + 密码 + "&username=" + 学号 + code + 密码 + code + (short)类型; }//组合出的POST表单内容
        }
        public enum fwrd : short
        {
            fee = 11,
            free = 12,
            pkuwireless = 14
        }
        public 用户信息 学生;
        public HttpWebRequest Request;
        public CookieContainer cContainer = new CookieContainer();
        public HWR()
        {
            //学生.学号 = "1301110110";
            //学生.密码 = "oudanyi6456";
            //学生.类型 = fwrd.free;
            
        }

        public void 建立连接(string URL, string PostData = "")
        {
            Request = (HttpWebRequest)WebRequest.Create(URL);
            if (PostData != "")
            {
                Request.Method = "POST";//设置PostData
                byte[] byteArray = Encoding.UTF8.GetBytes(PostData);
                Request.ContentType = "application/x-www-form-urlencoded";
                Request.ContentLength = byteArray.Length;

                Stream dataStreamRequest = Request.GetRequestStream();
                dataStreamRequest.Write(byteArray, 0, byteArray.Length);
                dataStreamRequest.Close();
            }
            Request.CookieContainer = cContainer;
        }

        public void 获得cookie()
        {

            建立连接("http://its.pku.edu.cn/cas/login", 学生.postData());
            HttpWebResponse response = (HttpWebResponse)Request.GetResponse();
            //读取cookie(Request);
            //读取网页(response);
            response.Close();
            //cContainer.GetCookies(request.RequestUri)[0].Expires = cContainer.GetCookies(request.RequestUri)[1].Expires;
        }

        public string[] 连接(string 连接类型)
        {
            获得cookie();
            建立连接("http://its.pku.edu.cn/netportal/" + 连接类型);
            HttpWebResponse response = (HttpWebResponse)Request.GetResponse();
            //读取网页(response);
            string 文档 = 分析网页(response);
            response.Close();
            string[] Content = 返回信息(文档);

            if (Content[0].Contains("NO") && 连接类型.StartsWith("ipgwopen")) return disConnectSpe(文档);
            return Content;

        }

        /// <summary>
        /// 断开指定连接
        /// </summary>
        public string[] disConnectSpe(string 文档)
        {
            string postData;
            int i = 文档.IndexOf("\"messages\" value") + 18;
            int j = 文档.IndexOf("\">", i);
            学生.value = 文档.Substring(i, j - i);

            postData = "messages=" + 学生.value + "&operation=get_disconnectip_err&from=cas&uid=" + 学生.学号 + "&timeout=1&range=2";

            建立连接("http://its.pku.edu.cn/netportal/ipgw.ipgw?" + postData);//POST不行GET却可以
            HttpWebResponse response = (HttpWebResponse)Request.GetResponse();
            string IP地址 = 分析网页(response);
            response.Close();

            //断开指定连接
            string[] IP = new string[4];
            Regex[] reg = new Regex[2];
            reg[0] = new Regex(@"(?<=td2>)\d+(\.\d+){3}?");//IP地址
            reg[1] = new Regex(@"(?<=td2>)[\d\-]+\s[\d:]+\.0");//时间
            MatchCollection MC;
            for (i = 0; i < 2; i++)
            {
                MC = reg[i].Matches(IP地址);
                if (MC.Count < 2) break;
                for (j = 0; j < 2; j++) IP[i * 2 + j] = MC[j].Value;
            }
            DateTime 时间;
            for (i = 2; i < 4; i++)
            {
                时间 = Convert.ToDateTime(IP[i]);
                IP[i] = 时间.ToString("M-d hh:mm:ss");
            }
            return IP;


        }

        public string[] 断开指定连接(string IP)
        {
            //断开第一个连接
            string postData = "messages=" + 学生.value + "&operation=disconnectip_err&from=cas&uid=" + 学生.学号 + "&timeout=1&range=1&disconnectip=" + IP;
            建立连接("http://its.pku.edu.cn/netportal/ipgw.ipgw?" + postData);

            HttpWebResponse response = (HttpWebResponse)Request.GetResponse();
            string 文档 = 分析网页(response);
            response.Close();
            string[] Content = 返回信息(文档);
            return Content;
        }
        private void 读取cookie(HttpWebRequest request)
        {
            //读取cookie
            foreach (Cookie cook in cContainer.GetCookies(request.RequestUri))
            {
                Console.WriteLine("Cookie:");
                Console.WriteLine("{0} = {1}", cook.Name, cook.Value);
                Console.WriteLine("Domain: {0}", cook.Domain);
                Console.WriteLine("Path: {0}", cook.Path);
                Console.WriteLine("Port: {0}", cook.Port);
                Console.WriteLine("Secure: {0}", cook.Secure);

                Console.WriteLine("When issued: {0}", cook.TimeStamp);
                Console.WriteLine("Expires: {0} (expired? {1})", cook.Expires, cook.Expired);
                Console.WriteLine("Don't save: {0}", cook.Discard);
                Console.WriteLine("Comment: {0}", cook.Comment);
                Console.WriteLine("Uri for comments: {0}", cook.CommentUri);
                Console.WriteLine("Version: RFC {0}", cook.Version == 1 ? "2109" : "2965");

                // Show the string representation of the cookie.
                Console.WriteLine("String: {0}", cook.ToString());
            }
        }
        private void 读取网页(HttpWebResponse response)
        {
            //读取文件
            Stream dataStreamResponse = response.GetResponseStream();

            StreamReader reader = new StreamReader(dataStreamResponse);
            string ResponseFromServer = reader.ReadToEnd();
            Console.WriteLine(ResponseFromServer);
            reader.Close();
        }
        private string 分析网页(HttpWebResponse response)
        {
            Stream dataStreamResponse = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStreamResponse);
            string ResponseFromServer = reader.ReadToEnd();
            reader.Close();
            return ResponseFromServer;
        }
        private string[] 返回信息(string 文档)
        {
            int i = 文档.IndexOf("<!--IPGWCLIENT_START") + 21;
            int j = 文档.IndexOf("IPGWCLIENT_END-->");
            string Content = 文档.Substring(i, j - i - 1);
            return Content.Split(' ');
        }

    }


}
