using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Config;
using XProxy.Base;

namespace XProxy.Plugin
{
    /// <summary>
    /// ����ӿ�
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="manager">���������</param>
        void OnInit(PluginManager manager);

        /// <summary>
        /// ��ʼ����
        /// </summary>
        /// <param name="listener">������</param>
        void OnListenerStart(Listener listener);

        /// <summary>
        /// ֹͣ����
        /// </summary>
        /// <param name="listener">������</param>
        void OnListenerStop(Listener listener);

        /// <summary>
        /// ��һ����ͻ��˷�����ʱ������
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <returns>�Ƿ�����ͨ��</returns>
        Boolean OnClientStart(Session session);

        /// <summary>
        /// ����Զ�̷�����ʱ������
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <returns>�Ƿ�����ͨ��</returns>
        Boolean OnServerStart(Session session);

        /// <summary>
        /// �ͻ����������������ʱ������
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="Data">����</param>
        /// <returns>��������������</returns>
        Byte[] OnClientToServer(Session session, Byte[] Data);

        /// <summary>
        /// ��������ͻ��˷�����ʱ������
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="Data">����</param>
        /// <returns>��������������</returns>
        Byte[] OnServerToClient(Session session, Byte[] Data);

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
    }
}