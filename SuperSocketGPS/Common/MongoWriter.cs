using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
//using log4net;
using MongoDB.Driver;
using SuperSocketGPS.Model;

namespace SuperSocketGPS.Common
{
    public class MongoWriter
    {
        private readonly string _mongoServerAddress = ConfigurationManager.AppSettings["MongoServer"];
        private readonly string _dataBase = ConfigurationManager.AppSettings["MongoDataBase"];
        private readonly string _collection = ConfigurationManager.AppSettings["MongoCollection"];
        private readonly MongoCollection _mongoCollection;
        //private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _isRunning;
        private readonly ConcurrentQueue<GPSData> _messageQueue = new ConcurrentQueue<GPSData>();
        public MongoWriter()
        {
            if (string.IsNullOrWhiteSpace(_mongoServerAddress) || string.IsNullOrWhiteSpace(_dataBase) ||
                string.IsNullOrWhiteSpace(_collection))
            {
                var ex=new Exception("缺少有效Mongo配置");
                LogManager.Error(ex.Message,ex);
                throw ex;
            }
            var mongoServer = new MongoClient(_mongoServerAddress).GetServer();
            var mongoDatabase = mongoServer.GetDatabase(_dataBase);
            _mongoCollection = mongoDatabase.GetCollection(_collection);
        }

        public void Enqueue(GPSData data)
        {
            try
            {
                if (_isRunning)
                {
                    _messageQueue.Enqueue(data);
                }
            }
            catch (Exception e)
            {
                LogManager.Error(e.Message,e);
                throw;
            }
        }

        public void WriteToMongo()
        {
            while (_isRunning)
            {
                var msgList = new List<GPSData>();
                GPSData msg;
               while(_messageQueue.Count > 0 && _messageQueue.TryDequeue(out msg))
                {
                   msgList.Add(msg);
                }
                if (msgList.Any())
                    Insert(msgList);
                if (_messageQueue.Count < 10)
                {
                    Thread.Sleep(10);
                }
            }

            Thread.CurrentThread.Abort();
        }

        private bool Insert(List<GPSData> msgs)
        {
            var res = _mongoCollection.InsertBatch(msgs);
            return res.Count(t => t.Ok) == msgs.Count;
        }

        public void Start()
        {
            var thread=new Thread(WriteToMongo);
            _isRunning = true;
            thread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
        }

    }
}
