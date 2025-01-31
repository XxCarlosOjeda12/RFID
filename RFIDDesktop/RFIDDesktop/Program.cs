using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Threading;
namespace RFIDDesktop
{
    class Program
    {
        static void Main(string[] args)
        {
            RFIDUDP srv = new RFIDUDP();
            if (srv.Valido)
            {
                srv.IniciarRFID();
                while (true)
                {
                    Thread.Sleep(500);
                }
            }
        }
        
    }
}
