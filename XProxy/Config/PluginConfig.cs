using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using System.ComponentModel.Design;
using System.Windows.Forms;
using XProxy.Plugin;

namespace XProxy.Config
{
	/// <summary>
	/// �������
	/// </summary>
	[Serializable]
	[Description("�������")]
	public class PluginConfig
	{
		private String _Name = "δ����";
		/// <summary>
		/// �����
		/// </summary>
		[ReadOnly(true)]
		[Category("����"), DefaultValue("δ�������"), Description("�����")]
		public String Name { get { return _Name; } set { _Name = value; } }

		private String _Author;
		/// <summary>
		/// �������
		/// </summary>
		[ReadOnly(true)]
		[Category("����"), DefaultValue("����"), Description("�������")]
		public String Author { get { return _Author; } set { _Author = value; } }

		private String _Version;
		/// <summary>
		/// ����汾
		/// </summary>
		[ReadOnly(true)]
		[Category("����"), Description("����汾")]
		public String Version { get { return _Version; } set { _Version = value; } }

		private String _Path;
		/// <summary>
		/// ���·��
		/// </summary>
		[ReadOnly(true)]
		//[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
		[Category("����"), Description("���·��")]
		public String Path { get { return _Path; } set { _Path = value; } }

		private String _ClassName;
		/// <summary>
		/// ����
		/// </summary>
		[ReadOnly(true)]
		[Category("����"), Description("����")]
		public String ClassName { get { return _ClassName; } set { _ClassName = value; } }

		private String _Extend;
		/// <summary>
		/// ��չ��Ϣ1
		/// </summary>
		[Category("��չ"), Description("��չ��Ϣ")]
		public String Extend { get { return _Extend; } set { _Extend = value; } }

		private String _Extend2;
		/// <summary>
		/// ��չ��Ϣ2
		/// </summary>
		[Category("��չ"), Description("��չ��Ϣ��")]
		public String Extend2 { get { return _Extend2; } set { _Extend2 = value; } }

		/// <summary>
		/// ������
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Name);
			if (!String.IsNullOrEmpty(Author)) sb.AppendFormat("��{0}��", Author);
			if (!String.IsNullOrEmpty(ClassName)) sb.AppendFormat("��{0} V{1}", ClassName, Version);
			if (!String.IsNullOrEmpty(Path)) sb.AppendFormat("��{0}", Path);
			//sb.AppendFormat("{0}��{1}����{2} {3}��{4}", Name, Author, ClassName, Version, Path);
			return sb.ToString();
		}
	}

	/// <summary>
	/// ������ϱ༭��
	/// </summary>
	public class PluginsEditor : ArrayEditor
	{
		/// <summary>
		/// ������
		/// </summary>
		/// <param name="type"></param>
		public PluginsEditor(Type type)
			: base(type)
		{
		}

		/// <summary>
		/// ������
		/// </summary>
		/// <param name="itemType"></param>
		/// <returns></returns>
		protected override object CreateInstance(Type itemType)
		{
			PluginSelectorForm form = new PluginSelectorForm();
			form.Plugins = PluginManager.AllPlugins;
			form.ShowDialog();
			if (form.SelectedItem != null)
			{
				PluginConfig pc = form.SelectedItem;
				form.Dispose();
				return pc;
			}
			form.Dispose();
			return null;
		}
	}

	/// <summary>
	/// Http������ϱ༭��
	/// </summary>
	public class HttpPluginsEditor : PluginsEditor
	{
		/// <summary>
		/// ������
		/// </summary>
		/// <param name="type"></param>
		public HttpPluginsEditor(Type type)
			: base(type)
		{
		}

		/// <summary>
		/// ������
		/// </summary>
		/// <param name="itemType"></param>
		/// <returns></returns>
		protected override object CreateInstance(Type itemType)
		{
			PluginSelectorForm form = new PluginSelectorForm();
			form.Plugins = PluginManager.AllHttpPlugins;
			form.ShowDialog();
			if (form.SelectedItem != null)
			{
				PluginConfig pc = form.SelectedItem;
				form.Dispose();
				return pc;
			}
			form.Dispose();
			return null;
		}
	}
}