using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Xml;
namespace Asistencia
{
    class Program
    {
        static int m_sala;
        static int[] m_tipoRFID = new int[2];
        static string[] m_direccion = new string[2];
        static string m_bienvenida;


        static void Main(string[] args)
        {
            //0 fuera       Entrada
            //1 dentro      Salida
            //tipo 0 impinj, 1 oppiot
            LeerXML();    
            Asistencia asistencia = new Asistencia(1, m_direccion, m_tipoRFID, "localhost", "eventosregistro", m_bienvenida);
            asistencia.Iniciar();
            try
            {
                while (true)
                {

                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: "+ DateTime.Now.ToString() + ", " + e.Message);
            }
            finally
            {
                asistencia.Terminar();
            }

        }
        static void LeerXML()
        {
            try
            {
                if (File.Exists("config.xml"))
                {

                    Console.WriteLine("Leyendo config.xml");

                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load("config.xml");

                    XmlNodeList lista = xdoc.GetElementsByTagName("sala");

                    XmlElement e = (XmlElement)lista[0];

                    m_sala = int.Parse(e.InnerText);

                    lista = xdoc.GetElementsByTagName("rfid");

                    e = (XmlElement)lista[0];
                    int ubicacion = int.Parse(e.GetAttribute("ubicacion"));
                    m_tipoRFID[ubicacion] = int.Parse(e.GetAttribute("modelo"));
                    m_direccion[ubicacion] = e.InnerText;

                    e = (XmlElement)lista[1];
                    ubicacion = int.Parse(e.GetAttribute("ubicacion"));
                    m_tipoRFID[ubicacion] = int.Parse(e.GetAttribute("modelo"));
                    m_direccion[ubicacion] = e.InnerText;

                    lista = xdoc.GetElementsByTagName("bienvenida");
                    e = (XmlElement)lista[0];
                    m_bienvenida = e.InnerText;
                }
            }
            catch (Exception e)
            {

                
            }


        }
    }
}
