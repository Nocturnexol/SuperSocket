using System;
using System.Collections.Generic;
//using log4net;
using SuperSocket.SocketBase;
using SuperSocketGPS.Common;

namespace SuperSocketGPS.SuperSocket
{
    public class BsProtocolSession : AppSession<BsProtocolSession, BsProtocolRequestInfo>
    {
        //private readonly ILog _logger = LogManager.GetLogger(typeof(BsProtocolSession));
        public List<byte> FragBytes { get; set; }
        protected override void HandleException(Exception e)
        {
            Send("error: {0}", e.Message);
            LogManager.Error(e.Message, e);
        }
        /// <summary>  
        /// 新连接  
        /// </summary>  
        protected override void OnSessionStarted()
        {
            //输出客户端IP地址  
            Console.WriteLine(LocalEndPoint.Address.ToString());
            Send("Hello User,Welcome to SuperSocket Telnet Server!");
        }
    }
}
