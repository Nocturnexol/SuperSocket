using System.Windows.Forms;
//using log4net;
using SuperSocket.SocketBase.Command;
using SuperSocketGPS.Common;

namespace SuperSocketGPS.SuperSocket
{
    public class BsProtocolRequestInfoCommand : CommandBase<BsProtocolSession, BsProtocolRequestInfo>
    {
        public override void ExecuteCommand(BsProtocolSession session, BsProtocolRequestInfo requestInfo)
        {
            //var logger = LogManager.GetLogger(GetType());
            requestInfo.Body?.ForEach(t =>
            {
                LogManager.Info(t.ToString());
                //Extensions.AddLog(t.ToString());
            });

        }
    }
}
