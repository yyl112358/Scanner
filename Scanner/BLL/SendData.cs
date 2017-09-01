using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;

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

        public event Action<int> OnTimeOut;
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
        /// 设置或获取获取结果时的超时时间(PS：单位S)
        /// </summary>
        public int TimeOut
        {
            get { return m_TimeOut; }
            set
            {
                if (value < 0 || value > 20)
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

        #region Method
        /// <summary>
        /// 向远端端口发送数据并获取返回的数据
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <exception cref="Exception">上一个请求在TimeOut时间内还没有完成又重新调用了该方法</exception>
        /// <returns>返回的字节流</returns>
        public byte[] GetResult(string data)
        {
            byte[] result = null;
            if (string.IsNullOrEmpty(data))
            {
                throw new Exception("请传入正确的要发送的字符信息");
            }
            System.Text.Encoding encoding = System.Text.Encoding.GetEncoding(this.m_Encoding);
            byte[] sendData = encoding.GetBytes(data);
            if (sendData != null && sendData.Length > 0)
            {
                result = GetResult(sendData);
            }
            else
            {
                throw new Exception("编码发生错误");
            }

            return result;
        }
        /// <summary>
        /// 向远端端口发送数据并获取返回的数据
        /// </summary>
        /// <param name="date">要发送的数据</param>
        /// <exception cref="Exception">上一个请求在TimeOut时间内还没有完成又重新调用了该方法</exception>
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
                Func<Socket, byte[]> del = null;
                if (sendLength == data.Length)
                {
                    del = new Func<Socket, byte[]>((sock) =>
                     {
                         byte[] resultByte = null;
                         SockAndResult Info = new SockAndResult() { sock = sock, Result = null };
                         #region 另起线程进行接收动作
                         Thread reciveThread = new Thread((o) =>
                          {
                              SockAndResult param = o as SockAndResult;
                              Socket s;
                              if (o == null)
                              {
                                  return;
                              }
                              else
                              {
                                  s = param.sock;
                              }

                              using (MemoryStream ms = new MemoryStream())
                              {
                                  do
                                  {
                                      byte[] buff = new byte[512];
                                      int reciveLength = 0;
                                      reciveLength = s.Receive(buff, 0, 512, SocketFlags.None);
                                      if (reciveLength > 0)
                                      {
                                          ms.Write(buff, 0, reciveLength);
                                      }
                                      if (reciveLength == 0)
                                      {
                                          break;
                                      }
                                      buff = new byte[512];
                                  }
                                  //持续接收
                                  while (s.Available > 0);
                                  byte[] reciveResult = new byte[ms.Length];
                                  ms.Seek(0, SeekOrigin.Begin);
                                  ms.Read(reciveResult, 0, (int)ms.Length);
                                  param.Result = reciveResult;
                              }
                              if (socket.Connected)
                              {
                                  socket.Shutdown(SocketShutdown.Both);
                                  socket.Close();
                                  //socket.Dispose();
                              }
                          });
                         reciveThread.IsBackground = true;
                         reciveThread.Start(Info);
                         #endregion

                         TimeSpan ts = TimeSpan.FromSeconds(this.TimeOut);
                         DateTime WaitTimeTo = DateTime.Now.Add(ts);
                         //当前线程监视运行时间
                         while (true)
                         {
                             Thread.Sleep(TimeSpan.FromSeconds(1));
                             if (reciveThread.ThreadState == ThreadState.Stopped)
                             {
                                 resultByte = Info.Result;
                                 break;
                             }
                             //超时
                             else if (WaitTimeTo.CompareTo(DateTime.Now) < 0)
                             {
                                 reciveThread.Abort();
                                 OnTimeOut?.Invoke(TimeOut);
                                 return null;
                             }
                         }
                         return resultByte;
                     });
                    byte[] invorkResult = del.Invoke(socket);
                    result = invorkResult;
                }
                else
                {
                    throw new Exception("发送数据失败");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            ReleaseLock();
            return result;
        }
        /// <summary>
        /// 以异步调用的方式获取远程端口的返回值
        /// </summary>
        /// <param name="callBack">回调</param>
        /// <param name="date">要发送的数据</param>
        /// <param name="result">结果</param>
        /// <exception cref="Exception">上一个请求在TimeOut时间内还没有完成又重新调用了该方法</exception>
        public void AsyncGetResult(Action<object> callBack, string data)
        {
            Func<string, byte[]> func = new Func<string, byte[]>(GetResult);
            func.BeginInvoke(data, new AsyncCallback((isyncresult) =>
            {
                AsyncResult res = isyncresult as AsyncResult;
                Func<string, byte[]> del = (Func<string, byte[]>)res.AsyncDelegate;
                byte[] reciveResult = del.EndInvoke(isyncresult);
                callBack(reciveResult);
            }), new object());

        }
        /// <summary>
        /// 以异步调用的方式获取远程端口的返回值
        /// </summary>
        /// <param name="callBack">回调</param>
        /// <param name="date">要发送的数据</param>
        /// <param name="result">结果</param>
        /// <exception cref="Exception">上一个请求在TimeOut时间内还没有完成又重新调用了该方法</exception>
        public void AsyncGetResult(Action<object> callBack, byte[] data)
        {
            Func<byte[], byte[]> func = new Func<byte[], byte[]>(GetResult);
            func.BeginInvoke(data, new AsyncCallback((isyncresult)=> {
                AsyncResult res = isyncresult as AsyncResult;
                Func<byte[], byte[]> del = (Func<byte[], byte[]>)res.AsyncDelegate;
                byte[] reciveResult =del.EndInvoke(isyncresult);
                callBack(reciveResult);
            }),new object());
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
                IsTransferNow = true;
            }
        }

        private void ReleaseLock()
        {
            lock (Lock)
            {
                IsTransferNow = false;
            }
        }

        /// <summary>
        /// 判断指定的编码类型名称在当前系统中是否可用
        /// </summary>
        /// <param name="Encodname">要判断的编码的名称</param>
        /// <returns></returns>
        public static bool IsCanUseEncode(string Encodname)
        {
            bool result = true;
            try
            {
                System.Text.Encoding.GetEncoding(Encodname);
            }
            catch (ArgumentException e)
            {
                result = false;
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 封装内部包含以连接远程终结点的Socket返回字节数组的类
        /// </summary>
        class SockAndResult
        {
            public Socket sock { get; set; }

            public byte[] Result { get; set; }
        }
    }
}
