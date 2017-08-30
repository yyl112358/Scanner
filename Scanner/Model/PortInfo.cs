using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Scanner.Model
{
    public class PortInfo
    {
        #region Property
        public string Name { get; set; }

        public int Port { get; set;}

        public string Message { get; set; }

        #endregion

        #region Constructor
        public PortInfo(int port)
        {
            this.Port = port;
        }
        public PortInfo(int port, string Name) : this(port)
        {
            this.Name = Name;
        }
        public PortInfo(int port, string Name, string Message) : this(port, Name)
        {
            this.Message = Message;
        }

        public override string ToString()
        {
            return string.Format("Port:{0}    Name:{1}   Message:{2}",Port,Name,Message);
        }
        #endregion
    }
}
