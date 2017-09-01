using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scanner;
using Scanner.BLL;
using System.Collections;
using System.Collections.Generic;
using Scanner.Model;

namespace Scanner.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestSend()
        {
            SendData sd = new SendData();
            sd.IP = "10.2.16.94";
            sd.Port = 8080;
            sd.Encoding = "UTF-8";
            sd.TimeOut = 50;
            string SendStr = @"GET /tomcat.css HTTP/1.1
Host: 10.2.16.94:8080
Connection: keep-alive
Pragma: no-cache
Cache-Control: no-cache
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.104 Safari/537.36 Core/1.53.3368.400 QQBrowser/9.6.11860.400
Accept: text/css,*/*;q=0.1
Accept-Encoding: gzip, deflate, sdch
Accept-Language: zh-CN,zh;q=0.8

";
            byte[] result = sd.GetResult(SendStr);
            string resultStr = System.Text.Encoding.UTF8.GetString(result);
            byte[] result2 = sd.GetResult(SendStr);
            string resultStr2 = System.Text.Encoding.UTF8.GetString(result2);
        }

        [TestMethod]
        public void TestAsync()
        {
            SendData sd = new SendData();
            sd.IP = "10.2.16.94";
            sd.Port = 8080;
            sd.Encoding = "UTF-8";
            sd.TimeOut = 50;
            string SendStr = @"GET /tomcat.css HTTP/1.1
Host: 10.2.16.94:8080
Connection: keep-alive
Pragma: no-cache
Cache-Control: no-cache
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.104 Safari/537.36 Core/1.53.3368.400 QQBrowser/9.6.11860.400
Accept: text/css,*/*;q=0.1
Accept-Encoding: gzip, deflate, sdch
Accept-Language: zh-CN,zh;q=0.8

";
            byte[] result;
            int i = 5;
            sd.AsyncGetResult(new Action<object>((o) => { i = 20; }), SendStr);
        }
        [TestMethod]
        public void TestAsyn()
        {
            SendData sd = new SendData();
            sd.IP = "10.2.16.94";
            sd.Port = 8080;
            sd.Encoding = "UTF-8";
            sd.TimeOut = 50;
            string SendStr = @"GET /tomcat.css HTTP/1.1
Host: 10.2.16.94:8080
Connection: keep-alive
Pragma: no-cache
Cache-Control: no-cache
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.104 Safari/537.36 Core/1.53.3368.400 QQBrowser/9.6.11860.400
Accept: text/css,*/*;q=0.1
Accept-Encoding: gzip, deflate, sdch
Accept-Language: zh-CN,zh;q=0.8
";
            byte[] sender = System.Text.Encoding.UTF8.GetBytes(SendStr);
            sd.AsyncGetResult(new Action<object>((o) =>
            {
                byte[] resultBytes = o as byte[];
                if (resultBytes != null)
                {
                    string resultStr = System.Text.Encoding.UTF8.GetString(resultBytes);
                }
            }), sender);
            sd.OnTimeOut += (i) => {
                int j = i;

            };
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(20));
        }
        [TestMethod]
        public void TestEncodingStaticMethod()
        {
            string[] encodNames = new string[] { "UTF-8", "test", "Unicode", "ASCII" };
            List<bool> result = new List<bool>();
            foreach (var name in encodNames)
            {
                bool testResult = BLL.SendData.IsCanUseEncode(name);
                result.Add(testResult);
            }
        }
        [TestMethod]
        public void TestAddressValidete()
        {
            BLL.Scanner scan = new BLL.Scanner();
            //scan.Domain = "192.168.0.1";
            scan.Domain = "fsdfs.dsfk.com";
        }
    }
}
