using System.Net;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocketGPS.SuperSocket
{
    public class BsReceiveFilterFactory : IReceiveFilterFactory<BsProtocolRequestInfo>
    {
        public IReceiveFilter<BsProtocolRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            return new BsProtocolReceiveFilter(appSession);
        }
    }
}
