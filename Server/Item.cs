using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Item
    {
        public string Name { get; private set; }

        private int price;
        private object lockPrice = new object();

        public Item(string name, int price = 0)
        {
            this.Name = name;
            this.price = price;
        }

        public int GetPrice()
        {
            int tmp;
            lock (lockPrice)
            {
                tmp = price;
            }
            return tmp;
        }
        public bool UpdatePrice(int newPrice)
        {
            bool changed = false;
            lock (lockPrice)
            {
                if (newPrice > price)
                {
                    price = newPrice;
                    changed = true;
                }
            }
            return changed;
        }

        public override bool Equals(object obj)
        {
            if (obj is Item)
            {
                return this.Name == ((Item)obj).Name;
            }
            return false;
        }
    }
}
