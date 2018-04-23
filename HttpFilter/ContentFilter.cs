using System;
using System.Text;
using System.Text.RegularExpressions;
using XProxy.Base;
using XProxy.Config;
using XProxy.Http;

namespace HttpFilter
{
    /// <summary>
    /// ���ݹ�����
    /// </summary>
    public class ContentFilter : HttpPluginBase
    {
        /// <summary>
        /// ������Ӧ������ݡ�
        /// ���ڵ��ӳٴ���û�����ã�����������ӳٴ����Ϳ���ֱ��OnResponse����һ���汾��
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public override byte[] OnResponseContent(Session client, byte[] Data)
        {
            //Ҫע����롣��һ����gb2312���롣V1.0�������Զ�ʶ�����Ĺ��ܣ����ڻ�û��Ǩ�ƹ�����
            String str = Encoding.Default.GetString(Data);
            str = Regex.Replace(str, @"\w+\.baidu\.com",
                Manager.Manager.Listener.Address.ToString() + ":" +
                Manager.Manager.Listener.Config.Port.ToString());
            return Encoding.Default.GetBytes(str);
        }

        public override PluginConfig DefaultConfig
        {
            get
            {
				PluginConfig pc = base.DefaultConfig;
                pc.Name = "���ݹ�����";
                pc.Author = "��ʯͷ";
                return pc;
            }
        }
    }
}
