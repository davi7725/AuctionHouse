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
            TcpClient server = new TcpClient("localhost", 11000);

            NetworkStream stream = server.GetStream();
            StreamWriter writer = new StreamWriter(stream);
            StreamReader reader = new StreamReader(stream);

            Thread printer = new Thread(printServerMessages);
            printer.Start(reader);

            while (true)
            {
                writer.WriteLine(Console.ReadLine());
                writer.Flush();
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
