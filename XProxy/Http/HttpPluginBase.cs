using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Config;
using System.Reflection;
using XProxy.Base;

namespace XProxy.Http
{
    /// <summary>
    /// Http�������
    /// </summary>
    public abstract class HttpPluginBase : IHttpPlugin
    {
        #region ����
        private HttpPlugin _Manager;
        /// <summary>
        /// Http���������
        /// </summary>
        public virtual HttpPlugin Manager { get { return _Manager; } set { _Manager = value; } }

		private PluginConfig _Config;
		/// <summary>
		/// ��ǰ����
		/// </summary>
		public PluginConfig Config { get { return _Config; } set { _Config = value; } }

		/// <summary>
		/// Ĭ������
		/// </summary>
		public virtual PluginConfig DefaultConfig
		{
			get
			{
				PluginConfig pc = new PluginConfig();
				//pc.Name = "Http���";
				pc.Name = this.GetType().Name;
				pc.Author = "����";
				pc.ClassName = this.GetType().FullName;
				//������ڲ��������ֻ��ʾ������������ʾȫ��
				if (Assembly.GetExecutingAssembly() == this.GetType().Assembly)
					pc.ClassName = this.GetType().Name;
				pc.Version = this.GetType().Assembly.GetName().Version.ToString();
				pc.Path = System.IO.Path.GetFileName(this.GetType().Assembly.Location);
				return pc;
			}
		}

        /// <summary>
        /// д��־�¼�
        /// </summary>
        public virtual event WriteLogDelegate OnWriteLog;
        #endregion

        #region IHttpPlugin ��Ա
		#region ��ʼ��
		/// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="manager">���������</param>
        public virtual void OnInit(HttpPlugin manager)
        {
            Manager = manager;
		}
		#endregion

		#region ����ͷ/��Ӧͷ ����
        /// <summary>
        /// ����ͷ
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="Data">����</param>
        /// <returns>����������ͷ����</returns>
        public virtual Byte[] OnRequestHeader(Session session, Byte[] Data)
        {
			String header = Encoding.ASCII.GetString(Data);
			header = OnRequestHeader(session, header);
			return Encoding.ASCII.GetBytes(header);
        }

        /// <summary>
        /// ��Ӧͷ
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="Data">����</param>
        /// <returns>��������Ӧͷ����</returns>
        public virtual Byte[] OnResponseHeader(Session session, Byte[] Data)
        {
			String header = Encoding.ASCII.GetString(Data);
			header = OnResponseHeader(session, header);
			return Encoding.ASCII.GetBytes(header);
		}

		/// <summary>
		/// ����ͷ
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="requestheader">����ͷ</param>
		/// <returns>����������ͷ</returns>
		public virtual string OnRequestHeader(Session session, string requestheader)
		{
			return requestheader;
		}

		/// <summary>
		/// ��Ӧͷ
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="responseheader">��Ӧͷ</param>
		/// <returns>��������Ӧͷ</returns>
		public virtual string OnResponseHeader(Session session, string responseheader)
		{
			return responseheader;
		}
		#endregion

        #region ����/��Ӧ ������Ҫ�����ӳ٣�������ͷ��
        /// <summary>
        /// ����ʱ������������ͷ������Ҫ�����ӳ١�
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="request">����</param>
        /// <returns>����������</returns>
        public virtual string OnRequestBody(Session session, string request)
        {
            return request;
        }

        /// <summary>
        /// ��Ӧʱ������������ͷ������Ҫ�����ӳ١�
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="response">��Ӧ</param>
        /// <returns>��������Ӧ</returns>
        public virtual string OnResponseBody(Session session, string response)
        {
            return response;
        }
        #endregion

        #region ����/��Ӧ ԭʼ���ݴ���������ͷ��
        /// <summary>
        /// ����ʱ������������ͷ��������Ҫ�����ӳ١�
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="Data">����</param>
        /// <returns>����������</returns>
        public virtual byte[] OnRequestContent(Session session, byte[] Data)
        {
			String header = Encoding.ASCII.GetString(Data);
			header = OnRequestBody(session, header);
			return Encoding.ASCII.GetBytes(header);
		}

        /// <summary>
        /// ��Ӧʱ������������ͷ��������Ҫ�����ӳ١�
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="Data">����</param>
        /// <returns>����������</returns>
        public virtual byte[] OnResponseContent(Session session, byte[] Data)
        {
			//String header = Encoding.ASCII.GetString(Data);
			//header = OnResponseBody(session, header);
			//return Encoding.ASCII.GetBytes(header);
			return Data;
		}
        #endregion
        #endregion

        #region IDisposable ��Ա
        /// <summary>
        /// �ͷ���Դ
        /// </summary>
        public virtual void Dispose()
        {
        }
        #endregion

        #region ��־
        /// <summary>
        /// д��־
        /// </summary>
        /// <param name="msg">��־</param>
        public virtual void WriteLog(String msg)
        {
            if (OnWriteLog != null)
            {
                OnWriteLog(msg);
            }
        }
        #endregion

		#region ����
		/// <summary>
        /// �����ء�
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Config == null ? base.ToString() : Config.ToString();
		}
		#endregion
	}
}