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
            TcpClient server = new TcpClient("localhost", 1234);

            NetworkStream stream = server.GetStream();
            StreamWriter writer = new StreamWriter(stream);
            StreamReader reader = new StreamReader(stream);
            writer.AutoFlush = true;

            Thread printer = new Thread(PrintServerMessages);
            printer.Start(reader);
            try
            {
                while (true)
                {
                    writer.WriteLine(Console.ReadLine());
                }
            }
            catch
            {
            }
        }

        static private void PrintServerMessages(object sr)
        {
            StreamReader reader = (StreamReader)sr;
            try
            {
                while (true)
                {
                    Console.WriteLine(reader.ReadLine());
                }
            }
            catch
            {
                Console.WriteLine("Server error!!!!");
            }
        }
    }
}
