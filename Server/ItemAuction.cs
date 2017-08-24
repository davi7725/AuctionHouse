using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server
{
    class ItemAuction
    {
        static int currID = 0;
        public Item Item { get; private set; }
        private Gavel gavel;
        public int ID { get; private set; }
        private List<Client> clients = new List<Client>();
        private object clientsLock = new object();
        private Mutex mutex = new Mutex();

        public ItemAuction(Item item)
        {
            ID = currID++;
            Item = item;
            gavel = new Gavel();
            //new Thread(new ThreadStart(() => AuctionStart()));
        }

        public void PartecipateAuction(Client c)
        {
            //TODO go back to selection
            AddClient(c);
            c.StreamWriter.WriteLine("Item on sale: " + Item.Name + "\tItem price: " + Item.GetPrice());
            while (true)
            {
                string message = c.StreamReader.ReadLine();
                try
                {

                    int biddingValue = int.Parse(message);

                    mutex.WaitOne();
                    if (Item.GetPrice() < biddingValue)
                    {
                        if (gavel.Stop())
                        {
                            Item.UpdatePrice(biddingValue);
                            c.StreamWriter.WriteLine("Bid ok");
                            Utils.Broadcast("Highest bid now " + biddingValue, (from client in clients select c.StreamWriter).ToList());
                            gavel.Start(c.Name, biddingValue);
                        }
                    }
                    mutex.ReleaseMutex();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    c.StreamWriter.WriteLine("Incorrect bid format");
                }
            }
        }
        private void AddClient(Client c)
        {
            lock (clientsLock)
            {
                clients.Add(c);
                gavel.AddClientStream(c.StreamWriter);
            }
        }

        public void RemoveClient(Client c)
        {
            lock (clientsLock)
            {
                clients.Remove(c);
                gavel.RemoveClientStream(c.StreamWriter);
            }
        }
    }
}