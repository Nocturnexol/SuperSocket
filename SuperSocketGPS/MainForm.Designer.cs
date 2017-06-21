namespace SuperSocketGPS
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.MaximizeBox = false;//使最大化窗口失效
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.显示窗口ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开目录ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出程序ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.startUp = new System.Windows.Forms.Button();
            this.close = new System.Windows.Forms.Button();
            this.info = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.log = new System.Windows.Forms.RichTextBox();
            this.clear = new System.Windows.Forms.LinkLabel();
            this.VehicleGridView = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VehicleGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.显示窗口ToolStripMenuItem,
            this.打开目录ToolStripMenuItem,
            this.退出程序ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(125, 70);
            // 
            // 显示窗口ToolStripMenuItem
            // 
            this.显示窗口ToolStripMenuItem.Name = "显示窗口ToolStripMenuItem";
            this.显示窗口ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.显示窗口ToolStripMenuItem.Text = "显示窗口";
            this.显示窗口ToolStripMenuItem.Click += new System.EventHandler(this.显示窗口ToolStripMenuItem_Click);
            // 
            // 打开目录ToolStripMenuItem
            // 
            this.打开目录ToolStripMenuItem.Name = "打开目录ToolStripMenuItem";
            this.打开目录ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.打开目录ToolStripMenuItem.Text = "打开目录";
            this.打开目录ToolStripMenuItem.Click += new System.EventHandler(this.打开目录ToolStripMenuItem_Click);
            // 
            // 退出程序ToolStripMenuItem
            // 
            this.退出程序ToolStripMenuItem.Name = "退出程序ToolStripMenuItem";
            this.退出程序ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.退出程序ToolStripMenuItem.Text = "退出程序";
            this.退出程序ToolStripMenuItem.Click += new System.EventHandler(this.退出程序ToolStripMenuItem_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // startUp
            // 
            this.startUp.Location = new System.Drawing.Point(465, 292);
            this.startUp.Name = "startUp";
            this.startUp.Size = new System.Drawing.Size(105, 39);
            this.startUp.TabIndex = 1;
            this.startUp.Text = "启动/关闭服务";
            this.startUp.UseVisualStyleBackColor = true;
            this.startUp.Click += new System.EventHandler(this.startUp_Click);
            // 
            // close
            // 
            this.close.Location = new System.Drawing.Point(576, 292);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(97, 39);
            this.close.TabIndex = 2;
            this.close.Text = "关闭";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // info
            // 
            this.info.AutoSize = true;
            this.info.Font = new System.Drawing.Font("宋体", 10.25F);
            this.info.Location = new System.Drawing.Point(9, 303);
            this.info.Name = "info";
            this.info.Size = new System.Drawing.Size(77, 14);
            this.info.TabIndex = 3;
            this.info.Text = "服务未启动";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(344, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "系统日志";
            // 
            // log
            // 
            this.log.Location = new System.Drawing.Point(346, 24);
            this.log.Name = "log";
            this.log.ReadOnly = true;
            this.log.Size = new System.Drawing.Size(327, 258);
            this.log.TabIndex = 5;
            this.log.Text = "";
            // 
            // clear
            // 
            this.clear.AutoSize = true;
            this.clear.Location = new System.Drawing.Point(620, 9);
            this.clear.Name = "clear";
            this.clear.Size = new System.Drawing.Size(53, 12);
            this.clear.TabIndex = 6;
            this.clear.TabStop = true;
            this.clear.Text = "清空日志";
            this.clear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.clear_LinkClicked);
            // 
            // VehicleGridView
            // 
            this.VehicleGridView.AllowUserToAddRows = false;
            this.VehicleGridView.AllowUserToDeleteRows = false;
            this.VehicleGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.VehicleGridView.Location = new System.Drawing.Point(12, 24);
            this.VehicleGridView.Name = "VehicleGridView";
            this.VehicleGridView.ReadOnly = true;
            this.VehicleGridView.RowTemplate.Height = 23;
            this.VehicleGridView.Size = new System.Drawing.Size(320, 258);
            this.VehicleGridView.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "车辆列表";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(685, 343);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.VehicleGridView);
            this.Controls.Add(this.clear);
            this.Controls.Add(this.log);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.info);
            this.Controls.Add(this.close);
            this.Controls.Add(this.startUp);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "问题车辆监控平台";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.VehicleGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 显示窗口ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 打开目录ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 退出程序ToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button startUp;
        private System.Windows.Forms.Button close;
        private System.Windows.Forms.Label info;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.RichTextBox LogBox { get { return log; } }
        private System.Windows.Forms.RichTextBox log;
        private System.Windows.Forms.LinkLabel clear;
        private System.Windows.Forms.DataGridView VehicleGridView;
        private System.Windows.Forms.Label label2;
    }
}

