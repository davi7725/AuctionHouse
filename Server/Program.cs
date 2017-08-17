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
        private static System.Timers.Timer gavel = new System.Timers.Timer();
        private string highestBidder;

        private void Run()
        {
            gavel.Elapsed += (source, e) =>
            {
                Broadcast("Bam");
            };
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

            streamWriter.WriteLine("Item on sale: " + item.Name + "\tItem price: " + item.Price);
            while (true)
            {
                string message = streamReader.ReadLine();
                try
                {
                    int biddingValue = int.Parse(message);
                    if (item.UpdatePrice(biddingValue))
                    {
                        streamWriter.WriteLine("Bid ok");
                        Broadcast("Highest bid now " + biddingValue);
                        SuccessfulBid(clientSocket.LocalEndPoint.ToString());
                    }
                    else
                    {
                        streamWriter.WriteLine("Bid failed");
                    }
                }
                catch (Exception e)
                {
                    streamWriter.WriteLine("Incorrect bid format");
                }
            }
        }

        private void Broadcast(string message)
        {
            streams.ForEach((stream) => stream.WriteLine(message));
        }

        private void SuccessfulBid(string clientName)
        {
            gavel.Interval = 10000;
            gavel.Enabled = true;
            highestBidder = clientName;
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}
