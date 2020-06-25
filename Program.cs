using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TIPServer
{
    public  class Server
    {
        private const int PortNumber = 37000;

        public static void Main(string[] args)
        {
            TCPServer tcpServer = new TCPServer(PortNumber);
            tcpServer.AcceptClients();
        }
    }
}
