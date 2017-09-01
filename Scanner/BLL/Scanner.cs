using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Scanner.Model;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Scanner.BLL
{
    public class Scanner
    {
        #region Delegate& Event
        public delegate void ScanPortCompleteHandler(List<PortInfo> result);

        public delegate void ScanProgressHandler(int port);

        public delegate void ScanedCanConnectHandler(PortInfo canConnPort);

        public event ScanPortCompleteHandler OnScanPortComplete;

        public event ScanProgressHandler OnScanProgress;

        public event ScanedCanConnectHandler OnScanedCanConnect;
        #endregion

        #region Filed
        private string m_Domain;
        Socket tcpSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //地址
        IPAddress address;
        //Set锁
        object Lock = new object();
        //结果集锁
        object ResultLock = new object();

        HashSet<int> Set = new HashSet<int>();

        List<PortInfo> ResultList = new List<PortInfo>();

        private int m_StartPort = 1;

        private int m_EndPort = 65535;

        private string m_Ipaddress = string.Empty;
        #endregion

        #region Property
        /// <summary>
        /// 程序要扫描的域
        /// </summary>
        public string Domain
        {
            get { return m_Domain; }
            set
            {
                IPAddress outer;
                bool parseResult = IPAddress.TryParse(value,out outer);
                if (parseResult)
                {
                    m_Domain = value;
                }
                else
                {
                    IPAddress[] DnsAnalysis;
                    try
                    {
                        DnsAnalysis = Dns.GetHostAddresses(value);
                    }
                    catch (SocketException e)
                    {
                        throw new Exception("设置的IP地址或域名错误");
                    }
                }
            }
        }
        /// <summary>
        /// 扫描的开始端口
        /// </summary>
        public int StartPort
        {
            get { return m_StartPort; }
            set { if (value > m_EndPort) { throw new Exception("起始端口大于终点端口"); } else if (value < 1) { throw new Exception("端口值不能小于1"); } else { m_StartPort = value; } }
        }
        /// <summary>
        /// 扫描的终止端口
        /// </summary>
        public int EndPort
        {
            get { return m_EndPort; }
            set { if (value < m_StartPort) { throw new Exception("终止结点小于起始结点"); } else if (value > 65535) { throw new Exception("端口值 大于65535"); } else { m_EndPort = value; } }
        }

        public string Ipaddress
        {
            get
            {
                return m_Ipaddress;
            }
        }
        #endregion
        /// <summary>
        /// 外部调用开始扫描函数
        /// </summary>
        public void Scanning()
        {
            Set.Clear();
            ResultList.Clear();
            List<PortInfo> result = new List<PortInfo>();
            if (string.IsNullOrEmpty(m_Domain))
            {
                throw new Exception("domain 为空");
            }
            IPAddress ipAdd;
            bool ParseResult = IPAddress.TryParse(m_Domain, out ipAdd);

            if (!ParseResult)
            {
                IPAddress[] addressList = Dns.GetHostAddresses(m_Domain);
                if (addressList.Count() > 0)
                {
                    address = addressList[0];
                    m_Ipaddress = address.ToString();
                }
                else
                { throw new Exception("输入的域名或IP错误"); }
            }
            else
            {
                address = ipAdd;
                m_Ipaddress = address.ToString();
            }
            Func<List<PortInfo>> func = new Func<List<PortInfo>>(ScanAction);
            func.BeginInvoke(new AsyncCallback((t) =>
            {
                AsyncResult r = (AsyncResult)t;
                Func<List<PortInfo>> del = (Func<List<PortInfo>>)r.AsyncDelegate;
                List<PortInfo> rtn = del.EndInvoke(t);
                //触发事件
                OnScanPortComplete(rtn);
            }), this);
        }

        /// <summary>
        /// 扫描动作函数
        /// </summary>
        /// <returns></returns>
        private List<PortInfo> ScanAction()
        {
            List<PortInfo> result = new List<PortInfo>();
            IPEndPoint endPoint;
            /*此处需要优化不然 启动的太慢 2017-08-30*/
            for (int i = StartPort; i < EndPort; i++)
            {
                endPoint = new IPEndPoint(address, i);
                //对每个端口进行异步不然太慢了
                Func<IPEndPoint, bool> fun = new Func<IPEndPoint, bool>(Scan);
                fun.BeginInvoke(endPoint, new AsyncCallback((t) =>
                {
                    AsyncResult r = (AsyncResult)t;
                    int port = (int)r.AsyncState;
                    Func<IPEndPoint, bool> del = (Func<IPEndPoint, bool>)r.AsyncDelegate;
                    bool rtn = del.EndInvoke(t);
                    //加锁设置 Set对应的port值
                    lock (Lock)
                    {
                        Set.Add(port);
                        OnScanProgress(Set.Count());
                    }
                }), i);
            }
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                lock (Lock)
                {
                    int i = StartPort;
                    for (; i < EndPort; i++)
                    {
                        if (Set.Contains(i))
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (i == EndPort)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            result = ResultList;
            return result;
        }
        /// <summary>
        /// 细化的扫描部分
        /// </summary>
        /// <param name="endPoint">扫描的远程终结点</param>
        /// <returns></returns>
        private bool Scan(IPEndPoint endPoint)
        {
            bool result = true;
            tcpSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                tcpSock.Connect(endPoint);
            }
            catch (Exception e)
            {
                result = false;
            }
            finally
            {
                if (tcpSock.Connected)
                {
                    tcpSock.Close();
                }
                tcpSock.Dispose();
            }
            if (result)
            {
                lock (ResultLock)
                {
                    PortInfo info = new PortInfo(endPoint.Port);
                    ResultList.Add(info);
                    if (OnScanedCanConnect.Target != null)
                    {
                        OnScanedCanConnect(info);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 判断一个字符串是否是当前机器可达的地址或域名
        /// </summary>
        /// <param name="domain">要验证的地址或域名</param>
        /// <returns></returns>
        public static bool IsLegalDomain(string domain)
        {
            bool result = false;
            IPAddress address;
            bool isNumbericIpAddress = IPAddress.TryParse(domain, out address);
            if (!isNumbericIpAddress)
            {
                IPAddress[] domainsIpadress = Dns.GetHostAddresses(domain);
                if (domainsIpadress.Count() > 0)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }
    }
}
