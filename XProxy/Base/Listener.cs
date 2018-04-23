using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;
using System.Collections.Generic;

using XLog;
using XProxy.Plugin;
using XProxy.Config;

namespace XProxy.Base
{
    /// <summary>
    /// �����������
    /// </summary>
    public class Listener : IDisposable
    {
        #region ����
        private IPAddress _Address;
        /// <summary>
        /// ��ַ
        /// </summary>
        public IPAddress Address
        {
            get
            {
                if (_Address != null) return _Address;

                if (Config.Address == "0.0.0.0")
                    _Address = IPAddress.Any;
                else
                {
                    try
                    {
                        _Address = Dns.GetHostEntry(Config.Address).AddressList[0];
                    }
                    catch (Exception ex)
                    {
                        Dispose();
                        String msg = "��ȡ[" + Config.Address + "]��IP��ַʱ����";
                        WriteLog(msg);
                        XLog.Trace.WriteLine(msg);
                        throw ex;
                    }
                }
                return _Address;
            }
        }

        private TcpListener _TcpServer;
        /// <summary>
        /// Tcp������
        /// </summary>
        public TcpListener TcpServer
        {
            get { return _TcpServer; }
            set { _TcpServer = value; }
        }

        /// <summary>
        /// �ͻ������顣������¼���������ӿͻ��ˡ�
        /// </summary>
        public List<Session> Clients = new List<Session>();

        /// <summary>
        /// д��־ί��
        /// </summary>
        public event WriteLogDelegate OnWriteLog;

        private PluginManager _Plugin;
        /// <summary>
        /// ���������
        /// </summary>
        internal PluginManager Plugin
        { get { return _Plugin; } set { _Plugin = value; } }

        private ListenerConfig _Config;
        /// <summary>
        /// ����
        /// </summary>
        public ListenerConfig Config
        {
            get
            {
                return _Config;
            }
            set
            {
                if (_Config != value)
                {
                    _Config = value;
                    _Address = null;
                }
            }
        }
        #endregion

        #region ���캯��
        /// <summary>
        /// ��ʼ�������������ʵ��
        /// </summary>
        /// <param name="config"></param>
        public Listener(ListenerConfig config)
        {
            Config = config;
        }
        #endregion

        #region ��ʼ/ֹͣ
        /// <summary>
        /// ��ָ����IP�Ͷ˿��Ͽ�ʼ����
        /// </summary>
        public virtual void Start()
        {
            if (Plugin == null)
            {
                Plugin = new PluginManager();
                Plugin.Listener = this;
                if (Config.IsShow && OnWriteLog != null) Plugin.OnWriteLog += new WriteLogDelegate(WriteLog);
                Plugin.OnInit(this);
            }

            try
            {
                int plugincount = Plugin.Plugins == null ? 0 : Plugin.Plugins.Count;
                WriteLog(String.Format("��ʼ���� {0}:{1} [{2}] �����{3}��", Address.ToString(), Config.Port, Config.Name, plugincount));
                if (TcpServer != null) Stop();
                TcpServer = new TcpListener(Address, Config.Port);
                // ָ�� TcpListener �Ƿ�ֻ����һ�������׽����������ض��˿ڡ�
                // ֻ�м�������IP��ʱ�򣬲Ŵ� ExclusiveAddressUse
                TcpServer.ExclusiveAddressUse = (Address == IPAddress.Any);
                TcpServer.Start();
                // ��ʼ�첽���ܴ��������
                TcpServer.BeginAcceptTcpClient(new AsyncCallback(this.OnAccept), TcpServer);
                IsDisposed = false;
                Plugin.OnListenerStart(this);
            }
            catch (Exception ex)
            {
                Dispose();
                String msg = "��ʼ����" + Address.ToString() + ":" + Config.Port + "ʱ����" + ex.Message;
                WriteLog(msg);
                XLog.Trace.WriteLine(msg);
                throw ex;
            }
        }

