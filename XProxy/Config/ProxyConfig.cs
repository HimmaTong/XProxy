using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace XProxy.Config
{
    /// <summary>
    /// ��������
    /// </summary>
    [Serializable]
    public class ProxyConfig
    {
        #region ����
        private ListenerConfig[] _Listeners;
        /// <summary>
        /// ����������
        /// </summary>
        public ListenerConfig[] Listeners { get { return _Listeners; } set { _Listeners = value; } }
        #endregion

        #region ���캯��
        //public static List<ListenerConfig> list = new List<ListenerConfig>();
        //public ProxyConfig()
        //{
        //    list.Add(this);
        //}

        private static ProxyConfig _Instance;
        /// <summary>
        /// Ĭ��ʵ��
        /// </summary>
        public static ProxyConfig Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = Load(null);
                    //if (_Instance != null) _Instance.IsSaved = false;
                }
                return _Instance;
            }
            set
            {
                _Instance = value;
            }
        }

        ///// <summary>
        ///// BindSourceĬ�ϻ��ʼ��һ��Config�࣬Ȼ��ŵ���GetList������
        ///// ���ԣ�Ӧ����ֻ�е�����Load���Ǹ���������Save��
        ///// </summary>
        //[NonSerialized]
        //private bool IsSaved = true;

        /// <summary>
        /// ����ʱ����
        /// </summary>
        ~ProxyConfig()
        {
            //if (!IsSaved)
            //{
            //    Save(null, this);
            //    IsSaved = true;
            //}
        }
        #endregion

        #region ���ر���
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="filename">�ļ���</param>
        /// <returns></returns>
		public static ProxyConfig Load(String filename)
		{
			if (String.IsNullOrEmpty(filename)) filename = DefaultFile;
			ProxyConfig config = new ProxyConfig();
			if (File.Exists(filename))
			{
				using (StreamReader sr = new StreamReader(filename, Encoding.UTF8))
				{
					XmlSerializer xs = new XmlSerializer(typeof(ProxyConfig));
					try
					{
						config = xs.Deserialize(sr) as ProxyConfig;
					}
					catch (Exception ex)
					{
						XLog.Trace.WriteLine("���ش��������ļ�" + filename + "ʱ��������\n" + ex.ToString());
					}
					sr.Close();
				}
			}
			return config;
		}

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="filename">�ļ���</param>
        /// <param name="config">Ҫ����Ķ���</param>
        public static void Save(String filename, ProxyConfig config)
        {
            if (config == null) return;
            if (String.IsNullOrEmpty(filename)) filename = DefaultFile;
            using (StreamWriter sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ProxyConfig));
                    xs.Serialize(sw, config);
                }
				catch (Exception ex)
				{
					XLog.Trace.WriteLine("������������ļ�" + filename + "ʱ��������\n" + ex.ToString());
				}
				sw.Close();
            }
        }

        /// <summary>
        /// Ĭ�������ļ�
        /// </summary>
        public static String DefaultFile
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Proxy.xml");
            }
        }
        #endregion
    }
}
