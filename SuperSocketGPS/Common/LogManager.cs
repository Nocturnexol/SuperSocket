using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SuperSocketGPS.Common
{
    /// <summary>
    /// 日志写入类
    /// </summary>
    public class LogManager
    {
        private static object lock_info = new object();
        private static object lock_log = new object();
        private static object lockLogN = new object();
        private static object lock_hour = new object();
        private static object lock_error = new object();
        private static object lock_logName = new object();
        private static object lock_UnrepeatLine = new object();

        /// <summary>
        /// 写入日志(每天一个文件)
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="folder">日志文件所在文件件,默认“Log”</param>
        public static void Log(string content, string folder = "Log")
        {
            lock (lock_log)
            {
                string fileDir = string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'), folder);
                CreateFolder(fileDir);
                string filePath = string.Format("{0}\\{1:yyyy-MM-dd}.log", fileDir, DateTime.Now);
                try
                {
                    using (StreamWriter sw = new StreamWriter(filePath, true))
                    {
                        sw.AutoFlush = true;
                        sw.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss fff") + "]" + content);
                        sw.Close();
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 写入日志(每天一个文件)
        /// 文件名上加上自定义部分
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="folder">日志文件所在文件件,默认“Log”</param>
        public static void LogN(string content, string nameAppend, string folder = "Log")
        {
            lock (lockLogN)
            {
                string fileDir = string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'), folder);
                // 创建目录
                CreateFolder(fileDir);
                string filePath = string.Format("{0}\\{1:yyyy-MM-dd}_{2}.log", fileDir, DateTime.Now, nameAppend);
                try
                {
                    using (StreamWriter sw = new StreamWriter(filePath, true))
                    {
                        sw.AutoFlush = true;
                        sw.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss fff") + "]" + content);
                        sw.Close();
                    }
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 写入日志(每天一个文件)
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="folder">日志文件所在文件件,默认“Log”</param>
        public static void LogHour(string content, string folder = "Log")
        {
            lock (lock_log)
            {
                string fileDir = string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'), folder);
                CreateFolder(fileDir);
                string filePath = string.Format("{0}\\{1:yyyyMMddHH}.log", fileDir, DateTime.Now);
                try
                {
                    using (StreamWriter sw = new StreamWriter(filePath, true))
                    {
                        sw.AutoFlush = true;
                        sw.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss fff") + "]" + content);
                        sw.Close();
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 写入信息
        /// </summary>
        /// <param name="content">内容</param>
        public static void Info(string content)
        {
            lock (lock_info)
            {
                string fileDir = string.Format("{0}\\MessageLog", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'));
                CreateFolder(fileDir);
                string filePath = string.Format("{0}\\{1:yyyy-MM-dd}.log", fileDir, DateTime.Now);
                try
                {
                    using (StreamWriter sw = new StreamWriter(filePath, true))
                    {
                        sw.AutoFlush = true;
                        sw.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss fff") + "]" + content);
                        sw.Close();
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 写入Exception信息
        /// </summary>
        /// <param name="content">错误文本</param>
        /// <param name="ex">异常信息</param>
        public static void Error(string content, Exception ex)
        {
            lock (lock_error)
            {
                string fileDir = string.Format("{0}\\ErrorLog", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'));
                CreateFolder(fileDir);
                string filePath = string.Format("{0}\\{1:yyyy-MM-dd}.log", fileDir, DateTime.Now);
                try
                {
                    using (StreamWriter sw = new StreamWriter(filePath, true))
                    {
                        sw.AutoFlush = true;
                        sw.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss fff") + "]" + content + "=>" + ex);
                        sw.Close();
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 写入Exception信息
        /// </summary>
        /// <param name="content">错误文本</param>
        /// <param name="ex">异常信息</param>
        public static void ErrorN(string content, string nameAppend, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            lock (lock_error)
            {
                string fileDir = string.Format("{0}\\ErrorLog", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'));
                // 创建目录
                CreateFolder(fileDir);
                string filePath = string.Format("{0}\\{1:yyyy-MM-dd}_{2}.log", fileDir, DateTime.Now, nameAppend);
                try
                {
                    using (StreamWriter sw = new StreamWriter(filePath, true))
                    {
                        sw.AutoFlush = true;

                        sb.Clear();
                        sb.Append("[");
                        sb.Append(DateTime.Now.ToString(("HH:mm:ss fff")));
                        sb.Append("]");
                        sb.Append(content);
                        sb.Append(",异常消息\r\n");
                        sb.Append(ex.Message);
                        sb.Append("\r\nSource:");
                        sb.Append(ex.Source);
                        sb.Append("\r\nTargetSite:");
                        sb.Append(ex.TargetSite);
                        sb.Append("\r\nStackTrace:");
                        sb.Append(ex.StackTrace);

                        sw.WriteLine(sb.ToString());
                        sw.Close();
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 写非重复内容日志文件,日志文件名为："前缀"+"yyyy-MM-dd.log"
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="logPrefix">日志文件前缀,默认后边跟上"yyyy-MM-dd"</param>
        public static void WriteUnrepeatLine(string msg, string logPrefix = null)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }
            lock (lock_UnrepeatLine)
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory + "SpecialLog\\";
                // 创建文件夹
                CreateFolder(basePath);
                string logname = string.Format("{0}\\{1}{2:yyyy-MM-dd}.log", basePath, string.IsNullOrWhiteSpace(logPrefix) ? "" : logPrefix.Trim(), DateTime.Now);
                FileStream fs = null;
                try
                {
                    if (File.Exists(logname))
                    {
                        // 获取所有内容
                        string[] lines = File.ReadAllLines(logname, Encoding.UTF8);
                        //如果包含已存在的消息则退出执行
                        if (lines.Contains(msg))
                        {
                            return;
                        }
                        fs = File.Open(logname, FileMode.Append, FileAccess.Write, FileShare.Write);
                    }
                    else
                    {
                        fs = File.Create(logname);
                    }
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.WriteLine(msg);
                    }
                }
                catch
                {

                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 写非重复内容日志文件,日志文件名为："前缀"+"yyyy-MM-dd.log"
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="logPrefix">日志文件前缀,默认后边跟上"yyyy-MM-dd"</param>
        public static void WriteUnrepeatLine(List<string> msgs, string logPrefix = null)
        {
            if (msgs == null || msgs.Count == 0)
            {
                return;
            }
            lock (lock_UnrepeatLine)
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory + "SpecialLog\\";
                // 创建文件夹
                CreateFolder(basePath);
                string logname = string.Format("{0}\\{1}{2:yyyy-MM-dd}.log", basePath, string.IsNullOrWhiteSpace(logPrefix) ? "" : logPrefix.Trim(), DateTime.Now);
                try
                {
                    List<string> lines = new List<string>();
                    if (File.Exists(logname))
                    {
                        // 获取文件已有内容
                        lines = File.ReadAllLines(logname, Encoding.UTF8).ToList();
                    }
                    using (StreamWriter sw = new StreamWriter(logname, true, Encoding.UTF8))
                    {
                        foreach (string msg in msgs)
                        {
                            if (string.IsNullOrWhiteSpace(msg) || lines.Contains(msg))
                                continue;
                            sw.WriteLine(msg);
                        }
                    }
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        public static void CreateFolder(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        private static readonly object lock_file = new object();
        /// <summary>
        /// 保存数据到文件
        /// </summary>
        /// <param name="filePath">文件全路径</param>
        /// <param name="content">内容</param>
        /// <param name="encoding">编码方式,默认为utf-8</param>
        public static void SaveDataToFile(string filePath, string content, Encoding encoding = null)
        {
            lock (lock_file)
            {
                if (encoding == null)
                {
                    encoding = Encoding.GetEncoding("utf-8");
                }
                try
                {
                    FileStream fs = null;
                    if (File.Exists(filePath))
                    {
                        fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    }
                    else
                    {
                        fs = File.Create(filePath);
                    }
                    using (StreamWriter sw = new StreamWriter(fs, encoding))
                    {
                        sw.Write(content);
                        sw.Close();
                        fs.Close();
                    }
                }
                catch { }
            }
        }
    }
}
