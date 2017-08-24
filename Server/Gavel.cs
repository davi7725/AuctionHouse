using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Server
{
    class Gavel
    {
        private List<StreamWriter> clientsStreams = new List<StreamWriter>();
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
        public void AddClientStream(StreamWriter sw)
        {
            lock (clientsLock)
            {
                clientsStreams.Add(sw);
            }
        }
        public void RemoveClientStream(StreamWriter sw)
        {
            lock (clientsLock)
            {
                clientsStreams.Remove(sw);
            }
        }

        private void Broadcast(string message)
        {
            lock (clientsLock)
            {
                clientsStreams.ForEach(stream => {
                    try
                    {
                        stream.WriteLine(message);
                    }
                    catch (IOException)
                    {   
                    }
                });
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
    }
}
