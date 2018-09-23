using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using XProxy.Config;
using System.Reflection;
using System.IO;
using XProxy.Http;
using XProxy.Base;

namespace XProxy.Plugin
{
	/// <summary>
	/// ���������
	/// </summary>
	public class PluginManager
	{
		#region ����
		private IList<IPlugin> _Plugins;
		/// <summary>
		/// ������ϡ�������¼���ʱ�򣬽������Ⱥ�˳����ò���Ĵ�������
		/// </summary>
		public IList<IPlugin> Plugins { get { return _Plugins; } set { _Plugins = value; } }

		private Listener _Listener;
		/// <summary>
		/// ��ǰ������
		/// </summary>
		public Listener Listener { get { return _Listener; } set { _Listener = value; } }

		/// <summary>
		/// д��־�¼�
		/// </summary>
		public event WriteLogDelegate OnWriteLog;
		#endregion

		#region ���ز��
		/// <summary>
		/// ���ز��
		/// </summary>
		/// <param name="configs"></param>
		private void LoadPlugin(IList<PluginConfig> configs)
		{
			if (configs == null || configs.Count < 1) return;
			var list = new List<IPlugin>();
			foreach (var config in configs)
			{
				if (!String.IsNullOrEmpty(config.ClassName))
				{
					Assembly asm;
					if (String.IsNullOrEmpty(config.Path))
						asm = Assembly.GetExecutingAssembly();
					else
					{
						var path = config.Path;
						if (!Path.IsPathRooted(path)) path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
						try
						{
							asm = Assembly.LoadFile(path);
						}
						catch (Exception ex)
						{
							WriteLog(String.Format("���ز������{0}\n{1}", config, ex.ToString()));
							continue;
						}
					}
					if (asm != null)
					{
						var t = asm.GetType(config.ClassName, false);
						if (t == null)
						{
							var ts = asm.GetTypes();
							foreach (var tt in ts)
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
							var obj = Activator.CreateInstance(t);
							if (obj != null)
							{
								var p = obj as IPlugin;
								if (p != null)
								{
									p.Config = config;
									list.Add(p);
									WriteLog(String.Format("���ز����{0}", config));
								}
							}
						}
					}
				}
			}
			Plugins = list;
		}
		#endregion

		#region �¼�
		/// <summary>
		/// ��ʼ��
		/// </summary>
		/// <param name="listener">������</param>
		public void OnInit(Listener listener)
		{
			LoadPlugin(listener.Config.Plugins);
			if (Plugins == null || Plugins.Count < 1) return;

			for (var i = 0; i < Plugins.Count; i++)
			{
				Plugins[i].OnWriteLog += new WriteLogDelegate(WriteLog);
				Plugins[i].OnInit(this);
			}
		}

		/// <summary>
		/// �ͷ���Դ
		/// </summary>
		public void OnDispose()
		{
			if (Plugins == null || Plugins.Count < 1) return;

			for (var i = 0; i < Plugins.Count; i++)
			{
				Plugins[i].Dispose();
			}
		}

		/// <summary>
		/// ��ʼ����
		/// </summary>
		/// <param name="listener">������</param>
		public void OnListenerStart(Listener listener)
		{
			if (Plugins == null || Plugins.Count < 1) return;

			for (var i = 0; i < Plugins.Count; i++)
			{
				Plugins[i].OnListenerStart(listener);
			}
		}

		/// <summary>
		/// ֹͣ����
		/// </summary>
		/// <param name="listener">������</param>
		public void OnListenerStop(Listener listener)
		{
			if (Plugins == null || Plugins.Count < 1) return;

			for (var i = 0; i < Plugins.Count; i++)
			{
				Plugins[i].OnListenerStop(listener);
			}
		}

		/// <summary>
		/// ��һ����ͻ��˷�����ʱ������
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <returns>�Ƿ�����ͨ��</returns>
		public Boolean OnClientStart(Session session)
		{
			if (Plugins == null || Plugins.Count < 1) return true;

			for (var i = 0; i < Plugins.Count; i++)
			{
				if (!Plugins[i].OnClientStart(session)) return false;
			}
			return true;
		}

