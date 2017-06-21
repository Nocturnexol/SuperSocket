using System.Collections.Generic;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocketGPS.SuperSocket
{
    public class BsProtocolRequestInfo : RequestInfo<List<BsGPSData>>
    {
        public BsProtocolRequestInfo(List<BsGPSData> bsDataList)
        {
            Initialize("BsProtocolRequestInfoCommand", bsDataList);
        }
    }
}
