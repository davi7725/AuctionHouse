using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Client
    {
        private Socket socket;
        private StreamWriter sw;
        private StreamReader sr;
        private object myLock = new object();
        public string Name { get; private set; }

        public Client(Socket clientSocket, StreamWriter sw, StreamReader sr, string name)
        {
            socket = clientSocket;
            this.sw = sw;
            this.sr = sr;
            this.Name = name;
        }

        public void Dispose()
        {
            sw.Close();
            sr.Close();
            socket.Close();
        }

        public void Send(string msg)
        {
            lock (myLock)
            {
                sw.WriteLine(msg);
            }
        }

        public string Receive()
        {
            return sr.ReadLine();
        }
    }
}
