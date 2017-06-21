using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;

//using DKDataForwardNew.Models;

namespace SuperSocketGPS.Common
{
    /// <summary>
    /// 线路客户端
    /// </summary>
    public class ClientSocket
    {
        /// <summary>
        /// 全局变量定义
        /// </summary>
        public Encoding encoding = Encoding.GetEncoding("gbk"); // 系统默认编码
        private string _IPAddress; // IP地址
        private int _Port; // 链接的端口号
        public Socket clientSocket; // socket 对象
        private byte[] _dataBuffer; // 缓冲区
        public Boolean _IsRun = false; // socket 状态
        public Boolean _AllowConn = true; // socket 是否允许定时重连
        public string _NoFullPack = string.Empty; // 不完整的半包
        private ConcurrentQueue<string> msgQueue = null;  // 存储发送到当前目标地址的队列
        private DateTime _LastKeepAliveTime; // 最后接收到的心跳时间

        public long SendCount = 0; // 记录每天发送记录数
        public long BeforeOneMinuiteCount = 0;
        public string OverQueueDataFolderPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\OverQueueData\\"; // 保存溢出的队列消息
        //private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string FD { get; private set; }
        public string LastConnectTime { get; private set; }
        public int MessageQueue { get { return this.msgQueue.Count; } }
        ManualResetEvent connectSet = new ManualResetEvent(false);//连接锁

        public ClientSocket(string ServerIP, int Port)
        {
            this._IPAddress = ServerIP;
            this._Port = Port;
            msgQueue = new ConcurrentQueue<string>();
            //LogManager.CreateFolder(OverQueueDataFolderPath);

            Thread ThreadTimerConn = new Thread(Connection);
            ThreadTimerConn.IsBackground = true;
            ThreadTimerConn.Start();
        }

        /// <summary>
        /// 定时链接
        /// </summary>
        /// <param name="Timer">秒</param>
        public void TimerConnction(int Timer)
        {
            _AllowConn = true;//允许定时重连
            connectSet.Set();
        }

