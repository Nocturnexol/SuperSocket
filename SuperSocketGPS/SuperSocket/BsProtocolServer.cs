using System.Windows.Forms;
using log4net;
using SuperSocket.SocketBase;
using CloseReason = SuperSocket.SocketBase.CloseReason;

namespace SuperSocketGPS.SuperSocket
{
    public class BsProtocolServer: AppServer<BsProtocolSession, BsProtocolRequestInfo>
    {
        private readonly ILog _logger=LogManager.GetLogger(typeof(BsProtocolServer));
        /// <summary>
        /// 使用自定义协议工厂
        /// </summary>
        public BsProtocolServer()
            : base(new BsReceiveFilterFactory())
        {
        }
        /// <summary>  
        /// 输出新连接信息  
        /// </summary>  
        /// <param name="session"></param>  
        protected override void OnNewSessionConnected(BsProtocolSession session)
        {
            base.OnNewSessionConnected(session);
            //输出客户端IP地址
            //Common.Extensions.AddLog($"{session.LocalEndPoint.Address}:连接\r\n");
            _logger.Info($"{session.LocalEndPoint.Address}:连接");
        }

        /// <summary>  
        /// 输出断开连接信息  
        /// </summary>  
        /// <param name="session"></param>  
        /// <param name="reason"></param>  
        protected override void OnSessionClosed(BsProtocolSession session, CloseReason reason)
        {
            base.OnSessionClosed(session, reason);
            //Common.Extensions.AddLog($"{session.LocalEndPoint.Address}:断开连接\r\n");
            //Common.Extensions.AddLog($"{session.RemoteEndPoint}连接断开. 断开原因:{reason}\r\n");
            _logger.Info($"{session.LocalEndPoint.Address}:断开连接\r\n{session.RemoteEndPoint}连接断开. 断开原因:{reason}");
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            MessageBox.Show(@"服务已停止!", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
