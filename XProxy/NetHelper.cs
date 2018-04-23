using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace XProxy
{
    /// <summary>
    /// ��������
    /// </summary>
    public static class NetHelper
    {
        /// <summary>
        /// ���ó�ʱ���ʱ��ͼ����
        /// </summary>
        /// <param name="socket">Ҫ���õ�Socket����</param>
        /// <param name="iskeepalive">�Ƿ�����Keep-Alive</param>
        /// <param name="starttime">�೤ʱ���ʼ��һ��̽�⣨��λ�����룩</param>
        /// <param name="interval">̽��ʱ��������λ�����룩</param>
        public static void SetKeepAlive(Socket socket,Boolean iskeepalive, Int32 starttime, Int32 interval)
        {
            uint dummy = 0;
            byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
            BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
            socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
        //struct tcp_keepalive
        //{
        //    u_long onoff; //�Ƿ�����Keep-Alive
        //    u_long keepalivetime; //�೤ʱ���ʼ��һ��̽�⣨��λ�����룩
        //    u_long keepaliveinterval; //̽��ʱ��������λ�����룩
        //};

        /// <summary>
        /// �����ջ
        /// </summary>
        public static void OutStack()
        {
            StackTrace st = new StackTrace(1);
            StringBuilder sb = new StringBuilder();
            StackFrame[] sfs = st.GetFrames();
            foreach (StackFrame sf in sfs)
            {
                sb.Append(sf.GetMethod().DeclaringType.FullName);
                sb.Append(".");
                sb.Append(sf.GetMethod().Name);
                String s = sf.GetFileName();
                if (!String.IsNullOrEmpty(s))
                {
                    sb.Append("(");
                    sb.Append(s);
                    sb.Append(",");
                    sb.Append(sf.GetFileLineNumber().ToString());
                    sb.Append("��)");
                }
                sb.Append(Environment.NewLine);
            }
            XLog.Trace.WriteLine(sb.ToString());
        }
    }
}