		/// <summary>
		/// ����Զ�̷�����ʱ������
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <returns>�Ƿ�����ͨ��</returns>
		public Boolean OnServerStart(Session session)
		{
			if (Plugins == null || Plugins.Count < 1) return true;

			for (var i = 0; i < Plugins.Count; i++)
			{
				if (!Plugins[i].OnServerStart(session)) return false;
			}
			return true;
		}
		#endregion

		#region ���ݽ����¼�
		/// <summary>
		/// �ͻ����������������ʱ������
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="Data">����</param>
		/// <returns>���������Data�����е����ݴ�С</returns>
		public Byte[] OnClientToServer(Session session, Byte[] Data)
		{
			if (Plugins == null || Plugins.Count < 1) return Data;
			if (Data == null || Data.Length < 1) return null;

			for (var i = 0; i < Plugins.Count; i++)
			{
				Data = Plugins[i].OnClientToServer(session, Data);
				if (Data == null || Data.Length < 1) return null;
			}
			return Data;
		}

		/// <summary>
		/// ��������ͻ��˷�����ʱ������
		/// </summary>
		/// <param name="session">�ͻ���</param>
		/// <param name="Data">����</param>
		/// <returns>���������Data�����е����ݴ�С</returns>
		public Byte[] OnServerToClient(Session session, Byte[] Data)
		{
			if (Plugins == null || Plugins.Count < 1) return Data;
			if (Data == null || Data.Length < 1) return null;

			for (var i = 0; i < Plugins.Count; i++)
			{
				Data = Plugins[i].OnServerToClient(session, Data);
				if (Data == null || Data.Length < 1) return null;
			}
			return Data;
		}
		#endregion

		#region ��־
		/// <summary>
		/// д��־
		/// </summary>
		/// <param name="msg">��־</param>
		public void WriteLog(String msg)
		{
			if (OnWriteLog != null)
			{
				if (Listener == null)
					OnWriteLog(msg);
				else
					OnWriteLog(String.Format("[{0}] {1}", Listener.Config.Name, msg));
			}
		}
		#endregion

		#region ��̬ �������
		private static PluginConfig[] _AllPlugins;
		/// <summary>
		/// ���в��
		/// </summary>
		public static PluginConfig[] AllPlugins
		{
			get
			{
				LoadAllAssembly();

				var list = new List<PluginConfig>();
				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var t in asm.GetTypes())
					{
						if (t.GetInterface(typeof(IPlugin).Name) != null)
						{
							try
							{
								var ip = asm.CreateInstance(t.FullName) as IPlugin;
								if (ip != null) list.Add(ip.DefaultConfig);
							}
							catch { }
						}
					}
				}
				_AllPlugins = new PluginConfig[list.Count];
				list.CopyTo(_AllPlugins);

				return _AllPlugins;
			}
		}

		private static PluginConfig[] _AllHttpPlugins;
		/// <summary>
		/// ����Http���
		/// </summary>
		public static PluginConfig[] AllHttpPlugins
		{
			get
			{
				LoadAllAssembly();

				var list = new List<PluginConfig>();
				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var t in asm.GetTypes())
					{
						//����IsAbstract���жϣ���ֹ�ѳ���������ʶ��Ϊ���
						if (!t.IsAbstract && t.GetInterface(typeof(IHttpPlugin).Name) != null)
						{
							try
							{
								var ip = asm.CreateInstance(t.FullName) as IHttpPlugin;
								if (ip != null) list.Add(ip.DefaultConfig);
							}
							catch { }
						}
					}
				}
				_AllHttpPlugins = new PluginConfig[list.Count];
				list.CopyTo(_AllHttpPlugins);

				return _AllHttpPlugins;
			}
		}

		private static void LoadAllAssembly()
		{
			var path = AppDomain.CurrentDomain.BaseDirectory;
			//path = Path.Combine(path, "Plugins");
			if (!Directory.Exists(path)) return;

            //����ʹ��AllDirectories�����ܶ���Ŀ¼��ʱ�򣬻Ῠ������
			var fs = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
			if (fs == null || fs.Length < 1) return;

			foreach (var s in fs)
			{
				try
				{
					Assembly.LoadFile(s);
				}
				catch { }
			}
		}
		#endregion
	}
}