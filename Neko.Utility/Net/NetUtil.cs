using Neko.Utility.Data;
using Neko.Utility.IO;
using Neko.Utility.IO.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Neko.Utility.Net
{
    /// <summary>
    /// 网络帮助类
    /// </summary>
    public sealed class NetUtil
    {
        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIP()
        {
            string result = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(p => p.AddressFamily.Equals(AddressFamily.InterNetwork)).MapToIPv4().ToString();
            return result;
        }

        /// <summary>
        /// 获取本机公网IP地址或指定主机公网地址
        /// <para>参数host为空时获取本机公网IP地址,否则获取指定域名公网IP地址</para>
        /// </summary>
        /// <param name="host">主机域名</param>
        /// <returns></returns>
        public static string GetIP(string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                return GetNetIP();
            }
            return Dns.GetHostAddresses(host).FirstOrDefault(p => p.AddressFamily.Equals(AddressFamily.InterNetwork)).MapToIPv4().ToString();
        }

        /// <summary>
        /// 获取公网IP
        /// </summary>
        /// <returns></returns>
        public static string GetNetIP()
        {
            string result = string.Empty;
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp("http://www.net.cn/static/customercare/yourip.asp");
                using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    string response = reader.ReadToEnd();
                    result = RegularUtil.Get(response, RegularDefaults.IPADDRESS);
                }
            }
            catch (SocketException ex)
            {
                LogUtil.WriteException("获取公网地址失败!网络连接错误!", ex);
            }
            return result;
        }

        /// <summary>
        /// 获取本地计算机与一个域名的延迟
        /// </summary>
        /// <param name="host">域名</param>
        /// <param name="timeout">超时时间(单位:秒)</param>
        /// <returns></returns>
        public static PingReply Ping(string host,int timeout)
        {
            return new Ping().Send(host, timeout);
        }

        /// <summary>
        /// <inheritdoc cref="Ping(string, int)"/>
        /// </summary>
        /// <param name="host">域名</param>
        /// <returns></returns>
        public static int Ping(string host)
        {
            PingReply reply = Ping(host, 5);
            return StringUtil.GetInt(reply.Status.Equals(IPStatus.Success) ? reply.RoundtripTime : int.MaxValue);
        }

        /// <summary>
        /// 获取本地计算机与公网的延迟
        /// </summary>
        /// <returns></returns>
        public static int Ping()
        {
            return Ping("www.baidu.com");
        }
    }
}
