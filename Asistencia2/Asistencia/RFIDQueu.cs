using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace Sitacomm.RFID
{

    public abstract class RFIDQueu
    {
        protected List<RFIDTag> m_lista = new List<RFIDTag>();
        protected RFIDReader m_reader = null;
        protected string m_readerHost;
        protected Mutex m_mutex = new Mutex();
        protected int m_timeout;
        protected int m_period;
        
        protected Thread m_thread = null;
        protected bool m_running = false;
        protected double m_power;

        public delegate void TagAddedHandler(RFIDTag tag, RFIDTag[] tags);
        public event TagAddedHandler TagAdded;

        public delegate void TagDeletedHandler(RFIDTag tag, RFIDTag[] tags);
        public event TagDeletedHandler TagDeleted;

        public delegate void TagUpdatedHandler(RFIDTag tag, RFIDTag[] tags);
        public event TagUpdatedHandler TagUpdated;




        
        public delegate void KeepAliveReceivedHandler();
        public event KeepAliveReceivedHandler KeepAliveReceived;

        public delegate void ConnectionLostHandler();
        public event ConnectionLostHandler ConnectionLost;
        

        public RFIDQueu(string readerHost, double powerdbm, int timeout, int monitorPeriod)
        {
            m_power = powerdbm;
            m_readerHost = readerHost;
            m_timeout = timeout;
            m_period = monitorPeriod;
            CreateReader();
            if (m_reader != null)
            {
                m_reader.TagsReported += OnTagsReported;
                m_reader.KeepaliveReceived += OnKeepAliveReceived;
                m_reader.ConnectionLost += OnConnectionLost;
            }
        }
        //protected abstract void ConfigureReader();
        protected abstract void CreateReader();
        protected abstract void OnTagsReported(RFIDReader sender, RFIDTagReport report);
        protected abstract void OnConnectionLost(RFIDReader sender);
        protected abstract void OnKeepAliveReceived(RFIDReader sender);
        public virtual int IndexOf(RFIDTag tag)
        {
            int ret = -1;

            try
            {
                if (tag == null)
                    throw new ArgumentNullException("tag");
                m_mutex.WaitOne();

                for (int i = 0; i < m_lista.Count; i++)
                {
                    if (m_lista[i].EPC == tag.EPC)
                    {
                        ret = i;
                        break;
                    }
                }
                m_mutex.ReleaseMutex();
            }
            catch (Exception e)
            {
                m_mutex.ReleaseMutex();
                throw e;
            }

            return ret;
        }
        public virtual void AddTag(RFIDTag tag)
        {
            try
            {

                int i;
                if ((i = IndexOf(tag)) == -1)
                {
                    m_mutex.WaitOne();
                    m_lista.Add(tag);
                    OnTagAdded(tag, Tags);
                    m_mutex.ReleaseMutex();
                }
                else
                {
                    m_mutex.WaitOne();
                    m_lista[i].LastSeenTime = tag.LastSeenTime;
                    OnTagUpdated(m_lista[i], Tags);
                    m_mutex.ReleaseMutex();
                }
            }
            catch (Exception e)
            {
                m_mutex.ReleaseMutex();
                throw e;
            }

        }
        public void Start()
        {
            m_reader.Start();
            if (m_thread == null)
            {
                m_thread = new Thread(new ThreadStart(ThreadProc));
                m_thread.Start();
            }

        }
        public void OnTagDeleted(RFIDTag tag, RFIDTag[] tags)
        {
            TagDeleted?.Invoke(tag, tags);
        }
        public void OnTagUpdated(RFIDTag tag, RFIDTag[] tags)
        {
            TagUpdated?.Invoke(tag, tags);
        }
        public void OnTagAdded(RFIDTag tag, RFIDTag[] tags)
        {
            TagAdded?.Invoke(tag, tags);
        }
        
        public void OnConnectionLost()
        {
            ConnectionLost?.Invoke();
        }
        public void OnKeepAliveReceived()
        {
            KeepAliveReceived?.Invoke();
        }
        
        public void Stop()
        {
            m_reader.Stop();
            if (m_thread != null)
            {
                m_running = false;
            }
        }

        public RFIDTag[] Tags
        {
            get
            {
                try
                {
                    RFIDTag[] array;

                    array = m_lista.ToArray();

                    return array;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        protected void ThreadProc()
        {
            // ulong ms1970 =(ulong) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            m_running = true;
            while (m_running)
            {


                Thread.Sleep(m_period);

                m_mutex.WaitOne();

                for(int i = 0; i< m_lista.Count; i++)
                {
                    DateTime now = DateTime.UtcNow;
                    ulong us = (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                    if (us - m_lista[i].LastSeenTime > (uint)m_timeout) 
                    {
                        RFIDTag t = m_lista[i];
                        m_lista.RemoveAt(i);
                        i--;
                        OnTagDeleted(t, Tags);
                    }

                }
                m_mutex.ReleaseMutex();



            }
        }


    }
}