        /// <summary>
        /// 添加数据到队列 返回队列中的值
        /// </summary>
        /// <param name="pp"></param>
        public int AddQueue(string bts)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(bts))
                {
                    //只有socket 在链接成功的状态才对队列中添加数据
                    if (_IsRun)
                    {
                        msgQueue.Enqueue(bts);
                    }
                }
            }
            catch
            { }
            return msgQueue.Count;
        }

        /// <summary>
        /// 获取数据队列大小
        /// </summary>
        /// <returns></returns>
        public int GetQueueSize()
        {
            return msgQueue.Count;
        }

        private void Connection(object Timer)
        {
            while (true)
            {
                try
                {
                    connectSet.WaitOne();
                    if (_IsRun || !_AllowConn)
                    {
                        connectSet.Reset();
                    }
                    else
                    {
                        Start();
                    }
                    Thread.Sleep(5000);
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 发送消息线程
        /// </summary>
        /// <param name="inteval">发送间隔(毫秒)</param>
        private void SendMessageThread(int inteval)
        {
            Thread thread = new Thread(LoopSendMessage);
            thread.IsBackground = true;
            thread.Start(inteval);
        }

        /// <summary>
        /// 循环发送消息到服务器端
        /// </summary>
        /// <param name="inteval">发送间隔(毫秒)</param>
        private void LoopSendMessage(object inteval)
        {
            bool flag = false;
            int que_cnt = 0;
            string bytes;
            while (_IsRun)
            {
                que_cnt = msgQueue.Count;
                if (que_cnt > 0 && msgQueue.TryDequeue(out bytes))
                {
                    flag = BeginSend(bytes);//异步发送
                    if (flag)
                    {
                        //ToDO:去掉记录发送日志，改为记录发送数量[2014-09-03 edit by hjh] 
                        Interlocked.Increment(ref this.SendCount);

                        //记录发送的信息
                        LogManager.Info($"发送报文：{bytes}");
                        //string msg = string.Format("[{0:HH:mm:ss}]{1}", DateTime.Now, bytes);
                        //Extensions.AddLog(bytes);
                        //_logger.Info(bytes);
                        //DataForward.Que_SentData.Enqueue(msg);
                    }
                }
                if (que_cnt < 50)
                {
                    Thread.Sleep(1);
                }
            }
            try
            {
                Thread.CurrentThread.Abort();
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// 心跳超时检测(超过3分钟没有收到心跳数据则断掉连接)
        /// </summary>
        private void KeepAliveTimeoutTestThread()
        {
            Thread thread = new Thread(KeepAliveTimeoutTest) {IsBackground = true};
            thread.Start(30000);
            OnMessage?.BeginInvoke(":启动下发心跳超时检测线程", "1", this, null, null);
        }

        /// <summary>
        /// 心跳超时检测(超过3分钟没有收到心跳数据则断掉连接)
        /// </summary>
        /// <param name="inteval">间隔(毫秒)</param>
        private void KeepAliveTimeoutTest(object inteval)
        {
            while (_IsRun)
            {
                DateTime time = DateTime.Now;
                // 如果下发心跳超过3分钟，则断开重连
                if ((time - this._LastKeepAliveTime).TotalMinutes > 3)
                {
                    OnMessage?.BeginInvoke(":下发心跳超时(3分钟)断开重连...", "1", null, null, null);
                    this.Close(true);
                    break;
                }
                Thread.Sleep(Convert.ToInt32(inteval));
            }
            try
            {
                Thread.CurrentThread.Abort();
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// 心跳检测线程
        /// </summary>
        /// <param name="interval">间隔(秒)</param>
        public void KeepAliveThread(int interval)
        {
            Thread thread = new Thread(SendKeepAlive) {IsBackground = true};
            thread.Start(interval);
        }

        /// <summary>
        /// 定时发送检测包
        /// </summary>
        /// <param name="timer">发送间隔(秒)</param>
        public void SendKeepAlive(object timer)
        {
            while (true)
            {
                try
                {
                    if (_IsRun)
                    {
                        Send(">");
                    }
                    else
                    {
                        Thread.CurrentThread.Abort();
                        break;
                    }
                    Thread.Sleep(Convert.ToInt32(timer) * 1000);
                }
                catch
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="ServerIP"></param>
        /// <param name="Port"></param>
        /// <returns></returns>
        public Boolean Start()
        {
            try
            {
                //msgQueue = new ConcurrentQueue<byte[]>(); //启动的时候重新初始化队列
                _dataBuffer = new byte[1024 * 100];
                _NoFullPack = string.Empty;
                IPEndPoint iep = new IPEndPoint(IPAddress.Parse(_IPAddress), _Port);
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(iep);
                clientSocket.BeginReceive(_dataBuffer, 0, _dataBuffer.Length, 0, new AsyncCallback(RecieveCallBack), clientSocket);
                _IsRun = true;
                this.LastConnectTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                this._LastKeepAliveTime = DateTime.Now;
                Connected?.BeginInvoke("已链接", "1", this, null, null);
                //发送数据线程
                this.SendMessageThread(0);
                //心跳监测线程
                //this.KeepAliveThread(30);
                //心跳超时检测
                //this.KeepAliveTimeoutTestThread();
            }
            catch (Exception ex)
            {
                _IsRun = false;
                //_logger.Error(ex.Message,ex);
                LogManager.Error(ex.Message, ex);
            }
            return _IsRun;
        }

        /// <summary>
        /// 回发数据给客户端
        /// </summary>
        /// <param name="AR"></param>
        private void RecieveCallBack(IAsyncResult AR)
        {
            try
            {
                Socket client = (Socket)AR.AsyncState;
                int REnd = client.EndReceive(AR);
                if (REnd > 0)
                {
                    string receivedData = BitConverter.ToString(_dataBuffer, 0, REnd);  // 接收数据片段
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(HandleReceivedDatagram), receivedData);
                    //HandleReceivedDatagram(receivedData);

                    //if (RecieveData != null)
                    //{
                    //    RecieveData.BeginInvoke(receivedData, "1", this, null, null);
                    //}
                    client.BeginReceive(_dataBuffer, 0, _dataBuffer.Length, 0, new AsyncCallback(RecieveCallBack), client);
                }
                else
                {
                    Close(true);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("释放") || ex.Message.Contains("关闭"))
                {
                    Close(true);
                }
            }
        }

        /// <summary>
        /// 处理接收到的报文
        /// </summary>
        /// <param name="state"></param>
        private void HandleReceivedDatagram(object state)
        {
            this._NoFullPack += state.ToString();
            //List<string> msgs = DatagramConvert.DatagramParser(ref this._NoFullPack, "5B", "5D");
            //msgs.ForEach(msg =>
            //{

            //});
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="Msg"></param>
        /// <returns></returns>
        public Boolean Send(string Msg)
        {
            Boolean IsSend = false;
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Send(encoding.GetBytes(Msg));
                    //记录发送的信息
                    //string msg = string.Format("[{0:HH:mm:ss}]{1}", DateTime.Now, Msg);
                    //DataForward.Que_SentData.Enqueue(msg);
                    IsSend = true;
                }
                else
                {
                    if (_IsRun)
                    {
                        Close(true);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("释放") || ex.Message.Contains("关闭"))
                {
                    Close(true);
                }
            }
            return IsSend;
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool Send(byte[] bytes)
        {
            bool IsSend = false;
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Send(bytes);
                    //记录发送的信息
                    string msg = $"[{DateTime.Now:HH:mm:ss}]{string.Join("", bytes.Select(p => p.ToString("X2")))}";
                    //DataForward.Que_SentData.Enqueue(msg);
                    IsSend = true;
                }
                else
                {
                    if (_IsRun)
                    {
                        Close(true);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("释放") || ex.Message.Contains("关闭"))
                {
                    Close(true);
                }
            }
            return IsSend;
        }

        /// <summary>
        /// 异步发送消息的方法
        /// </summary>
        /// <param name="Msg">消息内容</param>
        public bool BeginSend(string Msg)
        {
            byte[] data = this.encoding.GetBytes(Msg);
            return BeginSend(data);
        }

        /// <summary>
        /// 异步发送消息的方法
        /// </summary>
        /// <param name="data">消息内容</param>
        public bool BeginSend(byte[] data)
        {
            bool flag = false;
            try
            {
                clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendDataEnd), clientSocket);
                flag = true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("释放") || ex.Message.Contains("关闭"))
                {
                    Close(true);
                }
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// 发送数据完成处理函数
        /// </summary>
        /// <param name="iar">目标客户端Socket</param>
        public void SendDataEnd(IAsyncResult iar)
        {
            try
            {
                Socket client = (Socket)iar.AsyncState;
                if (client.Connected)
                {
                    int sent = client.EndSend(iar);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("释放") || ex.Message.Contains("关闭"))
                {
                    Close(true);
                }
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="IsAllowReconnect">是否允许断线重连</param>
        public void Close(bool IsAllowReconnect)
        {
            try
            {
                _AllowConn = IsAllowReconnect;
                _IsRun = false;
                if (clientSocket != null)
                {
                    if (clientSocket.Connected)
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                    }
                    Thread.Sleep(100);
                    clientSocket.Close();
                }
                ConnClose?.BeginInvoke(" 链接关闭", "0", this, null, null);
                if (!_AllowConn)
                {
                    // 保存队列中的数据到文件
                    while (msgQueue.Count > 0)
                    {
                        SaveQueueDataToFile();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("ClientSocket->Close()", ex);
            }
        }


        // 保存1000条队列数据到文件
        public void SaveQueueDataToFile()
        {
            try
            {
                // 从消息队列获取1000条数据
                List<string> list = new List<string>();
                string data;
                // 发送给调度客户端
                while (msgQueue.TryDequeue(out data))
                {
                    list.Add(data);
                    if (list.Count >= 1000)
                    {
                        break;
                    }
                }
                // 保存数据到文件
                if (list.Count > 0)
                {
                    string filepath = $"{OverQueueDataFolderPath}{DateTime.Now:yyyyMMddHHmmssfff}.log";
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in list)
                    {
                        sb.Append(item).AppendLine();
                    }
                    LogManager.SaveDataToFile(filepath, sb.ToString());
                    list.Clear();
                    list = null;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("ClientSocket=>SaveQueueDataToFile()", ex);
            }
        }

        #region 定义事件
        /// <summary>
        /// 
        /// </summary>
        /// <param name="datagram"></param>
        public delegate void EventBase(string datagram, string state, object other);
        /// <summary>
        /// 链接关闭事件
        /// </summary>
        public event EventBase ConnClose;
        /// <summary>
        /// 连接成功
        /// </summary>
        public event EventBase Connected;
        /// <summary>
        /// 收到数据
        /// </summary>
        public event EventBase RecieveData;
        /// <summary>
        /// 发送普通消息给客户端
        /// </summary>
        public event EventBase OnMessage;

        #endregion
    }
}
