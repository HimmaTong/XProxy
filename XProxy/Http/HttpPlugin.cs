using System;
using System.Collections.Generic;
using System.Text;
using XProxy.Plugin;
using XProxy.Config;
using System.Reflection;
using System.IO;
using XProxy.Base;

namespace XProxy.Http
{
	/// <summary>
	/// Http�����
	/// </summary>
	public class HttpPlugin : PluginBase
	{
		#region ����
		private Boolean _RequestDelay = false;
		/// <summary>
		/// �����ӳ١�
		/// �յ�һ������������󣬲�ת��������ֱ��ת����
		/// ���Ҫ����������������������ӳ١�
		/// �����ӳٽ���Ҫ�ܴ���ڴ���Դ���������ݡ�
		/// </summary>
		public Boolean RequestDelay { get { return _RequestDelay; } set { _RequestDelay = value; } }

		private Boolean _ResponseDelay = false;
		/// <summary>
		/// ��Ӧ�ӳ١�
		/// �յ�һ����������Ӧ�󣬲�ת��������ֱ��ת����
		/// ���Ҫ����������Ӧ�����������ӳ١�
		/// �����ӳٽ���Ҫ�ܴ���ڴ���Դ���������ݡ�
		/// </summary>
		public Boolean ResponseDelay { get { return _ResponseDelay; } set { _ResponseDelay = value; } }

		private IList<IHttpPlugin> _Plugins;
		/// <summary>
		/// ������ϡ�������¼���ʱ�򣬽������Ⱥ�˳����ò���Ĵ�������
		/// </summary>
		public IList<IHttpPlugin> Plugins { get { return _Plugins; } set { _Plugins = value; } }

		private Boolean _ShowRequest;
		/// <summary>
		/// ��ʾ����
		/// </summary>
		public Boolean ShowRequest { get { return _ShowRequest; } set { _ShowRequest = value; } }

		private Boolean _ShowResponse;
		/// <summary>
		/// ��ʾ��Ӧ
		/// </summary>
		public Boolean ShowResponse { get { return _ShowResponse; } set { _ShowResponse = value; } }
		#endregion

		#region ���ز��
		/// <summary>
		/// ���ز��
		/// </summary>
		/// <param name="configs"></param>
		private void LoadPlugin(IList<PluginConfig> configs)
		{
			if (configs == null || configs.Count < 1) return;
			List<IHttpPlugin> list = new List<IHttpPlugin>();
			foreach (PluginConfig config in configs)
			{
				if (!String.IsNullOrEmpty(config.ClassName))
				{
					Assembly asm;
					if (String.IsNullOrEmpty(config.Path))
						asm = Assembly.GetExecutingAssembly();
					else
					{
						String path = config.Path;
						if (!Path.IsPathRooted(path)) path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
						try
						{
							asm = Assembly.LoadFile(path);
						}
						catch (Exception ex)
						{
							WriteLog(String.Format("����Http�������{0}\n{1}", config, ex));
							continue;
						}
					}
					if (asm != null)
					{
						Type t = asm.GetType(config.ClassName, false);
						if (t == null)
						{
							Type[] ts = asm.GetTypes();
							foreach (Type tt in ts)
							{
								if (tt.Name.Contains(config.ClassName))
								{
									t = tt;
									break;
								}
							}
						}
						if (t != null)
						{
							Object obj = Activator.CreateInstance(t);
							if (obj != null)
							{
								IHttpPlugin p = obj as IHttpPlugin;
								if (p != null)
								{
									p.Config = config;
									list.Add(p);
									WriteLog(String.Format("����Http�����{0}", config));
								}
							}
						}
					}
				}
			}
			Plugins = list;
		}
		#endregion

		#region IPlugin ��Ա
		/// <summary>
		/// ��ʼ��
		/// </summary>
		/// <param name="manager">���������</param>
		public override void OnInit(PluginManager manager)
		{
			Manager = manager;

			LoadPlugin(manager.Listener.Config.HttpPlugins);
			if (Plugins == null || Plugins.Count < 1) return;

			for (int i = 0; i < Plugins.Count; i++)
			{
				Plugins[i].OnWriteLog += new WriteLogDelegate(WriteLog);
				Plugins[i].OnInit(this);
			}

			//���ú�����ʾ�ͻ��˺ͷ��������ݣ�����������ʾ
			ShowRequest = manager.Listener.Config.IsShow && manager.Listener.Config.IsShowClientData;
            ShowResponse = manager.Listener.Config.IsShow && manager.Listener.Config.IsShowServerData;
            manager.Listener.Config.IsShowClientData = false;
            manager.Listener.Config.IsShowServerData = false;
		}

		public override bool OnClientStart(Session session)
		{
			//ʹ��ͬ��
			//session.IsAsync = false;

			return base.OnClientStart(session);
		}

