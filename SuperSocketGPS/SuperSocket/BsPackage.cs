using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
//using log4net;
using SuperSocket.Common;
using SuperSocketGPS.Common;
using SuperSocketGPS.Model;
using ThreadState = System.Threading.ThreadState;

namespace SuperSocketGPS.SuperSocket
{
    public static class BsPackage
    {
        private static readonly string Mark = ConfigurationManager.AppSettings["Mark"];
        private static readonly string BeginMark = ConfigurationManager.AppSettings["BeginMark"];
        private static readonly string EndMark = ConfigurationManager.AppSettings["EndMark"];
        //private static readonly byte MarkByte = Convert.ToByte(Mark, 16);
        //public static readonly List<byte> SourceList = new List<byte>();
        public static readonly ConcurrentQueue<List<byte>> PacketQueue = new ConcurrentQueue<List<byte>>();
        private static bool _isRunning;
        //private static readonly ILog Logger = LogManager.GetLogger(typeof(BsPackage));

        public static void SeparatePacket(this IList<byte> source,BsProtocolSession session)
        {
            var mark = Mark.HexStrToByteArr();
            if(!source.Any()) return;
            if (source[0] == mark[0] && source[1] == mark[1])
            {
                if (source.Count >= 76)
                {
                    PacketQueue.Enqueue(source.Where((t, i) => i < 76).ToList());
                    SeparatePacket(source.Skip(76).ToList(),session);
                }
                else
                {
                    session.FragBytes.AddRange(source);
                }
            }
            else
            {
                var firstIndex = source.IndexOf(mark[0]);
                var secondIndex = source.IndexOf(mark[1]);
                if (firstIndex == -1)
                {
                    if (!session.FragBytes.Any())
                    {
                        //废包
                    }
                    else
                    {
                        session.FragBytes.AddRange(source);
                    }
                }
                else if (secondIndex > 0 && firstIndex == secondIndex - 1)
                {
                    session.FragBytes.AddRange(source.Where((t, i) => i < firstIndex));
                    SeparatePacket(source.Skip(firstIndex).ToList(), session);
                }
            }
        }
        public static void StartConsuming()
        {
            _isRunning = true;
            MainForm.AddRow += AddRow;
            var t = new Thread(() =>
            {
                for (; _isRunning;)
                {
                    List<byte> packet;
                    var qCount = PacketQueue.Count;
                    if (qCount > 0 && PacketQueue.TryDequeue(out packet))
                    {
                        try
                        {
                            var list = packet.Construct();
                            if (list != null && list.Any())
                            {
                                foreach (var data in list)
                                {
                                    //MainForm.AddRow(data.VehicleNum);
                                    MainForm.OnAddRow(data.VehicleNum);
                                    MainForm.MongoWriter.Enqueue(new GPSData
                                    {
                                        CommandCode = Convert.ToString(data.CommandCode, 16),
                                        VehicleNum = data.VehicleNum,
                                        Message = data.SourceHexStr,
                                        DateTime = data.GetDateTimeStr(),
                                        SaveTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                                    });

                                    MainForm.MSocket?.AddQueue(data.Parse());
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            LogManager.Error(packet.ByteArrToHexStr(), e);
                        }
                    }
                    else
                    {
                        Thread.Sleep(20);
                    }
                    
                }
                Thread.CurrentThread.Abort();
            });
            if (t.ThreadState != ThreadState.Running) t.Start();
        }

        private static void AddRow(string vNum, DataTable dt)
        {
            var existed = dt.Select($"车牌号='{vNum}'");
            if (existed.Any()) return;
            var dr = dt.NewRow();
            dr["车牌号"] = vNum;
            dt.Rows.Add(dr);
        }

        public static void StopConsuming()
        {
            _isRunning = false;
        }
        public static List<BsGPSData> Construct(this IList<byte> source)
        {
            var res = new List<BsGPSData>();
            try
            {
                if (string.IsNullOrWhiteSpace(Mark))
                {
                    var ex = new Exception("无法获取分包标识位！");
                    //Logger.Error("无法获取分包标识位！", ex);
                    LogManager.Error("无法获取分包标识位！", ex);
                    throw ex;
                }
                var packetList = new List<byte[]>();
                var mark = Mark.HexStrToByteArr();
                //分包
                if (source.Where((t, i) => i < 2).Except(mark).Any())
                {
                    var ex = new Exception("数据包格式错误！");
                    //Logger.Error("数据包格式错误！", ex);
                    LogManager.Error("数据包格式错误！", ex);
                    throw ex;
                }
                List<byte> packet;
                while ((source = SeparateSinglePacket(source, out packet)).Count >= 0 && packet.Count > 0)
                {
                    packetList.Add(packet.ToArray());
                }

                foreach (var pack in packetList)
                {
                    //转义
                    //var originalPack = pack.ByteArrToHexStr().Replace("7D01", "7D").Replace("7D02", "7E").HexStrToByteArr();
                    //var originalPack = pack;
                    var vnEndIndex = Array.IndexOf(pack.CloneRange(10, 16), (byte)0) + 10;
                    var bsData = new BsGPSData();
                    bsData.BeginMark = BitConverter.ToUInt16(pack, 0);
                    bsData.DataLength = Convert.ToUInt16(pack.CloneRange(2, 2).ByteArrToHexStr(), 16);
                    bsData.Version = pack[4];
                    bsData.SerialNum1 = pack[5];
                    bsData.WireStatus = pack[6];
                    bsData.SerialNum2 = Convert.ToUInt16(pack.CloneRange(7, 2).ByteArrToHexStr(), 16);
                    bsData.Status = pack[9];
                    bsData.VehicleNum = Encoding.Default.GetString(pack.CloneRange(10, vnEndIndex - 10));
                    bsData.Date = pack.CloneRange(vnEndIndex+1,3);
                    bsData.Time = pack.CloneRange(vnEndIndex+4,3);
                    bsData.CommandCode = pack[vnEndIndex+7];
                    bsData.Body = pack.CloneRange(vnEndIndex + 8, pack.Length - 6 - (vnEndIndex + 8)+1).ConstructBody();
                    bsData.LineId = pack.CloneRange(pack.Length-5,3);
                    bsData.SubLineCode = pack[pack.Length-2];
                    bsData.CheckCode = pack[pack.Length - 1];

                    //bsData.IdCode = Encoding.Default.GetString(originalPack.CloneRange(7, 17));
                    //if (bsData.IsMultiPacket())
                    //{
                    //    bsData.SerialNum2 = BitConverter.ToUInt16(originalPack, 24);
                    //    bsData.PacketAmount = BitConverter.ToUInt16(originalPack, 26);
                    //    bsData.PacketId = BitConverter.ToUInt16(originalPack, 28);
                    //    bsData.MsgBody = originalPack.CloneRange(30, originalPack.Length - 2 - 30);
                    //}
                    //else
                    //{
                    //    bsData.MsgBody = originalPack.CloneRange(24, originalPack.Length - 2 - 24);
                    //}
                    bsData.SourceHexStr = pack.ByteArrToHexStr();
                    res.Add(bsData);
                }


            }
            catch (Exception e)
            {
                LogManager.Error(e.Message,e);
                throw;
                //return null;
            }

            return res;
        }

        public static PeriodLocationInfo ConstructBody(this byte[] arr)
        {
            var driverIdStrEndIndex = Array.IndexOf(arr.CloneRange(29, arr.Length  - 29), (byte)0x00) + 29;
            var localeStrEndIndex =
                Array.IndexOf(arr.CloneRange(driverIdStrEndIndex + 3, arr.Length  - (driverIdStrEndIndex + 3)), (byte)0x00) +
                driverIdStrEndIndex + 3;
            var cellIdStrEndIndex =
                Array.IndexOf(arr.CloneRange(localeStrEndIndex + 1, arr.Length - (localeStrEndIndex + 1)), (byte)0x00) +
                localeStrEndIndex + 1;
            var body = new PeriodLocationInfo();
            //body.Latitude = Convert.ToUInt32(arr.CloneRange(0,4).ByteArrToHexStr(), 16);
            //body.Longitude = Convert.ToUInt32(arr.CloneRange(4, 4).ByteArrToHexStr(), 16);
            body.Latitude = arr.CloneRange(0, 4);
            body.Longitude = arr.CloneRange(4, 4);
            body.InstantaneousVelocity = Convert.ToUInt16(arr.CloneRange(8, 2).ByteArrToHexStr(), 16);
            body.Azimuth = Convert.ToUInt16(arr.CloneRange(10, 2).ByteArrToHexStr(), 16);
            body.VehicleStatus = arr[12];
            body.DirectionMark = arr[13];
            body.NextStationNum = arr[14];
            body.DistanceToNextStation = Convert.ToUInt16(arr.CloneRange(15, 2).ByteArrToHexStr(), 16);
            body.IsInStation = arr[17];
            body.IsCaching = arr[18];
            body.Mileage = arr.CloneRange(19,3);
            body.OverspeedCriterion = arr[22];
            body.Temperature = Convert.ToUInt16(arr.CloneRange(23, 2).ByteArrToHexStr(), 16);
            body.FuelInt = Convert.ToUInt16(arr.CloneRange(25, 2).ByteArrToHexStr(), 16);
            body.FuelFraction = arr[27];
            body.ServiceStatus = arr[28];
            body.DriverId = Encoding.Default.GetString(arr.CloneRange(29, driverIdStrEndIndex - 29));
            body.SIMCardType = arr[driverIdStrEndIndex+1];
            body.BaseLocationStatus = arr[driverIdStrEndIndex + 2];
            body.BaseLocationLocale = Encoding.Default.GetString(arr.CloneRange(driverIdStrEndIndex + 3, localeStrEndIndex - (driverIdStrEndIndex + 3)));
            body.BaseLocationCellId = Encoding.Default.GetString(arr.CloneRange(localeStrEndIndex + 1, cellIdStrEndIndex - (localeStrEndIndex + 1)));

            return body;
        }
        private static IList<byte> SeparateSinglePacket(IList<byte> source, out List<byte> packet)
        {
            packet = new List<byte>();
            //分包Flag，检测到标识位时加一，偶数时即分出一包。
            var flag = 0;
            var mark = Mark.HexStrToByteArr()[0];
            foreach (var b in source)
            {
                if (b == mark)
                {
                    flag++;
                    if (flag % 2 == 0)
                    {
                        //分出一包
                        break;
                    }
                }
                packet.Add(b);
            }
            source = source.Skip(packet.Count).ToArray();
            //var markIndex = source.IndexOf(mark, 0, source.Length);
            //if (markIndex != 0)
            //{
            //    source = source.Skip(markIndex).ToArray();
            //}
            return source;
        }

        public static string Parse(this BsGPSData src)
        {
            var res=new List<byte>();
            try
            {
                #region 数据头
                res.AddRange(102.IntToBytesBig());//长度
                res.AddRange(new[] {(byte) 0, (byte) 0, (byte) 0, (byte) 0});
                res.AddRange(new []{(byte)0x01,(byte)0x01});//业务数据类型
                res.AddRange(new[] { (byte)0, (byte)0, (byte)0, (byte)0 });
                res.AddRange(new[] { (byte)0, (byte)0, (byte)0 });//版本
                res.Add(0);
                res.AddRange(new[] { (byte)0, (byte)0, (byte)0, (byte)0 });//加密密钥
                #endregion

                #region 数据体
                //车辆ID
                var vIdByteArr = Encoding.GetEncoding("GBK").GetBytes(src.VehicleNum);
                res.AddRange(vIdByteArr);
                //右补0
                if (vIdByteArr.Length < 20)
                {
                    for (var i = 0; i < 20 - vIdByteArr.Length; i++)
                    {
                        res.Add(0);
                    }
                }
                //线路ID
                var lineId = src.LineId.ToList();
                lineId.Insert(0,0);
                res.AddRange(lineId);

                //定位时间
                const string format = "yyyyMMddHHmmss";
                //var dateTimeStr = $"20{src.Date.ByteArrToStr()}{src.Time.ByteArrToStr()}";
                var dateTimeStr = src.GetDateTimeStr(format);
                res.AddRange(Encoding.GetEncoding("GBK").GetBytes(dateTimeStr));

                //发送时间
                var nowStr = DateTime.Now.ToString(format);
                res.AddRange(Encoding.GetEncoding("GBK").GetBytes(nowStr));

                //上下行
                res.Add(src.Body.DirectionMark);

                //纬度
                //var lat = src.Body.Latitude;
                //var latitudeFraction = lat[1] + (lat[2] * 256 + lat[3]) / 10000;
                //var latitude = (lat[0] + latitudeFraction / 60) * 1000000;
                res.AddRange(ConvertTo84(src.Body.Latitude));

                //经度
                res.AddRange(ConvertTo84(src.Body.Longitude));

                //速度
                res.AddRange(src.Body.InstantaneousVelocity.UshortToBytesBig());

                //海拔高度
                res.AddRange(((ushort)0).UshortToBytesBig());

                //方向角
                res.AddRange(src.Body.Azimuth.UshortToBytesBig());

                //上一站
                res.AddRange(new[] {(byte) 0, (byte) (src.Body.NextStationNum - 1)});

                //下一站
                res.AddRange(new[] {(byte) 0, src.Body.NextStationNum});

                //下一站距离
                res.AddRange(src.Body.DistanceToNextStation.UshortToBytesBig());

                //营运状态
                res.Add(src.Body.ServiceStatus);

                //车辆状态
                //var status = Convert.ToString(src.Body.VehicleStatus, 2).PadLeft(8, '0');
                //var sb = new StringBuilder(status) {[6] = '1'};
                //res.Add(Convert.ToByte(sb.ToString(), 2));
                res.Add((byte) (src.Body.VehicleStatus | 64));

                //超速标准
                res.Add(src.Body.OverspeedCriterion);

                #endregion

                //转义 
                res =res.Escape().ToList();


                //添加头尾标识
                res.Insert(0,Convert.ToByte(BeginMark,16));
                res.AddRange(((ushort)0).UshortToBytesBig());
                res.Add(Convert.ToByte(EndMark,16));


            }
            catch (Exception e)
            {
                LogManager.Error(e.Message,e);
                throw;
            }
            return  res.ToArray().ByteArrToHexStr();
        }

        /// <summary>
        /// 经纬度转84经纬度
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private static byte[] ConvertTo84(byte[] src)
        {
            var latitudeFraction = src[1] + (src[2] * 256 + src[3]) / 10000;
            var latitude = (src[0] + latitudeFraction / 60) * 1000000;
            return latitude.IntToBytesBig();
        }

        /// <summary>
        /// 转义
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private static IEnumerable<byte> Escape(this IEnumerable<byte> src)
        {
            return
                src.ToArray()
                    .ByteArrToHexStr()
                    .Replace("5B", "5A01")
                    .Replace("5A", "5A02")
                    .Replace("5D", "5E01")
                    .Replace("5E", "5E02")
                    .HexStrToByteArr();
        }

        //public static byte[] Parse<T>(this T src) where T : BsGpsData
        //{
        //    try
        //    {
        //        var type = src.GetType();
        //        var res = src;
        //        var body = new List<byte>();
        //        if (type == typeof(BsDataGeneralResponse))
        //        {
        //            var source = src as BsDataGeneralResponse ?? new BsDataGeneralResponse();
        //            body.Add(source.OriginalMsgId);
        //            body.AddRange(BitConverter.GetBytes(source.OriginalMsgNum));
        //            body.AddRange(BitConverter.GetBytes(source.MsgResult));
        //        }
        //        else if (type == typeof(BsDataAuth))
        //        {
        //            var source = src as BsDataAuth ?? new BsDataAuth();
        //            body.AddRange(BitConverter.GetBytes(source.LoginSerialNum));
        //            body.AddRange(source.EndClock);
        //            body.AddRange(Encoding.Default.GetBytes(source.SoftwareVerNum));
        //            body.AddRange(Encoding.Default.GetBytes(source.HardwareVerNum));
        //            body.AddRange(Encoding.Default.GetBytes(source.LicencePlateNum));
        //        }
        //        else if (type == typeof(BsDataAuthResponse))
        //        {
        //            var source = src as BsDataAuthResponse ?? new BsDataAuthResponse();
        //            body.Add(source.LoginStatus);
        //            body.AddRange(source.ServerClock);
        //            body.Add(source.EncryptionType);
        //            body.AddRange(BitConverter.GetBytes(source.EncryptionKeyLength));
        //            body.AddRange(source.EncryptionKey);
        //        }
        //        else if (type == typeof(BsDataMalAlert))
        //        {
        //            var source = src as BsDataMalAlert ??
        //                         new BsDataMalAlert();
        //            body.Add(source.MalCodeAmount);
        //            foreach (var mal in source.Malfunctions)
        //            {
        //                body.AddRange(BitConverter.GetBytes(mal));
        //            }
        //        }
        //        else if (type == typeof(BsDataParamSetup))
        //        {
        //            var source = src as BsDataParamSetup ?? new BsDataParamSetup();
        //            body.Add(source.ParamAmount);
        //            foreach (var param in source.ParamList)
        //            {
        //                body.AddRange(BitConverter.GetBytes(param.ParamId));
        //                body.AddRange(BitConverter.GetBytes(param.ParamLength));
                       
        //            }

        //        }
        //        else if (type == typeof(BsDataParamQueryResponse))
        //        {
        //            var source = src as BsDataParamQueryResponse ?? new BsDataParamQueryResponse();
        //            body.Add(source.ParamAmount);
        //            foreach (var param in source.ParamList)
        //            {
        //                body.AddRange(BitConverter.GetBytes(param.ParamId));
        //                body.AddRange(BitConverter.GetBytes(param.ParamLength));
                        
        //            }

        //        }
        //        else if (type == typeof(BsDataParamQuery))
        //        {
        //            var source = src as BsDataParamQuery ?? new BsDataParamQuery();
        //            body.Add(source.ParamAmount);
        //            foreach (var id in source.ParamIdList)
        //            {
        //                body.AddRange(BitConverter.GetBytes(id));
        //            }

        //        }
        //        else if (type == typeof(BsDataUpdate))
        //        {
        //            var source = src as BsDataUpdate ?? new BsDataUpdate();
        //            body.Add(source.ServerIpAddressLength);
        //            body.AddRange(Encoding.Default.GetBytes(source.ServerIpAddress));
        //            body.Add(source.FileNameLength);
        //            body.AddRange(Encoding.Default.GetBytes(source.FileName));
        //        }
        //        else
        //        {

        //        }
        //        res.MsgBody = body.ToArray();
        //        return ParseBsData(res);
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Error(e.Message,e);
        //        throw;
        //    }
        //}

        //private static byte[] ParseBsData(BsGpsData data)
        //{
        //    var res = new List<byte> {data.MsgId, data.MsgProp};
        //    res.AddRange(BitConverter.GetBytes(data.DataLength));
        //    res.AddRange(BitConverter.GetBytes(data.MsgSerialNum));
        //    res.AddRange(Encoding.Default.GetBytes(data.IdCode));

        //    if (data.IsMultiPacket())
        //    {
        //        res.AddRange(BitConverter.GetBytes(data.SerialNum2));
        //        res.AddRange(BitConverter.GetBytes(data.PacketAmount));
        //        res.AddRange(BitConverter.GetBytes(data.PacketId));
        //    }

        //    res.AddRange(data.MsgBody);

        //    //得到校验码
        //    var flag = res.FirstOrDefault();
        //    res.ForEach(t => flag ^= t);
        //    res.Add(flag);

        //    //转义
        //    res = res.ToArray().ByteArrToHexStr().Replace("7D", "7D01").Replace("7E", "7D02").HexStrToByteArr().ToList();

        //    //插入首尾标识符
        //    res.Insert(0, data.BeginMark);
        //    res.Add(data.EndMark);

        //    return res.ToArray();
        //}


    }
}
