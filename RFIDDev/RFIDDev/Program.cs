using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Impinj.OctaneSdk;
using Sitacomm.RFID;
namespace RFIDDev
{
    class Program
    {
        static void Main(string[] args)
        {
            //RFIDOppiotReader reader = new RFIDOppiotReader("10.1.0.51", 30);
            RFIDImpijnReader reader = new RFIDImpijnReader("192.168.1.82", 28.5);
            reader.TagsReported += Reader_TagsReported;


            reader.Connect();
            reader.Start();

         /*   reader2.TagsReported += Reader_TagsReported;



            reader2.Connect();
            reader2.Start();
            */
            Console.Write("Presionar Enter para terminar");
            Console.ReadLine();
            reader.Stop();
            //reader2.Stop();
        }
        /*
         OPPIOT
        private static void Reader_TagsReported(RFIDReader reader, RFIDTagReport report)
        {
            RFIDImpijnTagReport r = (RFIDImpijnTagReport)report;
            foreach (string epc in r.Report)
            {
                Console.WriteLine(DateTime.Now.ToString() + ": " + epc);
            }
        }
        */


        /*
        IMPINJ

        private static void Reader_TagsReported(RFIDReader reader, RFIDTagReport report)
        {
            //RFIDOppiotTagReport r = (RFIDOppiotTagReport)report;
            RFIDImpijnTagReport r = (RFIDImpijnTagReport)report;
            foreach (var tag in r.Report.Tags)
            {
                //Console.WriteLine(DateTime.Now.ToString() + ": " + epc);
                // Accede al EPC y conviértelo a un string legible
                string epc = tag.Epc.ToHexString();
                Console.WriteLine($"{DateTime.Now}: {epc}");

            }
        }

        */

    }
}
