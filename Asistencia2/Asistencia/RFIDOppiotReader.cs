using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReaderB;
using System.Threading;
using System.Net.NetworkInformation;
namespace Sitacomm.RFID
{

    class RFIDOppiotReader: RFIDReader
    {
        protected Thread m_thread;
        protected int m_period = 100;
        protected byte m_readerAdd = 0xff;
        protected int m_comIndex;
        protected bool m_connected = false;
        protected bool m_reading = false;
        public RFIDOppiotReader(string hostname, double powerdbm) : base(hostname,powerdbm)
        {

        }
        public override string Tipo { get { return "OPPIOT"; } }
        public override void Connect()
        {
            if (m_host.IndexOf("COM") != -1)
            {
                int port = 0;
                int openresult;
                byte fBaud = 5; //57600
                openresult = 30;
                m_comIndex = -1;



                try
                {
                    port = Convert.ToInt32(m_host.Substring(3, m_host.Length - 3));
                    fBaud = 5;
                    openresult = StaticClassReaderB.OpenComPort(port, ref m_readerAdd, fBaud, ref m_comIndex);

                    if (openresult == 0x35)
                    {
                        m_connected = false;
                    }
                    else if (openresult == 0)
                    {
                        m_connected = true;
                    }
                    else
                    {
                        m_connected = false;
                    }
                    if (m_comIndex == -1)
                    {
                        m_connected = false;
                    }

                    Configure();
                }
                catch (Exception e)
                {
                    m_connected = false;
                }
            }
            else
            {
                int openresult;
                openresult = 30;
                m_comIndex = -1;
                int port = 27011;


                try
                {
                    openresult = StaticClassReaderB.OpenNetPort(port, m_host, ref m_readerAdd, ref m_comIndex);

                    if (openresult == 0x35)
                    {
                        m_connected = false;
                    }
                    else if (openresult == 0 || openresult == 0x30)
                    {
                        m_connected = true;
                    }
                    else
                    {
                        m_connected = false;
                    }
                    if (m_comIndex == -1)
                    {
                        m_connected = false;
                    }

                    Configure();
                }
                catch (Exception e)
                {
                    m_connected = false;
                }
            }
        }
        public override void Disconnect()
        {
            if (m_connected)
            {
                if (m_host.IndexOf("COM") != -1)
                {

                    int port = Convert.ToInt32(m_host.Substring(3, m_host.Length - 3));
                    StaticClassReaderB.CloseSpecComPort(port);
                }
                else
                {
                    StaticClassReaderB.CloseNetPort(m_comIndex);
                }
            }
        }
        protected override void Configure()
        {
            if (m_connected)
            {

                byte dminfre = 128;
                byte dmaxfre = 0;
                byte fComAdr = 0;
                byte scantime = 10;
                byte[] Parameter = { 1, 4, 1, 2, 1, 0 };

                int fCmdRet = StaticClassReaderB.SetPowerDbm(ref fComAdr, (byte)m_power, m_comIndex);
                if (fCmdRet == 48)
                {
                    m_connected = false;
                    OnConnectionLost(this);
                    return;
                }
                fCmdRet = StaticClassReaderB.Writedfre(ref fComAdr, ref dmaxfre, ref dminfre, m_comIndex);
                if (fCmdRet == 48)
                {
                    m_connected = false;
                    OnConnectionLost(this);
                    return;
                }

                fCmdRet = StaticClassReaderB.WriteScanTime(ref fComAdr, ref scantime, m_comIndex);
                if (fCmdRet == 48)
                {
                    m_connected = false;
                    OnConnectionLost(this);
                    return;
                }

                fCmdRet = StaticClassReaderB.SetWorkMode(ref fComAdr, Parameter, m_comIndex);
                if (fCmdRet == 48)
                {
                    m_connected = false;
                    OnConnectionLost(this);
                    return;
                }
                Parameter[0] = 0;
                fCmdRet = StaticClassReaderB.SetWorkMode(ref fComAdr, Parameter, m_comIndex);
                if (fCmdRet == 48)
                {
                    m_connected = false;
                    OnConnectionLost(this);
                    return;
                }


            }
        }
        public override void Start()
        {
            IniciarEscaneo();
        }
        public override void Stop()
        {
            DetenerEscaneo();
        }

        public override RFIDSettings Settings
        {
            get
            {
                return null;
            }

            set
            {
            }
        }
        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();

        }
        public List<string> LeerEPC()
        {
            int m = 0;
            string sEPC = null;
            List<string> EPC = new List<string>();
            byte AdrTID = 0;
            byte LenTID = 0;
            byte TIDFlag = 0;
            int CardNum = 0;
            int Totallen = 0;
            int EPClen = 0;
            string temps;
            byte port = 0;
            byte[] m_EPC = new byte[5000];

            if (m_connected)
            {
                int fCmdRet = StaticClassReaderB.Inventory_G2(ref port, AdrTID, LenTID, TIDFlag, m_EPC, ref Totallen, ref CardNum, m_comIndex);
                if ((fCmdRet == 1) | (fCmdRet == 2) | (fCmdRet == 3) | (fCmdRet == 4) | (fCmdRet == 0xFB))
                {
                    if (Totallen != 0)
                    {
                        byte[] daw = new byte[Totallen];
                        Array.Copy(m_EPC, daw, Totallen);
                        temps = ByteArrayToHexString(daw);
                        //  fInventory_EPC_List = temps;            
                        if (CardNum == 0)
                        {
                            return EPC;
                        }

                        for (int CardIndex = 0; CardIndex < CardNum; CardIndex++)
                        {
                            EPClen = daw[m];
                            sEPC = temps.Substring(m * 2 + 2, EPClen * 2);
                            m = m + EPClen + 1;
                            if (sEPC.Length != EPClen * 2)
                                return null;
                            EPC.Add(sEPC);
                        }
                    }
                }
                else if (fCmdRet == 48)
                {
                    m_connected = false;
                    OnConnectionLost(this);
                }

            }

            return EPC;
        }

        protected int IniciarEscaneo()
        {
            int ret = -1;
            if (m_connected)
            {
                Thread.Sleep(1000);
                try
                {
                    if (m_thread != null)
                    {
                        m_reading = false;
                        Thread.Sleep(500);
                        m_thread.Abort();
                    }
                }
                catch { }
                m_thread = new Thread(new ThreadStart(HiloEscaneo));
                m_thread.Start();
                ret = 0;
            }
            return ret;
        }

        protected void HiloEscaneo()
        {
            m_reading = true;
            while (m_reading)
            {
                List<string> res = LeerEPC();
                if (!m_connected)
                {
                    m_reading = false;
                }
                if (res.Count != 0)
                {
                    OnTagsReported(this, new RFIDOppiotTagReport(res));
                    Thread.Sleep(m_period);
                }
            }
            m_reading = false;
        }

        protected void DetenerEscaneo()
        {
            try
            {
                m_reading = false;
                Thread.Sleep(500);
                m_thread.Abort();
                m_thread = null;
                Disconnect();
            }
            catch { }

        }


    }
    public class RFIDOppiotTagReport : RFIDTagReport
    {
        protected List<string> m_report;

        public RFIDOppiotTagReport(List<string> impjnReport)
        {
            m_report = impjnReport;
        }

        public List<string> Report
        {
            get
            {
                return m_report;
            }
        }
    }
    public class RFIDOppiotTag : RFIDTag
    {
        public RFIDOppiotTag(string epc, ulong time) : base(epc, time) { }

    }

}