		/// <summary>
		/// �ͻ����������������ʱ������
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="Data">����</param>
		/// <returns>��������������</returns>
		public override byte[] OnClientToServer(Session session, byte[] Data)
		{
			if (Plugins == null || Plugins.Count < 1) return Data;

			//һ���������ASCII���룬����ֱ����ʾ
			if (ShowRequest)
				session.WriteLog("����ͷ��" + Data.Length + "Byte����\n" + Encoding.ASCII.GetString(Data));

			if (HttpHelper.IsHttpRequest(Data))
			{
				int p = ByteHelper.IndexOf(Data, "\r\n\r\n");
				if (p < 0) return null;
				Byte[] bts = ByteHelper.SubBytes(Data, 0, p);

				for (int i = 0; i < Plugins.Count; i++)
				{
					bts = Plugins[i].OnRequestHeader(session, bts);
					//ֱ����ֹ
					if (bts == null || bts.Length < 1) return null;
				}

				//String header = Encoding.ASCII.GetString(bts);

				//for (int i = 0; i < Plugins.Count; i++)
				//{
				//    header = Plugins[i].OnRequestHeader(session, header);
				//    //ֱ����ֹ
				//    if (String.IsNullOrEmpty(header)) return null;
				//}

				//bts = Encoding.ASCII.GetBytes(header + "\r\n\r\n");

				bts = ByteHelper.Cat(bts, Encoding.ASCII.GetBytes("\r\n\r\n"));

				//�Ƿ���������Ҫ����
				if (Data.Length > p + 4)
				{
					Data = ByteHelper.SubBytes(Data, p + 4, -1);
					for (int i = 0; i < Plugins.Count; i++)
					{
						Data = Plugins[i].OnRequestContent(session, Data);
						//ֱ����ֹ
						if (Data == null || Data.Length < 1) return null;
					}
					Data = ByteHelper.Cat(bts, Data);
				}
				else
					Data = bts;
			}
			else
			{
				for (int i = 0; i < Plugins.Count; i++)
				{
					Data = Plugins[i].OnRequestContent(session, Data);
					//ֱ����ֹ
					if (Data == null || Data.Length < 1) return null;
				}
			}

			return Data;
		}

		/// <summary>
		/// ��������ͻ��˷�����ʱ������
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="Data">����</param>
		/// <returns>��������������</returns>
		public override byte[] OnServerToClient(Session session, byte[] Data)
		{
			if (Plugins == null || Plugins.Count < 1) return Data;

			if (HttpHelper.IsHttpResponse(Data))
			{
				int p = ByteHelper.IndexOf(Data, "\r\n\r\n");
				if (p < 0) return null;
				Byte[] bts = ByteHelper.SubBytes(Data, 0, p);

				for (int i = 0; i < Plugins.Count; i++)
				{
					bts = Plugins[i].OnResponseHeader(session, bts);
					//ֱ����ֹ
					if (bts == null || bts.Length < 1) return null;
				}

				String header = Encoding.ASCII.GetString(bts);

				//������Ӧ��ֻ��ֱ����ʾͷ����ֻ��ͷ��������ASCII����
				if (ShowResponse)
					session.WriteLog("��Ӧͷ��" + Data.Length + "Byte����\n" + header);

				//for (int i = 0; i < Plugins.Count; i++)
				//{
				//    header = Plugins[i].OnResponseHeader(session, header);
				//    //ֱ����ֹ
				//    if (String.IsNullOrEmpty(header)) return null;
				//}

				//bts = Encoding.ASCII.GetBytes(header + "\r\n\r\n");

				bts = ByteHelper.Cat(bts, Encoding.ASCII.GetBytes("\r\n\r\n"));

				//�Ƿ���������Ҫ����
				if (Data.Length > p + 4)
				{
					Data = ByteHelper.SubBytes(Data, p + 4, -1);
					for (int i = 0; i < Plugins.Count; i++)
					{
						Data = Plugins[i].OnResponseContent(session, Data);
						//ֱ����ֹ
						if (Data == null || Data.Length < 1) return null;
					}
					Data = ByteHelper.Cat(bts, Data);
				}
				else
					Data = bts;
			}
			else
			{
                if (ShowResponse)
                {
#if !DEBUG
                    session.WriteLog("��Ӧ���ݣ�" + Data.Length + "Byte��");
#else
                    session.WriteLog("��Ӧ���ݣ�" + Data.Length + "Byte����\n" + Encoding.Default.GetString(Data));
#endif
                }
				for (int i = 0; i < Plugins.Count; i++)
				{
					Data = Plugins[i].OnResponseContent(session, Data);
					//ֱ����ֹ
					if (Data == null || Data.Length < 1) return null;
				}
			}

			return Data;
		}

		/// <summary>
		/// Ĭ������
		/// </summary>
		public override PluginConfig DefaultConfig
		{
			get
			{
				PluginConfig pc = base.DefaultConfig;
				pc.Name = "Http���";
				pc.Author = "��ʯͷ";
				return pc;
			}
		}

		#endregion

		#region IDisposable ��Ա
		/// <summary>
		/// �ͷ���Դ
		/// </summary>
		public override void Dispose()
		{
			if (Plugins == null || Plugins.Count < 1) return;

			for (int i = 0; i < Plugins.Count; i++)
			{
				Plugins[i].Dispose();
			}
		}

		#endregion
	}
}