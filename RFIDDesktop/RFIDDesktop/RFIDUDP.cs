using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Xml;
using Sitacomm;
using System.Net;
using System.Net.Sockets;
namespace RFIDDesktop
{
    public class RFIDUDP
    {
        protected string[] m_cientes = null;
        protected UdpClient m_udpsvr;
        protected IPEndPoint[] m_sender;
        protected string m_com;
        protected int m_udpPort;
        protected bool m_valido = false;
        protected RFID105Reader m_RFID;

        public bool Valido
        {
            get
            {
                return m_valido;
            }
        }
        
        public bool InicializarUDP()
        {
            bool ret = false;
            try
            {
                
                m_udpsvr = new UdpClient();

                m_sender = new IPEndPoint[m_cientes.Length];
                for (int i = 0; i< m_cientes.Length; i++)
                {
                    IPAddress serverAddr = IPAddress.Parse(m_cientes[i]);
                    m_sender[i] = new IPEndPoint(serverAddr, m_udpPort);
                }
                ret = true;
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
            return ret;
        }

        public RFIDUDP()
        {
            try
            {
                if (AbrirXml())
                {
                    m_RFID = new RFID105Reader();
                    m_RFID.OnCodeDetected += EPCLeido;
                    m_valido = true;
                    InicializarUDP();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
            
        }
        public void IniciarRFID()
        {
            Console.WriteLine("MENSAJE: Iniciando Servicio");
            m_RFID.IniciarEscaneo();
        }
        public void TerminarRFID()
        {
            Console.WriteLine("MENSAJE: Terminando Servicio");
            m_RFID.DetenerEscaneo();            
        }
        public bool AbrirXml()
        {
            bool ret = false;
            try
            {
                if (File.Exists("config.xml"))
                {

                    System.Console.WriteLine("Leyendo config.xml");

                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load("config.xml");

                    XmlNodeList lista = xdoc.GetElementsByTagName("udp");
                    if (lista.Count == 0)
                    {
                        Console.WriteLine("ERROR: No existe el tag udp. Terminando");
                        return ret;
                    }
                    XmlElement e= (XmlElement)lista[0];
                    if (!int.TryParse(e.InnerText, out m_udpPort))
                    {
                        Console.WriteLine("ERROR: No existe el tag udp. Terminando");
                        return ret;
                    }


                    lista = xdoc.GetElementsByTagName("cliente");
                    if (lista.Count == 0)
                    {
                        Console.WriteLine("ERROR: No existen clientes. Terminando");
                        return ret;
                    }
                    m_cientes = new string[lista.Count];
                    for(int i =0; i< lista.Count; i++) 
                    {
                        e = (XmlElement)lista[i];
                        m_cientes[i] = e.InnerText;
                    }

                    lista = xdoc.GetElementsByTagName("rfid");
                    if (lista.Count == 0)
                    {
                        Console.WriteLine("ERROR: No existe el tag rfid. Terminando");
                        return ret;
                    }
                    e = (XmlElement)lista[0];
                    m_com = e.InnerText;
                    ret = true;
                }
            }
            catch(Exception e)
            {

                ret = false;
            }

            return ret;
        }

        public void EPCLeido(object codigo)
        {
            string EPC = (string)codigo;

            Console.WriteLine("MENSAJE: Código leído:" + EPC);
            byte[] data = Encoding.ASCII.GetBytes(EPC);

            foreach(IPEndPoint ep in m_sender)
            {
                m_udpsvr.Send(data, data.Length, ep);
                Console.WriteLine("MENSAJE: Enviando código:" + EPC);
            }

        }

    }

}
