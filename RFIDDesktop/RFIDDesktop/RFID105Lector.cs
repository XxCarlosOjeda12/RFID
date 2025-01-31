using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReaderB;
using System.Threading;
namespace Sitacomm
{
    public class RFID105Reader
    {   
        protected int m_puerto;
        protected byte m_direccionCom;
        protected byte m_velocidad;
        protected int m_indicePuerto;
        protected bool m_LectorAbierto = false;
        protected byte[] m_version = { 0, 0 };
        protected byte m_tipoLector;
        protected byte m_frecMin = 0;
        protected byte m_frecMax = 0;
        protected byte m_dbm = 0;
        protected byte m_PeriodoEscaneo = 0;
        protected int m_estadoRespuesta = -1;
        protected byte[] m_EPC = new byte[5000];
        protected Thread m_hilo = null;
        protected bool m_escaneando = false;
        public delegate void EPCDetectedHdl(string codigo);
        public event EPCDetectedHdl OnCodeDetected;
        protected int m_Periodo = 1000;



        public RFID105Reader()
        {
            m_puerto = 0; 
            m_direccionCom = 0; //direccion lector 0
            m_velocidad = 5; // 57600
            m_indicePuerto = 0;
            
            
            
        }
        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();

        }
        public string LeerEPC()
        {
            string EPC = null;
            byte AdrTID = 0;
            byte LenTID = 0;
            byte TIDFlag = 0;
            int CardNum = 0;
            int Totallen = 0;
            int EPClen = 0;
            string temps;


            if (m_LectorAbierto)
            {
                m_estadoRespuesta = StaticClassReaderB.Inventory_G2(ref m_direccionCom, AdrTID, LenTID, TIDFlag, m_EPC, ref Totallen, ref CardNum, m_indicePuerto);
                if ((m_estadoRespuesta == 1) | (m_estadoRespuesta == 2) | (m_estadoRespuesta == 3) | (m_estadoRespuesta == 4) | (m_estadoRespuesta == 0xFB))
                {
                    if (Totallen != 0)
                    {
                        byte[] daw = new byte[Totallen];
                        Array.Copy(m_EPC, daw, Totallen);
                        temps = ByteArrayToHexString(daw);
                      //  fInventory_EPC_List = temps;            //存贮记录
                        if (CardNum == 0)
                        {
                            return null;
                        }
                        EPClen = daw[0];
                        EPC = temps.Substring(2, EPClen * 2);
                        if (EPC.Length != EPClen * 2)
                            return null;
                      
                    }
                }

            }

            return EPC;
        }
        public void CerrarLector()
        {
            StaticClassReaderB.CloseSpecComPort(m_direccionCom);
        }
        public int AbrirLector()
        {
            int resultado = 0;
            resultado = StaticClassReaderB.AutoOpenComPort(ref m_puerto, ref m_direccionCom, m_velocidad, ref m_indicePuerto);
            if (resultado == 0)
            {
                byte[] TRYTipo = { 0, 0 };

                m_LectorAbierto = true;
                m_estadoRespuesta = StaticClassReaderB.GetReaderInformation(ref m_direccionCom, m_version, ref m_tipoLector, TRYTipo, ref m_frecMax, ref m_frecMin, ref m_dbm, ref m_PeriodoEscaneo, m_indicePuerto);
                if (m_estadoRespuesta == 0)
                {
                    if (m_tipoLector != 0x08)
                    {
                        StaticClassReaderB.CloseSpecComPort(m_direccionCom);
                        m_LectorAbierto = false;
                        return 1; //modelo no es ZK RFID105
                    }
                    m_dbm = 13; // 13dmb
                    m_frecMax = 49; //frecuencias US
                    m_frecMin = 128;
                    m_PeriodoEscaneo = 10;
                    m_estadoRespuesta = StaticClassReaderB.SetPowerDbm(ref m_direccionCom, m_dbm, m_indicePuerto);
                    m_estadoRespuesta = StaticClassReaderB.Writedfre(ref m_direccionCom, ref m_frecMax, ref m_frecMin, m_indicePuerto);
                    m_estadoRespuesta = StaticClassReaderB.WriteScanTime(ref m_direccionCom, ref m_PeriodoEscaneo, m_indicePuerto);
                    return 0;
                }
                else if ((m_estadoRespuesta == 0x35) | (m_estadoRespuesta == 0x30))
                {
                    StaticClassReaderB.CloseSpecComPort(m_direccionCom);
                    m_LectorAbierto = false;
                    return 2; //No se p uede comunicar con el equipo

                }
                else
                {
                    return 3;
                }

                
            }
            else
                return 3; //error

        }
        public int IniciarEscaneo(int periodo=100)
        {
            m_Periodo = periodo;
            int ret = -1;
            if (AbrirLector() == 0)
            {
                Thread.Sleep(1000);
                try
                {
                    if (m_hilo != null)
                    {
                        m_escaneando = false;
                        Thread.Sleep(500);
                        m_hilo.Abort();
                    }
                }
                catch { }
                m_hilo = new Thread(new ThreadStart(HiloEscaneo));
                m_hilo.Start();
                ret = 0;
            }
            return ret;
        }

        protected void HiloEscaneo()
        {
            m_escaneando = true;
            while (m_escaneando)
            {
                String res = LeerEPC();

                if (res != null)
                {
                    
                    OnCodeDetected?.Invoke(res);
                    Thread.Sleep(m_Periodo);
                }
            }
        }

        public void DetenerEscaneo()
        {
            try
            {
                m_escaneando = false;
                Thread.Sleep(500);
                m_hilo.Abort();
                m_hilo = null;
                CerrarLector();
            }
            catch { }

        }
    }
}
