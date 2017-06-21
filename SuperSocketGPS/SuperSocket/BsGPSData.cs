using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocketGPS.Common;

namespace SuperSocketGPS.SuperSocket
{
    public class BsGPSData
    {
        /// <summary>
        /// 标识位
        /// </summary>
        public ushort BeginMark { get; set; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public ushort DataLength { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public byte Version{ get; set; }

        /// <summary>
        /// 序列号1
        /// </summary>
        public byte SerialNum1 { get; set; }

        /// <summary>
        /// 序列号2
        /// </summary>
        public ushort SerialNum2 { get; set; }

        /// <summary>
        /// 检测线状态
        /// </summary>
        public byte WireStatus { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// 车辆编号
        /// </summary>
        public string VehicleNum { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public byte[] Date { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public byte[] Time { get; set; }

        /// <summary>
        /// 线路ID
        /// </summary>
        public byte[] LineId { get; set; }

        /// <summary>
        /// 子线路编码
        /// </summary>
        public byte SubLineCode { get; set; }

        /// <summary>
        /// 命令字
        /// </summary>
        public byte CommandCode { get; set; }

        /// <summary>
        /// 数据体
        /// </summary>
        public PeriodLocationInfo Body { get; set; }

        /// <summary>
        /// 校验码
        /// </summary>
        public byte CheckCode { get; set; }

        public string SourceHexStr { get; set; }

        public string GetDateTimeStr()
        {
            var dateTime=new DateTime(int.Parse("20"+Date[0]),Date[1],Date[2],Time[0],Time[1],Time[2]);
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
        }
        public string GetDateTimeStr(string format)
        {
            var dateTime=new DateTime(int.Parse("20"+Date[0]),Date[1],Date[2],Time[0],Time[1],Time[2]);
            return dateTime.ToString(format);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var lineId=LineId.ToList();
            lineId.Insert(0, 0x00);
            var l = Convert.ToUInt32(lineId.ByteArrToHexStr(), 16);
            sb.Append($"标识位：{BitConverter.GetBytes(BeginMark).ByteArrToHexStr()}，数据长度：{DataLength}，版本:{Version}，序列号1：{SerialNum1}，序列号2：{SerialNum2}，检测线状态：{Convert.ToString(WireStatus,2).PadLeft(8,'0')}，状态位：{Convert.ToString(Status,2).PadLeft(8,'0')}，车辆编号：{VehicleNum}，时间：{GetDateTimeStr()}，命令字：0x{CommandCode:X2}，线路ID：{l}，子线路编码：{SubLineCode}，校验码：{CheckCode}.\r\n");
            sb.Append(Body);
            return sb.ToString();
        }
    }

    public class PeriodLocationInfo
    {
        /// <summary>
        /// 纬度
        /// </summary>
        public byte[] Latitude { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public byte[] Longitude { get; set; }

        /// <summary>
        /// 瞬时速度
        /// </summary>
        public ushort InstantaneousVelocity { get; set; }

        /// <summary>
        /// 方位角
        /// </summary>
        public ushort Azimuth { get; set; }

        /// <summary>
        /// 车辆状态
        /// </summary>
        public byte VehicleStatus { get; set; }

        /// <summary>
        /// 上下行标识
        /// </summary>
        public byte DirectionMark { get;set; }

        /// <summary>
        /// 下一站编号
        /// </summary>
        public byte NextStationNum { get; set; }

        /// <summary>
        /// 距离下一站
        /// </summary>
        public ushort DistanceToNextStation { get; set; }

        /// <summary>
        /// 是否在站点内
        /// </summary>
        public byte IsInStation { get; set; }

        /// <summary>
        /// 是否缓存数据
        /// </summary>
        public byte IsCaching { get; set; }

        /// <summary>
        /// 总里程
        /// </summary>
        public byte[] Mileage { get; set; }

        /// <summary>
        /// 超速标准
        /// </summary>
        public byte OverspeedCriterion { get; set; }

        /// <summary>
        /// 车内温度
        /// </summary>
        public ushort Temperature { get; set; }

        /// <summary>
        /// 油量（整数）
        /// </summary>
        public ushort FuelInt { get; set; }

        /// <summary>
        ///油量（小数） 
        /// </summary>
        public byte FuelFraction { get; set; }

        /// <summary>
        /// 营运状态
        /// </summary>
        public byte ServiceStatus{get; set; }

        /// <summary>
        /// 驾驶员ID
        /// </summary>
        public string DriverId { get; set; }

        /// <summary>
        /// SIM卡类型
        /// </summary>
        public byte SIMCardType { get; set; }

        /// <summary>
        /// 基站定位：状态
        /// </summary>
        public byte BaseLocationStatus { get;set; }

        /// <summary>
        /// 基站定位：位置
        /// </summary>
        public string BaseLocationLocale { get; set; }

        /// <summary>
        /// 基站定位：Cell ID
        /// </summary>
         public string BaseLocationCellId { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            //var latitudeArr = BitConverter.GetBytes(Latitude);
            //var longtitudeArr = BitConverter.GetBytes(Longitude);
            var distance=Mileage.ToList();
            distance.Insert(0, 0x00);
            var dis = Convert.ToUInt32(distance.ByteArrToHexStr(), 16);
            sb.Append(
                $"纬度：{Latitude[0]}°{Latitude[1]}.{Latitude[2]}{Latitude[3]}''，经度：{Longitude[0]}°{Longitude[1]}.{Longitude[2]}{Longitude[3]}''，瞬时车速：{InstantaneousVelocity}公里/小时，方位角：{Azimuth}°，车辆状态：{VehicleStatus}，上下行标识：{DirectionMark}，下一站编号：{NextStationNum}，距离下一站：{DistanceToNextStation}米，是否在站点内：{IsInStation}，是否缓存数据：{IsCaching}，累计里程：{dis}米，超速标准：{OverspeedCriterion}公里/小时，营运状态：0x{ServiceStatus:X2}，驾驶员身份ID：{DriverId}，SIM卡类型：0x{SIMCardType:X2}，基站定位：状态：{BaseLocationStatus}，基站定位：位置：{BaseLocationLocale}，基站定位：Cell ID：{BaseLocationCellId}");
            return sb.ToString();
        }
    }
}
