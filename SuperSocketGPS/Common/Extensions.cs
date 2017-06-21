using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace SuperSocketGPS.Common
{
    public static class Extensions
    {
        /// <summary>
        /// 大端模式int转byte[]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] IntToBytesBig(this int value)
        {
            byte[] src = new byte[4];
            src[0] = (byte) ((value >> 24) & 0xFF);
            src[1] = (byte) ((value >> 16) & 0xFF);
            src[2] = (byte) ((value >> 8) & 0xFF);
            src[3] = (byte) (value & 0xFF);
            return src;
        }

        /// <summary>
        /// 大端模式ushort转byte[]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] UshortToBytesBig(this ushort value)
        {
            byte[] src = new byte[2];
            src[0] = (byte) ((value >> 8) & 0xFF);
            src[1] = (byte) (value & 0xFF);
            return src;
        }

         /// <summary>
        /// byte[]转十进制字符串
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string ByteArrToStr(this IList<byte> src)
        {
            return src == null ? string.Empty : src.Aggregate("", (current, b) => current + b.ToString("D2"));
        }

        /// <summary>
        /// byte[]转十六进制字符串
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string ByteArrToHexStr(this IList<byte> src)
        {
            //BitConverter.ToString(src).Replace("-", "");
            return src == null ? string.Empty : src.Aggregate("", (current, b) => current + b.ToString("X2"));
        }

        /// <summary>
        /// 十六进制字符串转byte[]
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static byte[] HexStrToByteArr(this string src)
        {
            if (string.IsNullOrEmpty(src)) return null;
            var array = new byte[src.Length / 2];
            for (var i = 0; i < src.Length / 2; i++)
            {
                var str = src.Substring(i * 2, 2);
                array[i] = (byte)Convert.ToInt32(str, 16);
            }
            return array;
        }

        /// <summary>
        /// 获取枚举值中文描述
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum enumValue)
        {
            var str = enumValue.ToString();
            var field = enumValue.GetType().GetField(str);
            var objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs.Length == 0) return str;
            var attr = (DescriptionAttribute)objs[0];
            return attr.Description;
        }

        public static string ToLogFormat(this string str)
        {
            return $"[{DateTime.Now:MM/dd HH:mm:ss}] {str}\r\n";
        }

        public static void AddLog(string str)
        {
            var logBox = ((MainForm)Program.AppForm).LogBox;
            if (logBox == null) return;
             if (logBox.InvokeRequired)
                {
                    logBox.Invoke(new MethodInvoker(() =>
                    {
                        logBox.AppendText(str.ToLogFormat());
                        logBox.SelectionStart = logBox.TextLength;
                        logBox.ScrollToCaret();
                    }));
                }
                else
                {
                    logBox.AppendText(str.ToLogFormat());
                    logBox.SelectionStart = logBox.TextLength;
                    logBox.ScrollToCaret();
                }
        }
    }
}
