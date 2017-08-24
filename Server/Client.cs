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
        public StreamWriter StreamWriter { get; private set; }
        public StreamReader StreamReader { get; private set; }
        public string Name { get; private set; }

        public Client(Socket clientSocket, StreamWriter sw, StreamReader sr, string name)
        { 
            socket = clientSocket;
            StreamWriter = sw;
            StreamReader = sr;
            this.Name = name;
        }

        public void Dispose()
        {
            StreamWriter.Close();
            StreamReader.Close();
            socket.Close();
        } 
    }
}
