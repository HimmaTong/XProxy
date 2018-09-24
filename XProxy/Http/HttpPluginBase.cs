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
        /// <summary>
        /// ��ǰ����
        /// </summary>
        public PluginConfig Config { get; set; }

        /// <summary>
        /// Ĭ������
        /// </summary>
        public virtual PluginConfig DefaultConfig
		{
			get
			{
				var pc = new PluginConfig();
				//pc.Name = "Http���";
				pc.Name = GetType().Name;
				pc.Author = "����";
				pc.ClassName = GetType().FullName;
				//������ڲ��������ֻ��ʾ������������ʾȫ��
				if (Assembly.GetExecutingAssembly() == GetType().Assembly)
					pc.ClassName = GetType().Name;
				pc.Version = GetType().Assembly.GetName().Version.ToString();
				pc.Path = System.IO.Path.GetFileName(GetType().Assembly.Location);
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
			var header = Encoding.ASCII.GetString(Data);
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
			var header = Encoding.ASCII.GetString(Data);
			header = OnResponseHeader(session, header);
			return Encoding.ASCII.GetBytes(header);
		}

		/// <summary>
		/// ����ͷ
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="requestheader">����ͷ</param>
		/// <returns>����������ͷ</returns>
		public virtual String OnRequestHeader(Session session, String requestheader)
		{
			return requestheader;
		}

		/// <summary>
		/// ��Ӧͷ
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="responseheader">��Ӧͷ</param>
		/// <returns>��������Ӧͷ</returns>
		public virtual String OnResponseHeader(Session session, String responseheader)
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
        public virtual String OnRequestBody(Session session, String request)
        {
            return request;
        }

        /// <summary>
        /// ��Ӧʱ������������ͷ������Ҫ�����ӳ١�
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="response">��Ӧ</param>
        /// <returns>��������Ӧ</returns>
        public virtual String OnResponseBody(Session session, String response)
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
        public virtual Byte[] OnRequestContent(Session session, Byte[] Data)
        {
			var header = Encoding.ASCII.GetString(Data);
			header = OnRequestBody(session, header);
			return Encoding.ASCII.GetBytes(header);
		}

        /// <summary>
        /// ��Ӧʱ������������ͷ��������Ҫ�����ӳ١�
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="Data">����</param>
        /// <returns>����������</returns>
        public virtual Byte[] OnResponseContent(Session session, Byte[] Data)
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
        public override String ToString()
        {
            return Config == null ? base.ToString() : Config.ToString();
		}
		#endregion
	}
}