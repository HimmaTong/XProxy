using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Config;
using System.Reflection;
using XProxy.Base;

namespace XProxy.Http.Plugin
{
    /// <summary>
    /// �������
    /// </summary>
    public class Reverse : HttpPluginBase
    {
        #region IHttpPlugin ��Ա
        #region ����ͷ/��Ӧͷ ����
        /// <summary>
        /// ����ͷ
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="requestheader">����ͷ</param>
        /// <returns>����������ͷ</returns>
        public override String OnRequestHeader(Session session, String requestheader)
        {
            return ProcessHttpRequestHeader(requestheader);
        }
        #endregion

        /// <summary>
        /// Ĭ������
        /// </summary>
        public override PluginConfig DefaultConfig
        {
            get
            {
                var pc = base.DefaultConfig;
                pc.Name = "�������";
                pc.Author = "��ʯͷ";
                return pc;
            }
        }
        #endregion

        #region ��չ
        /// <summary>
        /// ����HTTP����ͷ����HTTP����ʹ��
        /// </summary>
        /// <param name="header">����</param>
        /// <returns>����������</returns>
        public String ProcessHttpRequestHeader(String header)
        {
            if (String.IsNullOrEmpty(header)) return header;

            // �ҵ�HTTPͷ���������������ַ������HOST
            var headers = header.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (headers == null) return header;

            // ����ƴ��HTTP����ͷ
            var sb = new StringBuilder();
            foreach (var s in headers)
            {
                var ss = s.ToLower();
                if (ss.StartsWith("host:"))
                {
                    sb.Append("Host: ");
                    sb.Append(Manager.Manager.Listener.Config.ServerAddress);
                }
                else
                {
                    sb.Append(s);
                }
                sb.Append("\r\n");
            }
            // �����Լ��ı�ʶ
            sb.Append("X-Proxy: NewLifeXProxy");

            return sb.ToString();
        }
        #endregion
    }
}