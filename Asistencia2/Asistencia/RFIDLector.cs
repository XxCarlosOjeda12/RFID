using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Impinj.OctaneSdk;

namespace Sitacomm.RFID
{
    abstract public class RFIDReader
    {
        protected double m_power;
        protected string m_host;
        public delegate void TagsReportedHandler(RFIDReader reader, RFIDTagReport report);
        public event TagsReportedHandler TagsReported;

        public delegate void KeepaliveReceivedHandler(RFIDReader reader);
        public event KeepaliveReceivedHandler KeepaliveReceived;

        public delegate void ConnectionLostHandler(RFIDReader reader);
        public event ConnectionLostHandler ConnectionLost;




        public RFIDReader(string host, double powerdmb)
        {
            m_host = host;
            m_power = powerdmb > 30 ? 30 : powerdmb;
        }
        public string Host
        {
            get
            {
                return m_host;
            }
        }

        public abstract string Tipo { get;}

        public void OnTagsReported(RFIDReader reader, RFIDTagReport report)
        {
            TagsReported?.Invoke(this, report);
        }
        public void OnKeepaliveReceived(RFIDReader reader)
        {
            KeepaliveReceived?.Invoke(this);
        }
        public void OnConnectionLost(RFIDReader reader)
        {
            ConnectionLost?.Invoke(this);
        }
        abstract protected void Configure(); 
        abstract public  void Connect();
        abstract public void Disconnect();
        abstract public void Start();
        abstract public void Stop();
        abstract public RFIDSettings Settings
        {
            get;set;
        }

    }

    public interface RFIDTagReport
    {
    }
    public interface RFIDSettings
    {
    }

    public abstract class RFIDTag
    {
        protected string m_epc;
        protected ulong m_lastSeenTime;
        public RFIDTag(string epc, ulong time)
        {
            m_epc = epc;
            m_lastSeenTime = time;
        }
        public string EPC
        {
            get
            {
                return m_epc;
            }
        }
        public ulong LastSeenTime
        {
            get
            {
                return m_lastSeenTime;
            }
            set
            {
                m_lastSeenTime = value;
            }
        }
    }
    public class EventosTag: RFIDImpijnTag
    {
        protected string m_nombre = "";
        public EventosTag(string Nombre, string epc, ulong time): base(epc, time)
        {
            m_nombre = Nombre;
        }
        public string Nombre
        {
            get
            {
                return m_nombre;
            }
        }
    }
}
