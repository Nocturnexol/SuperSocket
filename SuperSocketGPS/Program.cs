using System;
using System.Windows.Forms;
using SuperSocket.SocketBase;

namespace SuperSocketGPS
{
    static class Program
    {
        // 添加静态字段AppForm，用于记录启动窗体
        public static Form AppForm;
        // 添加静态字段Bootstrap，用于获取AppSession
        public static IBootstrap Bootstrap = null;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(AppForm = new MainForm { StartPosition = FormStartPosition.CenterScreen });
        }
    }
}
