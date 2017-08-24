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
            AddClient(c);
            c.StreamWriter.WriteLine("Item on sale: " + Item.Name + "\tItem price: " + Item.GetPrice());
            bool repeat = true;
            c.StreamWriter.WriteLine("Type quit to quit");
            while (repeat)
            {
                string message = c.StreamReader.ReadLine();
                if (message != "quit")
                {
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
                                Utils.Broadcast("Highest bid now " + biddingValue, clients);
                                gavel.Start(c.Name, biddingValue);
                            }
                            else
                            {
                                c.StreamWriter.WriteLine("The auction is finished!");
                            }
                        }
                        else
                        {
                            if (gavel.IsFinished())
                            {
                                c.StreamWriter.WriteLine("The auction is finished!");
                            }
                            else
                            {
                                c.StreamWriter.WriteLine("Your bid is too low!");
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
                else
                {
                    repeat = false;
                    RemoveClient(c);
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

        public bool IsBidFinished()
        {
            return gavel.IsFinished();
        }
    }
}