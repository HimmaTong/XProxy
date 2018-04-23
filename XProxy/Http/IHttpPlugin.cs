using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Plugin;
using XProxy.Config;
using XProxy.Base;

namespace XProxy.Http
{
	/// <summary>
	/// Http����ӿ�
	/// </summary>
	public interface IHttpPlugin : IDisposable
	{
		#region ��ʼ��
		/// <summary>
		/// ��ʼ��
		/// </summary>
		/// <param name="manager">���������</param>
		void OnInit(HttpPlugin manager);
		#endregion

		#region ����ͷ/��Ӧͷ ����
		///// <summary>
		///// ����ͷ
		///// </summary>
		///// <param name="session">�ͻ���</param>
		///// <param name="requestheader">����ͷ</param>
		///// <returns>����������ͷ</returns>
		//String OnRequestHeader(Session session, String requestheader);

		///// <summary>
		///// ��Ӧͷ
		///// </summary>
		///// <param name="session">�ͻ���</param>
		///// <param name="responseheader">��Ӧͷ</param>
		///// <returns>��������Ӧͷ</returns>
		//String OnResponseHeader(Session session, String responseheader);

		/// <summary>
		/// ����ͷ
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="Data">����</param>
		/// <returns>����������ͷ����</returns>
		Byte[] OnRequestHeader(Session session, Byte[] Data);

		/// <summary>
		/// ��Ӧͷ
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="Data">����</param>
		/// <returns>��������Ӧͷ����</returns>
		Byte[] OnResponseHeader(Session session, Byte[] Data);
		#endregion

		#region ����/��Ӧ ������Ҫ�����ӳ�
		///// <summary>
		///// ����ʱ������������ͷ������Ҫ�����ӳ١�
		///// </summary>
		///// <param name="session">�ͻ���</param>
		///// <param name="request">����</param>
		///// <returns>����������</returns>
		//String OnRequestContent(Session session, String request);

		///// <summary>
		///// ��Ӧʱ������������ͷ������Ҫ�����ӳ١�
		///// </summary>
		///// <param name="session">�ͻ���</param>
		///// <param name="response">��Ӧ</param>
		///// <returns>��������Ӧ</returns>
		//String OnResponseContent(Session session, String response);
		#endregion

		#region ����/��Ӧ ԭʼ���ݴ���
		/// <summary>
		/// ����ʱ������������ͷ��������Ҫ�����ӳ١�
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="Data">����</param>
		/// <returns>����������</returns>
		Byte[] OnRequestContent(Session session, Byte[] Data);

		/// <summary>
		/// ��Ӧʱ������������ͷ��������Ҫ�����ӳ١�
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="Data">����</param>
		/// <returns>����������</returns>
		Byte[] OnResponseContent(Session session, Byte[] Data);
		#endregion

		#region ����
		/// <summary>
		/// ��ǰ����
		/// </summary>
		PluginConfig Config { get; set; }

		/// <summary>
		/// Ĭ������
		/// </summary>
		PluginConfig DefaultConfig { get; }

		/// <summary>
		/// д��־
		/// </summary>
		event WriteLogDelegate OnWriteLog;
		#endregion
	}
}