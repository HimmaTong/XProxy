using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using XProxy.Config;
using System.Net;
using XProxy.Base;

namespace XProxy.Http.Plugin
{
    /// <summary>
    /// ��Ӵ���
    /// </summary>
    public class Indirect : HttpPluginBase
    {
        #region ����
        /// <summary>
        /// Ϊͷ���������ݣ���ֹ��ʶ��
        /// </summary>
        public static String Key = "Get a blow! Love XinXin forever!";
        #endregion

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
            if (!IsLocal(session)) requestheader = HttpHelper.ProcessHttpRequestHeader(session, requestheader);
            return requestheader;
        }

        ///// <summary>
        ///// ��Ӧͷ
        ///// </summary>
        ///// <param name="session">�ͻ���</param>
        ///// <param name="responseheader">��Ӧͷ</param>
        ///// <returns>��������Ӧͷ</returns>
        //public override string OnResponseHeader(Session session, string responseheader)
        //{
        //    if (!IsLocal(session)) return Indirect.Key + responseheader;

        //    if (responseheader.StartsWith(Indirect.Key))
        //        responseheader = responseheader.Substring(Indirect.Key.Length);
        //    return responseheader;
        //}

        /// <summary>
        /// ����ͷ
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="Data">����</param>
        /// <returns>����������ͷ����</returns>
        public override Byte[] OnRequestHeader(Session session, Byte[] Data)
        {
            return Encrypt(Data);
        }

        /// <summary>
        /// ��Ӧͷ
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="Data">����</param>
        /// <returns>��������Ӧͷ����</returns>
        public override Byte[] OnResponseHeader(Session session, Byte[] Data)
        {
            return Encrypt(Data);
        }

        /// <summary>
        /// ����ʱ������������ͷ��������Ҫ�����ӳ١�
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="Data">����</param>
        /// <returns>����������</returns>
        public override Byte[] OnRequestContent(Session session, Byte[] Data)
        {
            return Encrypt(Data);
        }

        /// <summary>
        /// ��Ӧʱ������������ͷ��������Ҫ�����ӳ١�
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="Data">����</param>
        /// <returns>����������</returns>
		public override Byte[] OnResponseContent(Session session, Byte[] Data)
        {
            return Encrypt(Data);
        }

        private static Byte[] Encrypt(Byte[] Data)
        {
            if (Data == null || Data.Length < 1) return null;
            for (var i = 0; i < Data.Length; i++)
            {
                Data[i] ^= (Byte)('X');
            }
            return Data;
        }

        private static List<String> _LocalIPs;
        /// <summary>
        /// ����IP����
        /// </summary>
        protected static List<String> LocalIPs
        {
            get
            {
                if (_LocalIPs == null)
                {
                    var ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                    var list = new List<IPAddress>(ips);
                    list.Add(IPAddress.Any);
                    list.Add(IPAddress.Loopback);
                    _LocalIPs = new List<String>();
                    foreach (var ip in list)
                    {
                        _LocalIPs.Add(ip.ToString());
                    }
                }
                return _LocalIPs;
            }
        }

        /// <summary>
        /// �Ƿ񱾻�IP���÷��������жϼ�Ӵ����ǹ����ڿͻ��˻��Ƿ���ˡ�
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        private Boolean IsLocal(Session session)
        {
            var ip = session.IPAndPort;
            if (ip.IndexOf(":") > 0) ip = ip.Substring(0, ip.IndexOf(":"));
            return LocalIPs.Contains(ip);
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
                pc.Name = "��Ӵ���";
                pc.Author = "��ʯͷ";
                return pc;
            }
        }
        #endregion
    }
}
