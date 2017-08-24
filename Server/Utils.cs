using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    static class Utils
    {
        public static void Broadcast(string message, ICollection<StreamWriter> streams)
        {
            foreach (StreamWriter stream in streams)
            {
                stream.WriteLine(message);
            }
        }
    }
}
