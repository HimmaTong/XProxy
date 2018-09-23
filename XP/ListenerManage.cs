using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using XProxy.Config;
using XProxy;
using XProxy.Base;

namespace XP
{
    public partial class ListenerManage : Form
    {
        public ListenerManage()
        {
            InitializeComponent();
        }

        private void ListenerManage_Load(Object sender, EventArgs e)
        {
        }

        private Int32 MsgCount = 0;
        ///<summary>������־</summary>
        ///<remarks>������־��Ϣ��UI��Ϣ��</remarks>
        ///<param name="log">Ҫ�������־��Ϣ</param>
        public void WriteLog(String log)
        {
            //if (!IsShow.Checked) return;
            if (txtLog.InvokeRequired) // �Ƿ���ҪInvoke���ⲿ�߳�ʹ�øú���ʱ��������Ϊ��
            {
                try
                {
                    var d = new WriteLogDelegate(WriteLog);
                    Invoke(d, new Object[] { log });
                }
                catch { }
            }
            else
            {
                MsgCount++;
                if (MsgCount > 100)
                {
                    txtLog.Clear();
                    MsgCount = 0;
                }
                //txtLog.Text += "\r\n" + log;
				txtLog.AppendText("\r\n" + log);
                txtLog.Select(txtLog.TextLength, 0);
                //Ȼ���ƶ���������ʹ�����(text entry point)(��������ڵ�λ�ã���ʾ����
                //����Ҳ���Դﵽ���������·���Ŀ��
                txtLog.ScrollToCaret();
            }
        }

        public void WriteLog(Exception ex)
        {
            var sb = new StringBuilder();
            while (ex != null)
            {
                sb.Append(ex.Message);
                sb.Append(Environment.NewLine);
                ex = ex.InnerException;
            }
            WriteLog(sb.ToString());
        }

        XProxySvc xps = new XProxySvc();
        private void button1_Click(Object sender, EventArgs e)
        {
            if (button1.Text == "��ʼ")
            {
                xps.WriteLogEvent = new WriteLogDelegate(WriteLog);
                xps.StartService();
				if (xps.watcher != null) xps.watcher.EnableRaisingEvents = false;

                button1.Text = "ֹͣ";
            }
            else
            {
                xps.StopService();
                button1.Text = "��ʼ";
            }
        }

        private void ListenerManage_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (button1.Text != "��ʼ") xps.StopService();
            if (Owner != null) Owner.Visible = true;
        }

        private void timer1_Tick(Object sender, EventArgs e)
        {
            if (xps == null) return;
            if (xps.Listeners == null || xps.Listeners.Count < 1) return;

            var count = 0;
            foreach (var item in xps.Listeners)
            {
                if (item.Clients != null) count += item.Clients.Count;
            }
            label2.Text = count.ToString();
        }
    }
}