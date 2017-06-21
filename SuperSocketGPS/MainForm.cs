using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using SuperSocketGPS.Common;
using SuperSocketGPS.SuperSocket;
using CloseReason = System.Windows.Forms.CloseReason;

namespace SuperSocketGPS
{
    public partial class MainForm : Form
    {
        private bool _startupFlag;
        private readonly string _ipAddr = ConfigurationManager.AppSettings["SocketAddr"];
        private readonly string _port = ConfigurationManager.AppSettings["SocketPort"];
        public static ClientSocket MSocket;
        public static MongoWriter MongoWriter;
        public static DataTable VehicleTable;
        //private readonly ILog _logger=LogManager.GetLogger(typeof(MainForm));
        public MainForm()
        {
            InitializeComponent();
            MSocket = new ClientSocket(_ipAddr, int.Parse(_port));
            MSocket.ConnClose += new ClientSocket.EventBase(socket_ConnClose);
            MongoWriter = new MongoWriter();
            VehicleTable = new DataTable();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            VehicleTable.Columns.Add("车牌号");
            VehicleGridView.DataSource = VehicleTable;
            var column = VehicleGridView.Columns["车牌号"];
            if (column != null) column.Width = 276;
        }
        private void socket_ConnClose(string datagram, string state, object other)
        {
            //this.ShowMessage(datagram);
            if (!_startupFlag)
            {
                ClientSocket cs = other as ClientSocket;
                cs?.TimerConnction(5);
            }
        }
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //注意判断关闭事件Reason来源于窗体按钮，否则用菜单退出时无法退出!
            if (e.CloseReason != CloseReason.UserClosing) return;
            e.Cancel = true; //取消"关闭窗口"事件
            Visible = false;
        }

        private void 显示窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Visible = true;
            WindowState = FormWindowState.Normal;
            Focus();
        }

        private void 打开目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Application.StartupPath);
        }

        private void 退出程序ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Visible = true;
            WindowState = FormWindowState.Normal;
            Focus();
        }

        private void close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void startUp_Click(object sender, EventArgs e)
        {
            if (!_startupFlag)
            {
                Program.Bootstrap = BootstrapFactory.CreateBootstrap();
                if (!Program.Bootstrap.Initialize())
                {
                    MessageBox.Show(@"初始化失败!", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = Program.Bootstrap.Start();

                if (result == StartResult.Failed)
                {
                    MessageBox.Show(@"服务启动失败!", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                MessageBox.Show(@"服务启动成功!", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Common.Extensions.AddLog("服务已启动");
                info.Text = @"服务已启动";

                MSocket.TimerConnction(5);
                MongoWriter.Start();
                BsPackage.StartConsuming();
            }
            else
            {
                Program.Bootstrap.Stop();
                Program.Bootstrap = null;
                info.Text = @"服务已停止";
                Common.Extensions.AddLog("服务已停止");
                MSocket.Close(false);
                MongoWriter.Stop();
                BsPackage.StopConsuming();
                ClearVehicle();
            }

            _startupFlag = !_startupFlag;
        }

        private void clear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            log.Clear();
        }

        public delegate void AddDelegate(string vNum, DataTable dt);

        public static event AddDelegate AddRow;
        
        public static void OnAddRow(string vNum)
        {
            AddRow?.Invoke(vNum,VehicleTable);
        }
        private void ClearVehicle()
        {
            VehicleTable.Rows.Clear();
        }

    }
}
