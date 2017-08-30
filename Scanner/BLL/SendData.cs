using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Scanner.BLL
{
    /// <summary>
    /// 封装发送数据类
    /// </summary>
    public class SendData
    {
        #region Filed
        int m_Port;

        string m_IP = string.Empty;

        string m_Encoding = System.Text.Encoding.Default.WebName;

        int m_TimeOut;

        object Lock = new object();

        object TimeOutLock = new object();

        bool IsTransferNow = false;
        #endregion

        #region  Property
        /// <summary>
        /// 设置或获取发送数据的端口
        /// </summary>
        public int Port { get { return m_Port; } set { if (value > 65535 || value < 1) { throw new Exception("非法端口"); } else { m_Port = value; } } }

        /// <summary>
        /// 设置或获取发送数据使用的IP地址
        /// </summary>
        public string IP
        {
            get { return m_IP; }
            set
            {
                IPAddress outer;
                bool result = false;
                result = IPAddress.TryParse(value, out outer);
                if (result)
                {
                    m_IP = value;
                }
                else
                {
                    throw new Exception("IP地址非法");
                }
            }
        }

        /// <summary>
        /// 设置或获取Sender使用的编码格式
        /// </summary>
        public string Encoding
        {
            get { return m_Encoding; }
            set
            {
                try
                {
                    System.Text.Encoding.GetEncoding(value);
                }
                catch (ArgumentException e)
                {
                    throw new Exception("请输入系统可以支持的编码类型");
                }
                m_Encoding = value;
            }
        }
        /// <summary>
        /// 设置或获取获取结果时的超时时间
        /// </summary>
        public int TimeOut
        {
            get { return m_TimeOut; }
            set
            {
                if (m_TimeOut < 0 || m_TimeOut > 20)
                {
                    throw new Exception("超时时间不在0-20之间");
                }
                else
                {
                    m_TimeOut = value;
                }
            }
        }
        #endregion

        /// <summary>
        /// 向远端端口发送数据并获取返回的数据
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <exception cref="Exception">上一个请求还没有完成</exception>
        /// <returns>返回的字节流</returns>
        public byte[] GetResult(string data)
        {
            byte[] result = null;
            return result;
        }
        /// <summary>
        /// 向远端端口发送数据并获取返回的数据
        /// </summary>
        /// <param name="date">要发送的数据</param>
        /// <exception cref="Exception">上一个请求还没有完成</exception>
        /// <returns>返回的字节流</returns>
        public byte[] GetResult(byte[] data)
        {
            if (!(data != null && data.Length > 0))
            {
                throw new Exception("发送的数据为空");
            }
            byte[] result = null;
            try
            {
                GetLock();
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress address = IPAddress.Parse(m_IP);
                IPEndPoint endPoint = new IPEndPoint(address, m_Port);
                socket.Connect(endPoint);
                int sendLength = socket.Send(data);
                Func<Socket, byte[], int> del = null;
                if (sendLength == data.Length)
                {
                    del = new Func<Socket, byte[], int>((sock, resultByte) =>
                    {
                        int r = 0;
                        byte[] buff = new byte[512];
                        //持续接收
                        while (true)
                        {
                            int reciveLength = socket.Receive(buff, 0, 512, SocketFlags.None);
                            //缓冲区接满
                            if (reciveLength <= 512)
                            {

                            }
                            using ()
                        }
                        return r;
                    });
                    //异步以使用TimeOut，回调报告已接收完成
                    del.BeginInvoke(socket, result, new AsyncCallback((isyncresult) =>
                    {
                       
                    }), this);
                }
                else
                {
                    throw new Exception("发送数据失败");
                }
                while (true)
                {
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }
        /// <summary>
        /// 以异步调用的方式获取远程端口的返回值
        /// </summary>
        /// <param name="callBack">回调</param>
        /// <param name="date">要发送的数据</param>
        /// <param name="result">结果</param>
        /// <exception cref="Exception">上一个请求还没有完成</exception>
        public void AsyncGetResult(AsyncCallback callBack, string data, byte[] result)
        {
        }
        /// <summary>
        /// 以异步调用的方式获取远程端口的返回值
        /// </summary>
        /// <param name="callBack">回调</param>
        /// <param name="date">要发送的数据</param>
        /// <param name="result">结果</param>
        /// <exception cref="Exception">上一个请求还没有完成</exception>
        public void AsyncGetResult(AsyncCallback callBack, byte[] data, byte[] result)
        {

        }

        /// <summary>
        /// 获取锁方法(请保证该方法在请求返回结果的方法首部在try块中被执行)
        /// </summary>
        private void GetLock()
        {
            lock (Lock)
            {
                if (IsTransferNow)
                {
                    throw new Exception("上一个请求还没有完成");
                }
            }
        }
    }
}
