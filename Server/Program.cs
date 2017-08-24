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
            while (true)
            {
                Socket clientSocket = serverSocket.AcceptSocket();
                new Thread(new ThreadStart(() => ClientThread(InitializeClient(clientSocket)))).Start();
            }
        }

        private Client InitializeClient(Socket clientSocket)
        {
            NetworkStream networkStream = new NetworkStream(clientSocket);
            StreamWriter sw = new StreamWriter(networkStream);
            sw.AutoFlush = true;
            StreamReader sr = new StreamReader(networkStream);

            string name = null;
            do
            {
                if (name != null)
                {
                    sw.WriteLine("Name already taken!");
                }
                name = sr.ReadLine();
            } while (names.Contains(name));
            sw.WriteLine("Name ok.");
            return new Client(clientSocket, sw, sr, name);
        }

        private ItemAuction AuctionSelection(Client c)
        {
            c.StreamWriter.WriteLine("Choose an item.");
            foreach (ItemAuction itemAuction in auctions)
            {
                c.StreamWriter.WriteLine("{0} - Name: {1} - Current price: {2}", itemAuction.ID, itemAuction.Item.Name, itemAuction.Item.GetPrice());
            }

            ItemAuction ia = null;
            bool firstTime = true;
            do
            {
                if (!firstTime)
                {
                    c.StreamWriter.WriteLine("Invalid input.");
                }
                else
                {
                    firstTime = false;
                }

                try
                {
                    int id = int.Parse(c.StreamReader.ReadLine());
                    ia = GetAuctionFromID(id);
                }
                catch (FormatException)
                {
                }
            } while (ia == null);

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
            ItemAuction chosenAuction = null;
            try
            {
                while (true)
                {
                    chosenAuction = AuctionSelection(client);
                    chosenAuction.PartecipateAuction(client);
                }
            }
            catch (IOException)
            {
                if (chosenAuction != null)
                {
                    chosenAuction.RemoveClient(client);
                }
                names.Remove(client.Name);
                client.Dispose();
            }
        }

        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}
