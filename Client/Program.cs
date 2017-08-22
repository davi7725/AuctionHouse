using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient server = new TcpClient("127.0.0.1", 1234);

            NetworkStream stream = server.GetStream();
            StreamWriter writer = new StreamWriter(stream);
            StreamReader reader = new StreamReader(stream);
            writer.AutoFlush = true;

            Thread printer = new Thread(printServerMessages);
            printer.Start(reader);

            while (true)
            {
                writer.WriteLine(Console.ReadLine());
            }
        }

        static private void printServerMessages(object sr)
        {
            StreamReader reader = (StreamReader)sr;
            while (true)
                Console.WriteLine(reader.ReadLine());
        }
    }
}
