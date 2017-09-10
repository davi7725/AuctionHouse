using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using static Server.ItemAuction;

namespace Server
{
    class Gavel
    {
        private static readonly int SLEEP_FIRST = 5000;
        private static readonly int SLEEP_SECOND = 3000;
        private static readonly int SLEEP_THIRD = 2000;

        private event Message BroadcastMethods;

        private bool bidFinished = false;
        private string bidder;
        private int price;
        private object bidLock = new object();
        private object clientsLock = new object();
        private Thread currGavel;

        public bool IsFinished
        {
            get
            {
                lock (bidLock)
                {
                    return bidFinished;
                }
            }
        }
        public bool Reset(string bidder, int price)
        {
            lock (bidLock)
            {
                if (!bidFinished)
                {
                    this.bidder = bidder;
                    this.price = price;
                    if (currGavel != null)
                    {
                        currGavel.Abort();
                    }
                    currGavel = new Thread(Run);
                    currGavel.Start();
                    return true;
                }
                return false;
            }
        }

        private void Run()
        {
            try
            {
                Thread.Sleep(SLEEP_FIRST);
                Broadcast("First!");
                Thread.Sleep(SLEEP_SECOND);
                Broadcast("Second!");
                Thread.Sleep(SLEEP_THIRD);
                lock (bidLock)
                {
                    Broadcast("Third! Sold to " + bidder + " for " + price + "!");
                    bidFinished = true;
                }
            }
            catch (ThreadAbortException)
            {
            }
        }


        public void AddClient(Message sendMethod)
        {
            lock (clientsLock)
            {
                BroadcastMethods += sendMethod;
            }
        }
        public void RemoveClient(Message sendMethod)
        {
            lock (clientsLock)
            {
                BroadcastMethods -= sendMethod;
            }
        }

        private void Broadcast(string message)
        {
            lock (clientsLock)
            {
                BroadcastMethods?.Invoke(message);
            }
        }
        /*
        //private List<StreamWriter> clientsStreams = new List<StreamWriter>();
        private event Message BroadcastMethods;
        private Timer timer1 = new Timer(5000);
        private Timer timer2 = new Timer(8000);
        private Timer timer3 = new Timer(10000);

        private bool bidFinished = false;
        private string bidder;
        private int price;
        private object timerLock = new object();
        private object clientsLock = new object();
        private bool stopped = true;

        public Gavel()
        {
            timer1.AutoReset = false;
            timer2.AutoReset = false;
            timer3.AutoReset = false;

            timer1.Elapsed += (sender, e) =>
            {
                Broadcast("First!");
            };
            timer2.Elapsed += (sender, e) =>
            {
                Broadcast("Second!");
            };
            timer3.Elapsed += (sender, e) =>
            {
                lock (timerLock)
                {
                    if (!timer1.Enabled && !stopped)
                    {
                        Broadcast("Third! Sold to " + bidder + " for " + price +"!");
                        bidFinished = true;
                    }
                }
            };
        }

        public bool Stop()
        {
            bool stopOk = false;
            lock (timerLock)
            {
                if (!bidFinished)
                {
                    timer1.Stop();
                    timer2.Stop();
                    timer3.Stop();
                    stopOk = true;
                    stopped = true;
                }
            }
            return stopOk;
        }

        public void Start(string bidder, int price)
        {
            lock (timerLock)
            {
                this.bidder = bidder;
                this.price = price;
                timer1.Start();
                timer2.Start();
                timer3.Start();
                stopped = false;
            }
        }
        public void AddClientStream(Message sendMethod)
        {
            lock (clientsLock)
            {
                //clientsStreams.Add(sw);
                BroadcastMethods += sendMethod;
            }
        }
        public void RemoveClientStream(Message sendMethod)
        {
            lock (clientsLock)
            {
                //clientsStreams.Remove(sw);
                BroadcastMethods -= sendMethod;
            }
        }

        private void Broadcast(string message)
        {
            lock (clientsLock)
            {
                BroadcastMethods(message);
            }
        }

        public bool IsFinished()
        {
            bool res;
            lock (timerLock)
            {
                res = bidFinished;
            }
            return res;
        }
        */
    }
}