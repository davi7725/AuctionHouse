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
        private List<StreamWriter> clientsStreams;
        private Timer timer1 = new Timer(5000);
        private Timer timer2 = new Timer(8000);
        private Timer timer3 = new Timer(10000);

        private bool bidFinished = false;
        private string bidder;
        private int price;
        private object timerLock = new object();
        private bool stopped = true;

        public Gavel(List<StreamWriter> clientsStreams)
        {
            this.clientsStreams = clientsStreams;
            timer1.AutoReset = false;
            timer2.AutoReset = false;
            timer3.AutoReset = false;

            timer1.Elapsed += (sender, e) =>
            {
                clientsStreams.ForEach(stream => stream.WriteLine("First!"));
            };
            timer2.Elapsed += (sender, e) =>
            {
                clientsStreams.ForEach(stream => stream.WriteLine("Second!"));
            };
            timer3.Elapsed += (sender, e) =>
            {
                lock (timerLock) { 
                    if (!timer1.Enabled && !stopped) {
                        clientsStreams.ForEach(stream => stream.WriteLine("Third! Sold to {0} for {1}!", bidder, price));
                        bidFinished = true;
                    }
                }
            };
        }

        public bool Stop()
        {
            bool stopOk = false;
            lock (timerLock) {
                if (!bidFinished) {
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
    }
}
