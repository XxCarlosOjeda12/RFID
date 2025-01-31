using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitacomm;
using Sitacomm.RFID;
using System.Threading;
using Impinj.OctaneSdk;
using System.Net;
using System.Net.Sockets;
namespace Asistencia
{
    public class Asistencia
    {
        protected Mutex m_mutex = new Mutex();
        protected int m_sala;
        protected RFIDReader[] m_RFIDReader;
        protected string[] m_direccion;
        protected int[] m_tipoRFID;
        protected const int m_dentro = 1;
        protected const int m_fuera = 0;
        protected RegistroDB m_db;
        public delegate void OnDevice(int i);
        //public event OnDevice EventoDispositivo;
        protected UdpClient m_udpsvr;
        protected IPEndPoint m_sender;
        protected string m_client;
        protected int m_udpPort = 5874;


        //0 fuera       Entrada
        //1 dentro      Salida

        //tipo 0 impinj, 1 oppiot

        public Asistencia(int sala, string[] direccion, int[] tipo, string hostDB, string bd, string client)
        {
            m_client = client;
            m_sala = sala;
            m_db = new RegistroDB(hostDB, bd);
            m_direccion = (string[])direccion.Clone();
            m_tipoRFID = (int[])tipo.Clone();

            m_RFIDReader = new RFIDReader[2];
            for (int i = 0; i < 2; i++)
            {
                if (m_tipoRFID[i] == 0)
                {
                    m_RFIDReader[i] = new RFIDImpijnReader(m_direccion[i], 28);
                    //                    m_RFIDReader[i].KeepAliveReceived += OnKeepAlive;
                }
                else
                {
                    m_RFIDReader[i] = new RFIDOppiotReader(m_direccion[i], 30);
                }
            }

            m_RFIDReader[0].TagsReported += OnTagEntrada;
            m_RFIDReader[1].TagsReported += OnTagSalida;
            InicializarUDP();

        }


        public bool InicializarUDP()
        {
            bool ret = false;
            try
            {

                m_udpsvr = new UdpClient();

                IPAddress serverAddr = IPAddress.Parse(m_client);
                m_sender = new IPEndPoint(serverAddr, m_udpPort);
                ret = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
            return ret;
        }



        protected List<EPC> ObtenerEPC(RFIDTagReport report, string tipo)
        {
            List<EPC> lista = new List<EPC>();
            if (tipo == "Impijn")
            {
                TagReport tr = ((RFIDImpijnTagReport)report).Report;
                foreach (Tag tag in tr)
                {
                    lista.Add(new EPC { epc = tag.Epc.ToString().Replace(" ", string.Empty) });
                }
            }
            else if (tipo == "OPPIOT")
            {
                List<string> tr = ((RFIDOppiotTagReport)report).Report;

                foreach (string tag in tr)
                {

                    lista.Add(new EPC { epc = tag });

                }
            }
            return lista;
        }

        void Bienvenida(RFIDTagReport report, string tipo)
        {
            List<string> b = m_db.Bienvenida(report, tipo);

            foreach (string s in b)
            {
                byte[] data = Encoding.UTF8.GetBytes(s);
                m_udpsvr.Send(data, data.Length, m_sender);
                m_db.MarcarBienvenida(s);
            }
            

        }
        void OnTagEntrada(RFIDReader reader, RFIDTagReport report)
        {
            List<EPC> lista = null;
            m_mutex.WaitOne();
            try
            {
                lista = m_db.Entrada(report, m_sala, reader.Tipo);

            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now.ToString() + "|LECTORENTRADA|ERROR|" + e.Message);
            }

            m_mutex.ReleaseMutex();
            if (lista != null)
            {
                foreach (EPC e in lista)
                {
                    if (e.ActualizaEstado)
                        Console.WriteLine(DateTime.Now.ToString() + "|LECTORENTRADA|FUERA|" + e.epc);
                }
            }
            Bienvenida(report, reader.Tipo);
        }
        void OnTagSalida(RFIDReader reader, RFIDTagReport report)
        {
           List<EPC> lista = null;
            m_mutex.WaitOne();
            try
            {
                lista = m_db.Salida(report, m_sala, reader.Tipo);
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now.ToString() + "|LECTORSALIDA|ERROR|" + e.Message);
            }
            m_mutex.ReleaseMutex();
            if (lista != null)
            {
                foreach (EPC e in lista)
                {
                    if (e.ActualizaEstado)
                        Console.WriteLine(DateTime.Now.ToString() + "|LECTORSALIDA|DENTRO|" + e.epc);
                }
            }
        }
            

        public void Iniciar()
        {
            try
            {
                m_RFIDReader[1].Connect();
                m_RFIDReader[1].Start();

                m_RFIDReader[0].Connect();
                
                m_RFIDReader[0].Start();

            }catch(Exception e)
            {
                Console.WriteLine("Error Conexion: " + e.Message);
            }
        }

        public void Terminar()
        {
            try
            {
                m_RFIDReader[0].Stop();
                m_RFIDReader[0].Disconnect();
                m_RFIDReader[1].Stop();
                m_RFIDReader[0].Disconnect();       
            }
            catch (Exception e)
            {

            }
        }

        public void OnKeepAlive()
        {
            System.Console.WriteLine("Impjn: Keep Alive");
        }
 /*       public void OnTagAddedSalida(RFIDTag tag, RFIDTag[] tags)
        {
            
            if (m_db.Salida(tag.EPC, m_sala))
                System.Console.WriteLine("Entrando:\t\t" + DateTime.Now.ToString() + " " + tag.EPC);
        }

        public void OnTagAddedEntrada(RFIDTag tag, RFIDTag[] tags)
        {
            if (m_db.Entrada(tag.EPC, m_sala))
                System.Console.WriteLine("Saliendo:\t"+ DateTime.Now.ToString() + " " + tag.EPC);
        }
        */
    }

}
