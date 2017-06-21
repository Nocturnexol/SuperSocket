using MongoDB.Bson;

namespace SuperSocketGPS.Model
{
    public class GPSData
    {
        public ObjectId _id { get; set; }
        public string CommandCode { get; set; }
        public string VehicleNum { get; set; }
        public string Message { get; set; }
        public string DateTime { get; set; }
        public string SaveTime { get; set; }
        public override string ToString()
        {
            return $"命令字：{CommandCode}，车牌号：{VehicleNum}，报文内容：{Message}";
        }
    }
}
