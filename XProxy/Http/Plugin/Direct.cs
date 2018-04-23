using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using XProxy.Config;
using XProxy.Base;

namespace XProxy.Http.Plugin
{
    /// <summary>
    /// ֱ�Ӵ���
    /// </summary>
    public class Direct : HttpPluginBase
    {
        #region IHttpPlugin ��Ա
        #region ����ͷ/��Ӧͷ ����
        /// <summary>
        /// ����ͷ
        /// </summary>
        /// <param name="session">�ͻ���</param>
        /// <param name="requestheader">����ͷ</param>
        /// <returns>����������ͷ</returns>
        public override string OnRequestHeader(Session session, string requestheader)
        {
            return HttpHelper.ProcessHttpRequestHeader(session, requestheader);
        }
        #endregion

        /// <summary>
        /// Ĭ������
        /// </summary>
        public override PluginConfig DefaultConfig
        {
            get
            {
                PluginConfig pc = base.DefaultConfig;
                pc.Name = "ֱ�Ӵ���";
                pc.Author = "��ʯͷ";
                return pc;
            }
        }
        #endregion
    }
}
