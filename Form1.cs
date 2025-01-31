using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Sitacom.RFID;
using Sitacomm;
namespace Bienvenida
{
    public partial class Form1 : Form
    {
        protected int m_sleep0 = 2200;
        protected int m_sleep = 2200;
        protected UdpClient m_udp;
        protected int m_port = 5874;

        protected List<string> m_EPC = new List<string>();
//        protected RFIDImpijnQueu m_rfid;
        protected string m_DBHost;
        protected string m_Database;
        protected string m_RFIDModel;
        protected string m_RFIDReaderHost;
        protected bool m_iniciado = true;
        protected bool m_demo = false;
        protected int m_indiceConfirmacion = 0;
        protected bool m_BcolorFuente = false;
        protected bool m_BcolorFondoFuente = false;
        protected Color m_ColorFondoFuente;
        protected Color m_ColorFuente;
        protected string m_ArchivoImagenFondo = "";
        protected string m_ArchivoFuenteRegular = "";
        protected int m_tamanoFuente = 0;
        protected string m_ArchivoFuenteNegrita = "";
        protected int m_EspacioInformacion = 1;
        protected string m_ArchivoConfiguracion = "config.xml";
        protected Point m_Pimagen = new Point(10, 10);
        protected Size m_Simagen = new Size(10, 20);
        protected Point m_Pinformacion = new Point(10, 10);        
        protected List<string> m_informacion = new List<string>();
        protected List<string> m_etiquetas = new List<string>();
        protected string m_directorioRetratos = "";
        protected int m_TamanoFuente = 10;
        protected Label[] m_parametros;
  //      protected RegistroDB m_db;
        protected Thread m_hilo = null;
        protected Thread m_hiloMostrar = null;
        protected string m_dirFotografias=null;
        protected Mutex m_mutex = new Mutex();
        public Form1(bool demo)
        {
            m_demo = demo;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!CargarXML())
            {
                BeginInvoke(new MethodInvoker(Close));
            }
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, m_port);
            m_udp = new UdpClient(ipep);

            //            m_db = new RegistroDB(m_DBHost,m_Database);
            BackgroundImage = Image.FromFile(m_ArchivoImagenFondo);

            //fuentes
            System.Drawing.Text.PrivateFontCollection privateFonts = new System.Drawing.Text.PrivateFontCollection();
            privateFonts.AddFontFile(m_ArchivoFuenteRegular);
            privateFonts.AddFontFile(m_ArchivoFuenteNegrita);
            Font font = new Font(privateFonts.Families[0], m_tamanoFuente);
            Font fontN = new Font(privateFonts.Families[1], m_tamanoFuente);

            DoubleBuffered = true;
            if (m_informacion.Count != 0)
            {
                m_parametros = new Label[m_informacion.Count];
                for(int i =0; i< m_parametros.Length; i++)
                {
                    m_parametros[i] = new Label();
                    if(m_BcolorFondoFuente && m_BcolorFuente)
                        m_parametros[i].Font = (i==0)?fontN:font;
                    m_parametros[i].BackColor = m_ColorFondoFuente;
                    m_parametros[i].ForeColor = m_ColorFuente;
                   // m_parametros[i].Dock = DockStyle.Fill;
                    m_parametros[i].Width = 1100;
                    m_parametros[i].Height = 100;
                    m_parametros[i].TextAlign = ContentAlignment.MiddleCenter;
                    m_parametros[i].AutoSize = false;
                    //m_parametros[i].Text = "María de Montserrat Gonzalez".ToUpper();                    
                    Controls.Add(m_parametros[i]);
                    m_parametros[i].Location = new Point(m_Pinformacion.X, m_Pinformacion.Y + i * m_EspacioInformacion);
                    m_parametros[i].Visible = false;
                    //m_parametros[i].Visible = true;
                }
            }
            else
                m_parametros = null;



