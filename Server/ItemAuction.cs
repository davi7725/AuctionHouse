using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server
{
    class ItemAuction
    {
        public delegate void Message(string value);
        static int currID = 0;
        public Item Item { get; private set; }
        private Gavel gavel;
        public int ID { get; private set; }
        private List<Client> clients = new List<Client>();
        private object clientsLock = new object();
        private Mutex mutex = new Mutex();
        private event Message broadcast;
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
            c.Send("Item on sale: " + Item.Name + "\tItem price: " + Item.GetPrice() + "\tStarting price was: " + Item.StartingPrice);
            bool repeat = true;
            c.Send("Type quit to quit");
            while (repeat)
            {
                string message = c.Receive();
                if (message != "quit")
                {
                    try
                    {

                        int biddingValue = int.Parse(message);

                        mutex.WaitOne();
                        if (Item.GetPrice() < biddingValue)
                        {
                            if (gavel.Reset(c.Name, biddingValue))
                            {
                                Item.UpdatePrice(biddingValue);
                                c.Send("Bid ok");
                                broadcast("Highest bid now " + biddingValue);
                            }
                            else
                            {
                                c.Send("The auction is finished!");
                            }
                        }
                        else
                        {
                            if (gavel.IsFinished)
                            {
                                c.Send("The auction is finished!");
                            }
                            else
                            {
                                c.Send("Your bid is too low!");
                            }
                        }
                        mutex.ReleaseMutex();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        c.Send("Incorrect bid format");
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
                gavel.AddClient(c.Send);
                broadcast += c.Send;
            }
        }

        public void RemoveClient(Client c)
        {
            lock (clientsLock)
            {
                clients.Remove(c);
                gavel.RemoveClient(c.Send);
                broadcast -= c.Send;
            }
        }

        public bool IsBidFinished()
        {
            return gavel.IsFinished;
        }
    }
}