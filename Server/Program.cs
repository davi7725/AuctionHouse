using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Server
{
    class Program
    {
        private List<StreamWriter> streams = new List<StreamWriter>();
        private Item item = new Item("Item", 123);
        private Gavel gavel;
        private Mutex mutex = new Mutex();

        private void Run()
        {
            gavel = new Gavel(streams);
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 1234);
            serverSocket.Start();
            while (true) { 
                Socket clientSocket = serverSocket.AcceptSocket();
                new Thread(new ThreadStart(() => ClientThread(clientSocket))).Start();
            }
        }

        private void ClientThread(Socket clientSocket)
        {
            NetworkStream networkStream = new NetworkStream(clientSocket);
            StreamWriter streamWriter = new StreamWriter(networkStream);
            streamWriter.AutoFlush = true;
            StreamReader streamReader = new StreamReader(networkStream);
            streams.Add(streamWriter);

            streamWriter.WriteLine("Item on sale: " + item.Name + "\tItem price: " + item.GetPrice());
            while (true)
            {
                string message = streamReader.ReadLine();
                try
                {

                    int biddingValue = int.Parse(message);

                    mutex.WaitOne();
                    if (item.GetPrice() < biddingValue)
                    {
                        if (gavel.Stop())
                        {
                            item.UpdatePrice(biddingValue);
                            streamWriter.WriteLine("Bid ok");
                            Broadcast("Highest bid now " + biddingValue);
                            gavel.Start(clientSocket.LocalEndPoint.ToString(), biddingValue);
                        }
                    }
                    mutex.ReleaseMutex();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    streamWriter.WriteLine("Incorrect bid format");
                }
            }
        }

        private void Broadcast(string message)
        {
            streams.ForEach((stream) => stream.WriteLine(message));
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}