            m_retrato.Size = m_Simagen;
            m_retrato.Location = m_Pimagen;
            m_retrato.Visible = false;
            //m_retrato.Image = Image.FromFile(m_directorioRetratos + "/SMT0200.png");

            //Prueba
            //            MostrarInformacion("2");
            if (m_demo)
            {
                m_hilo = new Thread(new ThreadStart(HiloDemo));
                m_hilo.Start();
            }
            else
            {
                m_hilo = new Thread(new ThreadStart(HiloRFID));
                m_hilo.Start();
               

            }
            m_hiloMostrar = new Thread(new ThreadStart(MostrarInformacion));
            m_hiloMostrar.Start();
        }
        protected delegate void StringDelegate(string str);
        protected void MostrarInformacion(string EPC)
        {

            if (InvokeRequired)
            {
                BeginInvoke(new StringDelegate(MostrarInformacion), EPC);
            }
            else
            {


                if (m_EPC.Count != 0)
                {
                    m_mutex.WaitOne();
                    m_sleep = m_sleep0 / m_EPC.Count;
                    /*
                    if (m_EPC.Count > 10)
                    {
                        m_sleep = 500;
                    }
                    else if (m_EPC.Count > 8)
                    {
                        m_sleep = 600;
                    }
                    else if (m_EPC.Count > 5)
                    {
                        m_sleep = 1000;
                    }
                    else if (m_EPC.Count > 3)
                    {
                        m_sleep = 1500;
                    }
                    else
                    {
                        m_sleep = 2000;
                    }
                    */
                    EPC = m_EPC[0];


                    m_parametros[0].Text = EPC;// + " " + m_sleep + " " + m_EPC.Count;
                    m_parametros[0].Visible = true;
                        
                    
                
                    m_EPC.RemoveAt(0);
                    m_mutex.ReleaseMutex();
                }
            }
        }
        protected delegate void VoidDelegate();
        protected void OcultarInformacion()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new VoidDelegate(OcultarInformacion));
            }
            else
            {

               // m_retrato.Visible = false;
                for (int i = 0; i < m_parametros.Length; i++)
                {
                    m_parametros[i].Visible = false;
                }
                //Thread.Sleep(250);
                Thread.Sleep(50);
            }
        }
        protected bool CargarXML()
        {
            bool ret = false;
            try
            {
                if (File.Exists(m_ArchivoConfiguracion))
                {
                    using (XmlReader reader = XmlReader.Create("config.xml"))
                    {
                        XmlDocument xdoc = new XmlDocument();
                        xdoc.Load(reader);
                        XmlNodeList lista = xdoc.GetElementsByTagName("pantalla");
                        if (lista.Count == 0)
                        {
                            MessageBox.Show("Archivo de configuración inválido.\n No se encuentra el tag pantalla.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }
                        lista = xdoc.GetElementsByTagName("fondo");
                        if (lista.Count == 0)
                        {
                            MessageBox.Show("Archivo de configuración inválido.\n No se encuentra el tag fondo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }
                        XmlElement e = (XmlElement)lista[0];
                        m_ArchivoImagenFondo = e.InnerText;
                        if (!File.Exists(m_ArchivoImagenFondo))
                        {
                            MessageBox.Show("Archivo " + m_ArchivoImagenFondo + " no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }

                        lista = xdoc.GetElementsByTagName("retrato");
                        if (lista.Count == 0)
                        {
                            MessageBox.Show("Archivo de configuración inválido.\n No se encuentra el tag retrato.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }
                        e = (XmlElement)lista[0];

                        int x = e.HasAttribute("x") ? int.Parse(e.Attributes["x"].Value.ToString()) : -1;
                        int y = e.HasAttribute("y") ? int.Parse(e.Attributes["y"].Value.ToString()) : -1;

                        if (x == -1 || y == -1)
                        {
                            MessageBox.Show("Posición de retrato inválida.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }
                        m_Pimagen = new Point(x, y);

                        x = e.HasAttribute("ancho") ? int.Parse(e.Attributes["ancho"].Value.ToString()) : -1;
                        y = e.HasAttribute("alto") ? int.Parse(e.Attributes["alto"].Value.ToString()) : -1;
                        if (x == -1 || y == -1)
                        {
                            MessageBox.Show("Tamaño de retrato inválido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }
                        m_Simagen = new Size(x, y);

                        m_directorioRetratos = e.HasAttribute("directorio") ? e.Attributes["directorio"].Value : null;
                        if (m_directorioRetratos == null || !Directory.Exists(m_directorioRetratos))
                        {
                            MessageBox.Show("Archivo de configuración inválido.\n Directorio de imágenes inválido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }

                        lista = xdoc.GetElementsByTagName("datos");
                        if (lista.Count == 0)
                        {
                            MessageBox.Show("Archivo de configuración inválido.\n No se encuentra el tag datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }
                        e = (XmlElement)lista[0];

                        x = e.HasAttribute("x") ? int.Parse(e.Attributes["x"].Value.ToString()) : -1;
                        y = e.HasAttribute("y") ? int.Parse(e.Attributes["y"].Value.ToString()) : -1;

                        if (x == -1 || y == -1)
                        {
                            MessageBox.Show("Posición de información inválida.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }
                        m_Pinformacion = new Point(x, y);

                        m_EspacioInformacion = e.HasAttribute("espacio") ? int.Parse(e.Attributes["espacio"].Value.ToString()) : -1;
                        if (m_EspacioInformacion == -1)
                        {
                            MessageBox.Show("Espacio entre líneas inválido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }


                        m_tamanoFuente = e.HasAttribute("tamano") ? int.Parse(e.Attributes["tamano"].Value.ToString()) : -1;
                        if (m_tamanoFuente == -1)
                        {
                            MessageBox.Show("Tamaño de fuente inválido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }
                        x = e.HasAttribute("fondo") ? int.Parse(e.Attributes["fondo"].Value.ToString(), System.Globalization.NumberStyles.HexNumber) : -1;
                        int[] rgb;
                        if (x != -1)
                        {
                            rgb = ObtenerRGB(x);
                            m_ColorFondoFuente = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                            m_BcolorFondoFuente = true;
                        }
                        x = e.HasAttribute("color") ? int.Parse(e.Attributes["color"].Value.ToString(), System.Globalization.NumberStyles.HexNumber) : -1;
                        
                        if (x != -1)
                        {
                            rgb = ObtenerRGB(x);
                            m_ColorFuente = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                            m_BcolorFuente = true;
                        }




                        lista = xdoc.GetElementsByTagName("regular");
                        if (lista.Count != 0)
                        {
                            e = (XmlElement)lista[0];
                            m_ArchivoFuenteRegular = e.InnerText;
                            if (!File.Exists(m_ArchivoFuenteRegular))
                            {
                                MessageBox.Show("Archivo " + m_ArchivoFuenteRegular + " no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return ret;
                            }
                        }

                        lista = xdoc.GetElementsByTagName("negrita");
                        if (lista.Count != 0)
                        {
                            e = (XmlElement)lista[0];
                            m_ArchivoFuenteNegrita = e.InnerText;
                            if (!File.Exists(m_ArchivoFuenteNegrita))
                            {
                                MessageBox.Show("Archivo " + m_ArchivoFuenteNegrita + " no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return ret;
                            }
                        }
                        lista = xdoc.GetElementsByTagName("parametro");

                        if (lista.Count != 0)
                        {
                            m_indiceConfirmacion = -1;
                            int i;
                            for(i =0; i< lista.Count; i++)
                            {

                                e = (XmlElement)lista[i];

                                m_etiquetas.Add(e.HasAttribute("etiqueta") ? e.Attributes["etiqueta"].Value : "");

                                m_informacion.Add(e.InnerText);
                                if (e.InnerText == "Confirmacion")
                                    m_indiceConfirmacion = i;

                            }
                            /*
                            if (m_indiceConfirmacion == -1)
                            {
                                m_informacion.Add("Confirmacion");
                                m_etiquetas.Add("Confirmacion");
                                m_indiceConfirmacion = i;

                            }
                            */
                        }


                        lista = xdoc.GetElementsByTagName("database");
                        if (lista.Count == 0)
                        {
                            MessageBox.Show("Archivo de configuración inválido.\n No se encuentra el tag database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }
                        e = (XmlElement)lista[0];

                        m_DBHost = e.HasAttribute("host") ? e.Attributes["host"].Value : "localhost";
                        m_Database = e.InnerText;


                        lista = xdoc.GetElementsByTagName("rfid");
                        if (lista.Count == 0)
                        {
                            MessageBox.Show("Archivo de configuración inválido.\n No se encuentra el tag rfid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return ret;
                        }
                        e = (XmlElement)lista[0];

                        m_RFIDModel = e.HasAttribute("modelo") ? e.Attributes["modelo"].Value : "xportal";
                        m_RFIDReaderHost = e.InnerText;

                       



                        ret = true;
                    }
                }
                else
                {
                    MessageBox.Show("No existe el archivo de configuración.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return ret;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        protected int[] ObtenerRGB(int RGB)
        {
            int[] rgb = new int[3];

            for(int i = 2; i >= 0; i--)
            {
                rgb[i] = 0xff & RGB;
                RGB >>= 8;
            }

            return rgb;
        }


        protected void MostrarInformacion()
        {
            while (m_iniciado)
            {
                
                if (m_EPC.Count != 0)
                {
                    MostrarInformacion("");
                    Thread.Sleep(m_sleep);
                    OcultarInformacion();
                }
                else
                {
                    Thread.Sleep(50);
                }
            }

        }

        protected void HiloRFID()
        {
            //que.ConnectionLost += OnConnectionLost;
            //que.KeepAliveReceived += OnKeepAliveRecived;
            
            while(m_iniciado)
            {
                m_EPC.Add(RecibirNombre());
                Thread.Sleep(200);
            }
  
        }
/*
        void OnTag(RFIDTag tag, RFIDTag[] tags)
        {
            if (m_db.PreguntarBienvenida(tag.EPC))
            {

                m_mutex.WaitOne();
                m_EPC.Add(tag.EPC);
                m_mutex.ReleaseMutex();
                m_db.MarcarBienvenida(tag.EPC);
                Console.WriteLine(tag.EPC);



            }
            
        }

        void OnTagDel(RFIDTag tag, RFIDTag[] tags)
        {
            if (tags.Length == 0)
            {
                OcultarInformacion();
            }
        }
        */
/*

        void OnTag(RFIDTag tag, RFIDTag[] tags)
        {
            if (tags.Length == 0)
            {
                OcultarInformacion();
            }
            else
            {
                
                for(int i =0;i<tags.Length; i++)
                {
                    if (m_db.ExisteEPC(tags[i].EPC))
                    {
                        MostrarInformacion(tags[i].EPC);
                        break;
                    }                    
                }
                

            }
        }
        */
  

        protected void HiloDemo()
        {
            while (m_iniciado)
            {
                try
                {
                    for(int i = 1; i < 6; i++)
                    {
                        MostrarInformacion(i.ToString());
                        Thread.Sleep(5000);
                       // OcultarInformacion();
                    }
                    
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }

        }
        protected string RecibirNombre()
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = m_udp.Receive(ref sender);
            string buffer = Encoding.UTF8.GetString(data, 0, data.Length);
            return buffer;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(MessageBox.Show("¿Desea cerrar la aplicación?","Cerrar", MessageBoxButtons.YesNo,MessageBoxIcon.Question)== DialogResult.Yes)
            {
                m_iniciado = false;
                Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void m_retrato_Click(object sender, EventArgs e)
        {

        }
    }
}
