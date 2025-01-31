using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Impinj.OctaneSdk;
using System.Net.NetworkInformation;
using System.Threading;
namespace Sitacomm.RFID
{
    public class RFIDImpijnReader : RFIDReader
    {
        protected ImpinjReader m_reader = new ImpinjReader();

        public RFIDImpijnReader(string hostname, double powerdbm) : base(hostname, powerdbm)
        {
            m_reader.TagsReported += OnReadImpijnTag;
            m_reader.KeepaliveReceived += OnImpijnKeepaliveReceived;
            m_reader.ConnectionLost += OnImpijnConnectionLost;

        }
        public override void Connect()
        {
            m_reader.Connect(m_host);
            Configure();
        }
        public override string Tipo { get { return "Impijn"; } }
        public override void Disconnect()
        {
            if (m_reader.IsConnected)
                m_reader.Disconnect();
        }
        public override void Start()
        {
            if (m_reader.IsConnected)
                m_reader.Start();
        }
        public override void Stop()
        {
            if (m_reader.IsConnected)
                m_reader.Stop();
        }
        protected void OnReadImpijnTag(ImpinjReader reader, TagReport report)
        {
            if (reader == m_reader)
            {
                RFIDImpijnTagReport r = new RFIDImpijnTagReport(report);
                OnTagsReported(this, r);

            }
        }
        protected void OnImpijnKeepaliveReceived(ImpinjReader reader)
        {
            if (reader == m_reader)
                OnKeepaliveReceived(this);
        }

        protected void OnImpijnConnectionLost(ImpinjReader reader)
        {
            if (reader == m_reader)
                OnConnectionLost(this);
        }


        protected override void Configure()
        {
            RFIDImpijnSettings m_settings = (RFIDImpijnSettings)Settings;

            Settings settings = m_settings.Settings;


            settings.Report.IncludeAntennaPortNumber = true;

            settings.ReaderMode = ReaderMode.DenseReaderM8;

            settings.SearchMode = SearchMode.DualTarget;

            settings.Session = 2;

            settings.Report.IncludeAntennaPortNumber = true;
            settings.Report.IncludeFirstSeenTime = true;
            settings.Report.IncludeLastSeenTime = true;
            settings.Report.IncludeSeenCount = true;

            settings.Keepalives.Enabled = true;
            settings.Keepalives.PeriodInMs = 2000;
            settings.Keepalives.EnableLinkMonitorMode = true;
            settings.Keepalives.LinkDownThreshold = 1;



            foreach (AntennaConfig a in settings.Antennas)
            {
                a.TxPowerInDbm = m_power;
                //a.MaxTxPower = true;
                a.MaxRxSensitivity = true;
            }

            Settings = new RFIDImpijnSettings(settings);

        }



        public override RFIDSettings Settings
        {
            get
            {
                Settings s = null;
                if (m_reader.IsConnected)
                {
                    s = m_reader.QueryDefaultSettings();
                }
                return new RFIDImpijnSettings(s);
            }

            set
            {
                if (m_reader.IsConnected)
                {
                    RFIDImpijnSettings s = (RFIDImpijnSettings)value;
                    m_reader.ApplySettings(s.Settings);
                }
            }
        }

    }


    public class RFIDImpijnSettings : RFIDSettings
    {
        protected Settings m_setings;
        public RFIDImpijnSettings(Settings ImpijnSettings)
        {
            m_setings = ImpijnSettings;
        }
        public Settings Settings
        {
            get
            {
                return m_setings;
            }
        }
    }

    public class RFIDImpijnTagReport : RFIDTagReport
    {
        protected TagReport m_report;

        public RFIDImpijnTagReport(TagReport impjnReport)
        {
            m_report = impjnReport;
        }

        public TagReport Report
        {
            get
            {
                return m_report;
            }
        }
    }

    public class RFIDImpijnQueu : RFIDQueu
    {
        bool m_connected = false;
        protected ushort m_nKeepsAlive = 1;
        public RFIDImpijnQueu(string readerHost, double powerdbms = 28.5, int timeout = 5000, int monitorPeriod = 250) : base(readerHost, powerdbms, timeout, monitorPeriod) { }
        protected RFIDImpijnSettings m_settings;
        protected void Conectar()
        {
            do
            {
                try
                {
                    while (!ReaderIsAvailable(m_readerHost))
                    {
                        Thread.Sleep(1000);
                    }

                    m_reader.Connect();

                   // ConfigureReader();

                    m_connected = true;
                }
                catch (Exception)
                {
                    m_connected = false;
                }
            } while (!m_connected);
        }
 
        protected override void CreateReader()
        {
            

            m_reader = new RFIDImpijnReader(m_readerHost,m_power);
            m_reader.TagsReported += OnTagsReported;

            Conectar();


            
        }


 





        protected override void OnTagsReported(RFIDReader sender, RFIDTagReport report)
        {
            TagReport tr = ((RFIDImpijnTagReport)report).Report;
            foreach (Tag tag in tr)
            {
                RFIDImpijnTag ti = new RFIDImpijnTag(tag.Epc.ToString().Replace(" ", string.Empty), (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
                m_mutex.WaitOne();
                AddTag(ti);
                m_mutex.ReleaseMutex();
            }

        }

        protected override void OnConnectionLost(RFIDReader sender)
        {
            OnConnectionLost();
            
            m_connected = false;
            Conectar();
            m_reader.Start();

        }

        protected override void OnKeepAliveReceived(RFIDReader sender)
        {
            OnKeepAliveReceived();
            
            m_connected = true;


        }

        static bool ReaderIsAvailable(string address)
        {
            // Ping the reader.
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            byte[] buffer = Encoding.Default.GetBytes("12345");
            PingReply reply = pingSender.Send(address, 500, buffer, options);
            if (reply.Status == IPStatus.Success)
                return true;
            else
                return false;
        }

    }
    public class RFIDImpijnTag:RFIDTag
    {
        public RFIDImpijnTag(string epc, ulong time):base(epc, time){ }

    }
}
