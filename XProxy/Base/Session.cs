using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace XProxy.Base
{
    /// <summary>
    /// ������ <c>�ͻ���</c> �Ͽ��������������ʱ�Ļص�����
    /// </summary>
    /// <param name="session">Ҫ�ر������ӵ� <c>�ͻ���</c></param>
    public delegate Boolean DestroyDelegate(Session session);
    /// <summary>
    /// д��־ί��
    /// </summary>
    /// <param name="msg">��־��Ϣ</param>
    public delegate void WriteLogDelegate(String msg);

    ///<summary>���� <c>�ͻ���</c> �Ļ�������������</summary>
    ///<remarks>�ͻ����������ͻ��˺ͷ�����������</remarks>
    public class Session : IDisposable
    {
        #region �¼�
        /// <summary>
        /// �ͻ��˱������¼�
        /// </summary>
        public event DestroyDelegate OnDestroy;

        /// <summary>
        /// д��־�¼�
        /// </summary>
        public event WriteLogDelegate OnWriteLog;
        #endregion

        #region ����
        #region ����
        private Connection _Client;
        /// <summary>
        /// �ͻ�������
        /// </summary>
        public Connection Client
        {
            get { return _Client; }
            set { _Client = value; }
        }

        private Connection _Server;
        /// <summary>
        /// ����������
        /// </summary>
        public Connection Server
        {
            get
            {
                if (_Server == null)
                {
                    TcpClient tcpclient = ConnectServer(true);
                    if (tcpclient != null)
                    {
                        _Server = new Connection(tcpclient);
                        _Server.Name = "������";
                        _Server.Session = this;
                        _Server.OnRead += new ReadCompleteDelegate(OnServerToClient);

                        if (IsAsync)
                        {
                            // �����Ѿ����������뽨��һ���ȴ����������ݵ�ί��
                            _Server.BeginRead();
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ExchangeServer));
                        }
                    }
                }
                return _Server;
            }
            set { _Server = value; }
        }
        #endregion

        #region ������
        private Int32 _ServerPort;
        /// <summary>
        /// �������˿�
        /// </summary>
        public Int32 ServerPort
        {
            get { return _ServerPort; }
            set { _ServerPort = value; }
        }

        private IPAddress _ServerAddress;
        /// <summary>
        /// ��������ַ
        /// </summary>
        public IPAddress ServerAddress
        {
            get { return _ServerAddress; }
            set { _ServerAddress = value; }
        }
        #endregion

        #region ��������
        private String _GUID = NewGuid();
        /// <summary>
        /// ����Ψһ��ʶ
        /// </summary>
        public String GUID
        {
            get { return _GUID; }
        }

        private static Int32 _NewGuid = 1;
        private static String NewGuid()
        {
            return (_NewGuid++).ToString();
        }

        private DateTime _StartTime = DateTime.Now;
        /// <summary>
        /// ��ʼʱ��
        /// </summary>
        public DateTime StartTime
        {
            get { return _StartTime; }
        }

        private Int32 _TimeOut = 30000;
        /// <summary>
        /// ��ʱʱ�䡣Ĭ��30��
        /// </summary>
        public Int32 TimeOut
        {
            get { return _TimeOut; }
            set { _TimeOut = value; }
        }
        #endregion

        #region ��չ
        private String _IPAndPort = String.Empty;
        /// <summary>
        /// IP�˿�
        /// </summary>
        public String IPAndPort
        {
            get { return _IPAndPort; }
            set { _IPAndPort = value; }
        }

        private Listener _Listener;
        /// <summary>
        /// ��Ӧ�ļ�����
        /// </summary>
        public Listener Listener
        {
            get { return _Listener; }
            set { _Listener = value; }
        }

        private Dictionary<String, Object> _Items = new Dictionary<String, Object>();
        /// <summary>
        /// ��������ڴ洢��չ����
        /// </summary>
        public Dictionary<String, Object> Items { get { return _Items; } set { _Items = value; } }

		/// <summary>
		/// ʹ���첽
		/// </summary>
        public Boolean IsAsync
        {
            get { return Listener.Config.IsAsync; }
            set { Listener.Config.IsAsync = value; }
        }
        #endregion
        #endregion

        #region ���캯��
        /// <summary>
        /// ��ʼ��һ���ͻ���ʵ��
        /// </summary>
        /// <param name="tcpclient">���ͻ��˵�����</param>
        public Session(TcpClient tcpclient)
			: this(tcpclient, 0, IPAddress.Any)
		{
		}

        /// <summary>
        /// ��ʼ��һ���ͻ���ʵ��
        /// </summary>
        /// <param name="tcpclient">���ͻ��˵�����</param>
        /// <param name="port">Զ�̷������˿�</param>
        /// <param name="address">Զ�̷�������ַ</param>
        public Session(TcpClient tcpclient, Int32 port, String address)
            : this(tcpclient, port, address == "0.0.0.0" ? IPAddress.Any : Dns.GetHostEntry(address).AddressList[0])
        {
        }

        /// <summary>
        /// ��ʼ��һ���ͻ���ʵ��
        /// </summary>
        /// <param name="tcpclient">���ͻ��˵�����</param>
        /// <param name="port">Զ�̷������˿�</param>
        /// <param name="address">Զ�̷�������ַ</param>
        public Session(TcpClient tcpclient, Int32 port, IPAddress address)
        {
            Client = new Connection(tcpclient);
            Client.Session = this;
            Client.Name = "�ͻ���";
            Client.OnRead += new ReadCompleteDelegate(OnClientToServer);
            IPAndPort = tcpclient.Client.RemoteEndPoint.ToString();

            // �ڷ��ͻ���ջ�����δ��ʱ�����ӳ٣��������Ϸ�������
			// Winsock��Nagle�㷨������С���ݱ��ķ����ٶȣ���ϵͳĬ����ʹ��Nagle�㷨
			// Nagle �㷨ʹ�׽��ֻ������ 200 �����ڵ����ݰ���Ȼ��ʹ��һ�����ݰ��������ǣ��Ӷ�������������
			// ����ر�Nagle�㷨���Լӿ����紦���ٶ�
            tcpclient.NoDelay = true;
            
            ServerPort = port;
            ServerAddress = address;
        }
        #endregion

        #region �������� �Լ� Dispose��Դ����
        /// <summary>
        /// ���ٿͻ���ռ�õ���Դ
        /// </summary>
        /// <param name="msg">��Ϣ</param>
        public void Dispose(String msg)
        {
            WriteLog(msg);
            Dispose();
        }

        private Boolean IsDisposed = false;
        ///<summary>���ٿͻ���ռ�õ���Դ</summary>
        ///<remarks>�ر�������ͻ��˺ͷ�����������</remarks>
        ///<seealso cref ="System.IDisposable"/>
        public void Dispose()
        {
            if (IsDisposed) return;
            lock (this)
            {
                if (IsDisposed) return;
                IsDisposed = true;
                try
                {
                    Client.Dispose();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("�رտͻ������ӳ���" + ex.Message);
                }
                try
                {
                    Server.Dispose();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("�ر�Զ�̷��������ӳ���" + ex.Message);
                }
                if (OnDestroy != null)
                    OnDestroy(this);
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        ~Session()
        {
            Dispose();
        }
        #endregion

        #region ���ݽ�������
        #region ��ʼ
        ///<summary>���ܿͻ��˶����ӣ���ʼ������</summary>
        public void Start()
        {
            if (!Listener.Plugin.OnClientStart(this))
            {
                Dispose();
                return;
            }
			//�첽��ͬ��
            if (IsAsync)
                Client.BeginRead();
            else
            {
                //���ڻ������̣߳����뿪���߳�������ͬ������
                ThreadPool.QueueUserWorkItem(new WaitCallback(ExchangeClient));
            }
        }
        #endregion

		#region ͬ������
		/// <summary>ͬ������</summary>
        protected void ExchangeClient(Object obj)
        {
            try
            {
                while (true)
                {
                    Byte[] buf = Client.Read();
                    if (buf == null || buf.Length < 1) break;
                    buf = OnClientToServer(buf);
                    if (buf == null || buf.Length < 1) break;

                    //д������
                    Server.Write(buf);
                }
                Dispose();
            }
            catch (Exception ex)
            {
                Dispose("ͬ�������ͻ�������ʱ���� " + ex.Message);
            }
        }

        /// <summary>ͬ������</summary>
        protected void ExchangeServer(Object obj)
        {
            try
            {
                while (true)
                {
                    Byte[] buf = Server.Read();
                    if (buf == null || buf.Length < 1) break;
                    buf = OnServerToClient(buf);
                    if (buf == null || buf.Length < 1) break;

                    //д��ͻ���
                    Client.Write(buf);
                }
                Dispose();
            }
            catch (Exception ex)
            {
                Dispose("ͬ���������������ʱ���� " + ex.Message);
            }
        }
        #endregion
		#endregion

		#region ���ݽ����¼�
		/// <summary>
        /// �ͻ����������������ʱ����������ʱӦ�õ��ø÷������Ա�֤�������־�͵��ò����
        /// </summary>
        /// <param name="Data">����</param>
        /// <returns>��������������</returns>
        internal virtual Byte[] OnClientToServer(Byte[] Data)
        {
            if (Data == null || Data.Length < 1) return null;

            Data = Listener.Plugin.OnClientToServer(this, Data);

            if (Listener.Config.IsShowClientData)
                WriteLog("�ͻ������ݣ�" + Data.Length + "Byte����" + GUID + "��");

            //�첽��ʽʱ�����������ڽ����¼��У���Ҫʹ���첽д�룬ʹ���ܾ��������һ�����ݵĽ��չ���֮��
            if (IsAsync)
                Server.BeginWrite(Data);
            else
                Server.Write(Data);

            return Data;
        }

        /// <summary>
        /// ��������ͻ��˷�����ʱ����������ʱӦ�õ��ø÷������Ա�֤�������־�͵��ò����
        /// </summary>
        /// <param name="Data">����</param>
        /// <returns>��������������</returns>
        internal virtual Byte[] OnServerToClient(Byte[] Data)
        {
            if (Data == null || Data.Length < 1) return null;

            Data = Listener.Plugin.OnServerToClient(this, Data);

            if (Listener.Config.IsShowServerData) 
                WriteLog("��������ݣ�" + Data.Length + "Byte����" + GUID + "��");

            if (IsAsync)
                Client.BeginWrite(Data);
            else
                Client.Write(Data);

            return Data;
        }
        #endregion

        #region ���ӷ�����
        /// <summary>
        /// ���ӷ�����
        /// </summary>
        /// <param name="KeepAlive">�Ƿ�ʹ��KeepAlive</param>
        /// <returns>����</returns>
        protected TcpClient ConnectServer(Boolean KeepAlive)
        {
            TcpClient tcpclient = null;
            if (!Listener.Plugin.OnServerStart(this)) return null;
            try
            {
                tcpclient = new TcpClient();
                tcpclient.Connect(ServerAddress, ServerPort);
                // �ڷ��ͻ���ջ�����δ��ʱ�����ӳ�
                tcpclient.NoDelay = true;
                // ��Ҫ�������ӡ���Ҫʱ��Ҫ�Ͽ����ӣ�����Ӱ����������ܡ�
                if (KeepAlive)
                {
                    tcpclient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    NetHelper.SetKeepAlive(tcpclient.Client, true, TimeOut, TimeOut);
                }
            }
            catch (Exception ex)
            {
                Dispose("���ӷ��������� " + ex.Message);
                return null;
            }
            return tcpclient;
        }
        #endregion

        #region ��������
        ///<summary>������־</summary>
        ///<remarks>������־��Ϣ��UI��Ϣ��</remarks>
        ///<param name="log">Ҫ�������־��Ϣ</param>
        public void WriteLog(string log)
        {
            if (OnWriteLog != null)
            {
                //ʹ���̳߳��߳�д��־
                ThreadPool.QueueUserWorkItem(new WaitCallback(WriteLogCallBack), String.Format("[{2}]{0,6} {1}", GUID, log, Listener.Config.Name));
            }
        }
        private void WriteLogCallBack(Object msg)
        {
            if (msg == null || String.IsNullOrEmpty(msg.ToString())) return;
            OnWriteLog(msg.ToString());
        }
        #endregion
    }
}