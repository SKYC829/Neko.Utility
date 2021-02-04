using Neko.Utility.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Neko.Utility.Net
{
    /// <summary>
    /// Http Post/Get请求帮助类
    /// </summary>
    public class HttpUtil
    {
        /// <summary>
        /// 超时时间(单位:毫秒)
        /// <para>默认:100秒</para>
        /// </summary>
        public static int Timeout { get; set; }

        static HttpUtil()
        {
            Timeout = 100000;
        }

        /// <summary>
        /// 发起HttpGet请求
        /// </summary>
        /// <param name="url">请求路径</param>
        /// <param name="queryParams">请求参数</param>
        /// <param name="charset">字符编码</param>
        /// <returns></returns>
        public static string HttpGet(string url,IDictionary<string,string> queryParams,string charset)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url), "参数url不允许为空!");
            }
            if (string.IsNullOrEmpty(charset))
            {
                charset = "utf-8";
            }
            if(queryParams != null && queryParams.Count > 0)
            {
                string param = GetQueryParameter(queryParams, charset);
                if (url.Contains("?"))
                {
                    url = string.Format("{0}&{1}", url, param);
                }
                else
                {
                    url = string.Format("{0}?{1}", url, param);
                }
            }
            HttpWebRequest request = CreateWebRequest(url, "GET");
            request.ContentType = string.Format("application/x-www-form-urlencoded;charset={0}", charset);
            string result;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                result = SerializeUtil.ToJson(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 发起HttpPost请求
        /// </summary>
        /// <param name="url">请求路径</param>
        /// <param name="queryParams">请求参数</param>
        /// <param name="formData">表单参数</param>
        /// <param name="attachments">文件参数</param>
        /// <param name="charset">字符编码</param>
        /// <returns></returns>
        public static string HttpPost(string url,IDictionary<string,string> queryParams,IDictionary<string,string> formData = null,IDictionary<string,FileInfo> attachments = null,string charset = "utf-8")
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url), "参数url不允许为空!");
            }
            if (string.IsNullOrEmpty(charset))
            {
                charset = "utf-8";
            }
            if (queryParams != null && queryParams.Count > 0)
            {
                string param = GetQueryParameter(queryParams, charset);
                if (url.Contains("?"))
                {
                    url = string.Format("{0}&{1}", url, param);
                }
                else
                {
                    url = string.Format("{0}?{1}", url, param);
                }
            }
            string boundary = DateTime.Now.Ticks.ToString("X");
            HttpWebRequest request = CreateWebRequest(url, "POST");
            request.ContentType = string.Format("multipart/form-data;charset={0};boundary={1}", charset, boundary);
            Stream requestStream = request.GetRequestStream();
            byte[] boundaryStart = Encoding.GetEncoding(charset).GetBytes(string.Format("\r\n--{0}\r\n", boundary));
            byte[] boundaryEnd = Encoding.GetEncoding(charset).GetBytes(string.Format("\r\n--{0}--\r\n", boundary));
            string formdataFormat = "Content-Disposition:form-data;name=\"{0}\"\r\nContent-Type:text/plain\r\n\r\n{1}";
            string attachFormat = "Content-Disposition:form-data;name=\"{0}\";filename=\"{1}\"\r\nContent-Type:{2}\r\n\r\n";
            if(formData != null || attachments != null)
            {
                if (formData != null && formData.Count > 0)
                {
                    foreach (var param in formData)
                    {
                        string data = string.Format(formdataFormat, param.Key, param.Value);
                        byte[] dataBytes = Encoding.GetEncoding(charset).GetBytes(data);
                        requestStream.Write(boundaryStart, 0, boundaryStart.Length);
                        requestStream.Write(dataBytes, 0, dataBytes.Length);
                    }
                }
                if (attachments != null && attachments.Count > 0)
                {
                    foreach (var attach in attachments)
                    {
                        string data = string.Format(attachFormat, attach.Key, attach.Value.FullName, GetMimeType(attach.Value.GetMimeType()));
                        byte[] dataBytes = Encoding.GetEncoding(charset).GetBytes(data);
                        byte[] fileBytes = File.ReadAllBytes(attach.Value.FullName);
                        requestStream.Write(boundaryStart, 0, boundaryStart.Length);
                        requestStream.Write(dataBytes, 0, dataBytes.Length);
                        requestStream.Write(fileBytes, 0, fileBytes.Length);
                    }
                }
                requestStream.Write(boundaryEnd, 0, boundaryEnd.Length);
            }
            requestStream.Close();
            string result;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                result = SerializeUtil.ToJson(ex.Message);
            }
            return result;
        }

        protected static string GetQueryParameter(IDictionary<string,string> queryParams,string charset)
        {
            StringBuilder sb = new StringBuilder();
            bool flag = false;
            foreach (var param in queryParams)
            {
                string key = param.Key;
                string value = param.Value;
                if (!string.IsNullOrEmpty(key))
                {
                    if (flag)
                    {
                        sb.Append("&");
                    }
                    sb.Append(key);
                    sb.Append("=");
                    sb.Append(HttpUtility.UrlEncode(value, Encoding.GetEncoding(charset)));
                    flag = true;
                }
            }
            return sb.ToString();
        }

        protected static HttpWebRequest CreateWebRequest(string url,string method)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.ServicePoint.Expect100Continue = false;
            request.Method = method;
            request.KeepAlive = true;
            request.UserAgent = "NekoSKYC829";
            request.Timeout = Timeout;
            request.Headers.Add("Access-Control-Allow-Origin", "*"); //跨域头
            return request;
        }

        protected static string GetMimeType(string mimeType)
        {
            switch (mimeType)
            {
                case "JPG":
                    return "image/jpeg";
                case "GIF":
                    return "image/gif";
                case "PNG":
                    return "image/png";
                case "BMP":
                    return "image/bmp";
                case "SOL":
                case "SOR":
                case "TXT":
                    return "text/plain";
                default:
                    return "application/octet-stream"; //对应 .*文件
            }
        }
    }
}
