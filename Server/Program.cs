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
        private Mutex mutex = new Mutex();
        private List<ItemAuction> auctions = new List<ItemAuction>();
        private List<string> names = new List<string>();

        private void Run()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 1234);
            serverSocket.Start();

            for (int i = 0; i < 10; i++)
            {
                auctions.Add(new ItemAuction(new Item("Item number " + (i + 1), (i + 1) * 100)));
            }

            while (true)
            {
                Socket clientSocket = serverSocket.AcceptSocket();
                new Thread(new ThreadStart(() => ClientThread(InitializeClient(clientSocket)))).Start();
            }
        }

        private Client InitializeClient(Socket clientSocket)
        {
            try
            {
                NetworkStream networkStream = new NetworkStream(clientSocket);
                StreamWriter sw = new StreamWriter(networkStream);
                sw.AutoFlush = true;
                StreamReader sr = new StreamReader(networkStream);

                string name = null;
                sw.WriteLine("Choose a username!");
                do
                {
                    if (name != null)
                    {
                        sw.WriteLine("Name already taken!");
                    }
                    name = sr.ReadLine();
                } while (names.Contains(name));
                names.Add(name);
                sw.WriteLine("Name ok.");
                return new Client(clientSocket, sw, sr, name);
            }
            catch (IOException)
            {
                return null;
            }
        }

        private ItemAuction AuctionSelection(Client c)
        {
            c.Send("Choose an item.");
            foreach (ItemAuction itemAuction in auctions)
            {
                if (!itemAuction.IsBidFinished())
                {
                    c.Send(itemAuction.ID + " - Name: " + itemAuction.Item.Name + " - Current price: " + itemAuction.Item.GetPrice());
                }
            }

            ItemAuction ia = null;
            bool firstTime = true;
            do
            {
                if (!firstTime)
                {
                    c.Send("Invalid input.");
                }
                else
                {
                    firstTime = false;
                }

                try
                {
                    int id = int.Parse(c.Receive());
                    ia = GetAuctionFromID(id);
                }
                catch (FormatException)
                {
                }
            } while (ia == null || ia.IsBidFinished());

            return ia;
        }

        private ItemAuction GetAuctionFromID(int id)
        {
            foreach (ItemAuction ia in auctions)
            {
                if (ia.ID == id)
                {
                    return ia;
                }
            }
            return null;
        }

        private void ClientThread(Client client)
        {
            if (client != null)
            {
                ItemAuction chosenAuction = null;
                try
                {
                    while (true)
                    {
                        chosenAuction = AuctionSelection(client);
                        chosenAuction.PartecipateAuction(client);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentNullException || ex is IOException)
                    Console.WriteLine(client.Name + " disconnected");
                    if (chosenAuction != null)
                    {
                        chosenAuction.RemoveClient(client);
                    }
                    names.Remove(client.Name);
                    client.Dispose();
                }
            }
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}
