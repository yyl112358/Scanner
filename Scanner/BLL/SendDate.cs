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
    public class SendDate
    {
        #region Filed
        private int m_Port;

        private string m_IP=string.Empty;
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
            set{ m_IP = value;}
        }
        #endregion

    }
}
