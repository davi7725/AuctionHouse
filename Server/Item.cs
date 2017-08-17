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
        public int Price { get; private set; }
        private object lockPrice = new object();

        public Item(string name, int price = 0)
        {
            this.Name = name;
            this.Price = price;
        }

        public bool UpdatePrice(int newPrice)
        {
            bool changed = false;
            lock (lockPrice)
            {
                if (newPrice > Price)
                {
                    Price = newPrice;
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
