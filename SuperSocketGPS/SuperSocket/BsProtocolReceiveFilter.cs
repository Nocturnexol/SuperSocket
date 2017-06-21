using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SuperSocket.Common;
//using log4net;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocketGPS.Common;
using SuperSocketGPS.Model;

namespace SuperSocketGPS.SuperSocket
{
    public class BsProtocolReceiveFilter : IReceiveFilter<BsProtocolRequestInfo>
    {
        private readonly BsProtocolSession _appSession;
        private readonly object _lockObj = new object();
        //private readonly ILog _logger;
        public BsProtocolReceiveFilter(IAppSession appSession)
        {
            State = FilterState.Normal;
            _appSession = (BsProtocolSession) appSession;
            _appSession.FragBytes = new List<byte>();
            //_logger = LogManager.GetLogger(GetType());
        }

        public BsProtocolRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;
            var res = new ArraySegment<byte>(readBuffer, offset, length);
            var source = res.ToArray();
            //Common.Extensions.AddLog($"收到报文：{source.ByteArrToHexStr()}");
            //LogManager.Info($"收到报文：{source.ByteArrToHexStr()}");

            List<BsGPSData> list = null;
            try
            {
                //lock (_lockObj)
                //{
                //    BsPackage.SourceList.AddRange(source);
                //}
                //Common.Extensions.AddLog($"source长度：{source.Length}");
                //Common.Extensions.AddLog($"发送报文：{str}");
                 //LogManager.Info($"发送报文：{str}");
                source.SeparatePacket(_appSession);
                if (_appSession.FragBytes.Any() && _appSession.FragBytes.Count % 76 == 0)
                {
                    var fragArr = _appSession.FragBytes.ToArray();
                    for (var i = 0; i < fragArr.Length / 76; i++)
                    {
                        //BsPackage.PacketQueue.Enqueue(
                        //    _appSession.FragBytes.Where((t, k) => k >= i * 76 && k < (i + 1) * 76).ToList());
                        BsPackage.PacketQueue.Enqueue(fragArr.CloneRange(76 * i, 76).ToList());
                    }
                    _appSession.FragBytes.Clear();
                }

            }
            catch (Exception e)
            {
                LogManager.Error(e.Message, e);
            }

            return new BsProtocolRequestInfo(list);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public int LeftBufferSize { get; private set; }
        public IReceiveFilter<BsProtocolRequestInfo> NextReceiveFilter { get; private set; }
        public FilterState State { get; private set; }
    }
}