        /// <summary>
        /// ֹͣ����
        /// </summary>
        public virtual void Stop()
        {
            WriteLog("ֹͣ����" + Address.ToString() + ":" + Config.Port);
            if (Clients != null)
            {
                // Disposeʱ��ر�ÿ���ͻ��˵�����
                for (int i = Clients.Count - 1; i >= 0; i--)
                    (Clients[i] as Session).Dispose();
                Clients.Clear();
            }
            try
            {
                TcpServer.Stop();
                if (TcpServer.Server != null)
                {
                    if (TcpServer.Server.Connected)
                        TcpServer.Server.Shutdown(SocketShutdown.Both);
                    TcpServer.Server.Close();
                }
                Plugin.OnListenerStop(this);
                IsDisposed = true;
            }
            catch (Exception ex)
            {
                String msg = "ֹͣ����" + Address.ToString() + ":" + Config.Port + "ʱ����" + ex.Message;
                WriteLog(msg);
                XLog.Trace.WriteLine(msg);
                throw ex;
            }
        }
        #endregion

        #region �¿ͻ��˵���
        ///<summary>
        /// ���ͻ����ӵȴ�����ʱ�����á�
        /// �رռ�����ʱ��Ҳ�����ã�ֻ����EndAcceptTcpClient�ᴥ���쳣
        /// </summary>
        ///<param name="ar">�첽�����Ľ��</param>
        private void OnAccept(IAsyncResult ar)
        {
            TcpClient tcp = null;
            try
            {
                tcp = TcpServer.EndAcceptTcpClient(ar);
            }
            catch { return; }
            try
            {
                // �����¿�ʼ��������Ҫ�����˱�������߷���
                TcpServer.BeginAcceptTcpClient(new AsyncCallback(this.OnAccept), TcpServer);
            }
            catch { Dispose(); }
            try
            {
                if (tcp != null)
                {
                    NetHelper.SetKeepAlive(tcp.Client, true, 30000, 30000);
                    Session NewClient = OnAccept(tcp);
                    if (Config.IsShow && OnWriteLog != null) NewClient.OnWriteLog += new WriteLogDelegate(WriteLog);
                    NewClient.OnDestroy += new DestroyDelegate(ClientDestroy);
                    NewClient.Listener = this;
                    NewClient.WriteLog("�¿ͻ� (" + tcp.Client.RemoteEndPoint.ToString() + ")");
                    Clients.Add(NewClient);
                    NewClient.Start();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("��������������ʱ���� " + ex.Message);
            }
        }

        private Boolean ClientDestroy(Session session)
        {
            session.WriteLog("�ͻ�(" + session.IPAndPort + ")" + "�˳�");
            //WriteLog("�ͻ�" + session.GUID + "(" + session.IPAndPort + ")" + "�˳�");
            //GC.Collect();
            return Clients.Remove(session);
        }

        /// <summary>
        /// �¿ͻ������ӡ���ͨ���÷���������Ӧ�Ŀͻ���ʵ������
        /// ���磺return new HttpClient(session, ServerPort, ServerAddress);
        /// </summary>
        /// <param name="tcp">��Ӧ��TcpClient</param>
        /// <returns>�ͻ���ʵ��</returns>
        public virtual Session OnAccept(TcpClient tcp)
        {
            return new Session(tcp, Config.ServerPort, Config.ServerAddress);
        }
        #endregion

        #region ��Դ����
        /// <summary>
        /// �Ƿ��Ѿ�����
        /// </summary>
        private bool IsDisposed = false;

        ///<summary>���ټ�����ռ�õ���Դ</summary>
        ///<remarks>ֹͣ������������ <em>����</em> �ͻ�����һ�����٣����󽫲���ʹ��</remarks>
        ///<seealso cref ="System.IDisposable"/>
        public void Dispose()
        {
            if (IsDisposed) return;
            lock (this)
            {
                if (IsDisposed) return;
                IsDisposed = true;
                Stop();
                //GC.Collect();
            }
        }
        ///<summary>��ֹ������</summary>
        ///<remarks>����Dispose����������</remarks>
        ~Listener()
        {
            Dispose();
        }
        #endregion

        #region ��־
        /// <summary>
        /// д��־
        /// </summary>
        /// <param name="msg">��־</param>
        public void WriteLog(String msg)
        {
            if (Config.IsShow && OnWriteLog != null)
            {
                OnWriteLog(msg);
            }
        }
        #endregion
    }
}