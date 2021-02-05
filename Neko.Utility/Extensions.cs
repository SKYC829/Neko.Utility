using Neko.Utility.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neko.Utility.IO.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using Neko.Utility.Common;
using System.Drawing;
using System.Drawing.Imaging;
using Neko.Utility.IO;
using System.Reflection;
using System.Linq;

namespace Neko.Utility
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class Extensions
    {
        #region 字典扩展
        /// <summary>
        /// <inheritdoc cref="DictionaryUtil.Sort{TKey, TValue}(IDictionary{TKey, TValue})"/>
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dictionary">字典</param>
        /// <returns></returns>
        public static List<KeyValuePair<TKey, TValue>> Sort<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TValue : struct
        {
            return DictionaryUtil.Sort<TKey, TValue>(dictionary);
        }
        #endregion

        #region AspNetCore扩展
        #region 日志扩展
        /// <summary>
        /// 添加AspNetCore日志扩展
        /// </summary>
        /// <param name="services">服务</param>
        /// <param name="logLevel">日志等级</param>
        /// <param name="minimunRecordInterval">最小记录间隔(单位:毫秒)</param>
        /// <param name="isSingleton">
        /// 是否是单例模式
        /// <para>如果为false,则每次请求都会单独记录耗时</para>
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddNekoLog(this IServiceCollection services,LogLevel logLevel,double minimunRecordInterval,bool isSingleton)
        {
            if (isSingleton)
            {
                services.AddNekoLogSingleton(logLevel, minimunRecordInterval);
            }
            else
            {
                services.AddNekoLogScoped(logLevel, minimunRecordInterval);
            }
            return services;
        }

        private static void AddNekoLogSingleton(this IServiceCollection services,LogLevel logLevel,double minimunRecordInterval)
        {
            INekoLogger logger = new LogDate();
            logger.LogLevel = logLevel;
            logger.RecordMinimunInterval = minimunRecordInterval;
            services.AddSingleton<ILogger>(logger);
            services.AddSingleton<INekoLogger>(logger);
        }

        private static void AddNekoLogScoped(this IServiceCollection services, LogLevel logLevel, double minimunRecordInterval)
        {
            services.AddScoped<ILogger>((provider) =>
            {
                IHttpContextAccessor contextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                ILogger log = new LogDate(contextAccessor.HttpContext.Request.Path);
                (log as INekoLogger).LogLevel = logLevel;
                (log as INekoLogger).RecordMinimunInterval = minimunRecordInterval;
                return log;
            });
            services.AddScoped<INekoLogger>((provider) =>
            {
                IHttpContextAccessor contextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                INekoLogger log = new LogDate(contextAccessor.HttpContext.Request.Path);
                log.LogLevel = logLevel;
                log.RecordMinimunInterval = minimunRecordInterval;
                return log;
            });
        }
        #endregion
        #region 依赖注入扩展
        /// <summary>
        /// 自动批量依赖注入
        /// </summary>
        /// <typeparam name="TBaseInterface">基接口</typeparam>
        /// <param name="services">服务</param>
        /// <param name="interfaceAssemblyStr">依赖注入接口所在程序集文件路径</param>
        /// <param name="instanceAssemblyStr">依赖注入实例所在程序集文件路径</param>
        /// <returns></returns>
        public static IServiceCollection AutoDependencyInject<TBaseInterface>(this IServiceCollection services,string interfaceAssemblyStr,string instanceAssemblyStr)
        {
            Type baseType = typeof(TBaseInterface);
            Assembly interfaceAssembly = Assembly.LoadFrom(new FileInfo(interfaceAssemblyStr).FullName);
            Assembly instanceAssembly = Assembly.LoadFrom(new FileInfo(instanceAssemblyStr).FullName);
            if(interfaceAssembly == null)
            {
                throw new FileNotFoundException("无法加载程序集:[{0}],文件不存在!", interfaceAssemblyStr);
            }
            if(instanceAssembly == null)
            {
                throw new FileNotFoundException("无法加载程序集:[{0}],文件不存在!", instanceAssemblyStr);
            }
            IQueryable<Type> injectInterfaces = interfaceAssembly.GetTypes().Where(p => p.IsInterface && baseType.IsAssignableFrom(p)).AsQueryable();
            foreach (Type @interface in injectInterfaces)
            {
                IQueryable<Type> injectInstances = instanceAssembly.GetTypes().Where(p => p.IsClass && @interface.IsAssignableFrom(p) && !p.IsAbstract).AsQueryable();
                foreach (Type instance in injectInstances)
                {
                    IQueryable<Type> implememtInterfaces = instance.GetInterfaces().Where(p => p != baseType).AsQueryable();
                    foreach (Type implememtInterface in implememtInterfaces)
                    {
                        services.AddScoped(implememtInterface, instance);
                    }
                }
            }
            return services;
        }
        #endregion
        #endregion

        #region 文件扩展
        /// <summary>
        /// 获取文件类型
        /// <para>目前仅支持部分图片类型</para>
        /// </summary>
        /// <param name="file">文件信息</param>
        /// <returns></returns>
        public static string GetMimeType(this FileInfo file)
        {
            if(file == null)
            {
                throw new ArgumentNullException(nameof(file), "文件信息不允许为空!");
            }
            if (!file.Exists)
            {
                throw new FileNotFoundException("文件不存在!", file.FullName);
            }
            byte[] fileData = File.ReadAllBytes(file.FullName);
            string defaultResult = file.Extension.Replace(".", "").ToUpper();
            if (fileData == null || fileData.Length < 10)
            {
                return defaultResult;
            }
            if(fileData[0] == 71 && fileData[1] == 73 && fileData[2] == 70)
            {
                return "GIF";
            }
            if(fileData[1] == 80 && fileData[2] == 78 && fileData[3] == 71)
            {
                return "PNG";
            }
            if(fileData[6] == 74 && fileData[7] == 70 && fileData[8] == 73 && fileData[9] == 70)
            {
                return "JPG";
            }
            if(fileData[0] == 66 && fileData[1] == 77)
            {
                return "BMP";
            }
            return defaultResult;
        }
        #endregion

        #region 字符串和时间戳
        /// <summary>
        /// <inheritdoc cref="StringUtil.GetTimeStamp(DateTime)"/>
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <returns></returns>
        public static string ToTimeStamp(this DateTime dateTime)
        {
            return StringUtil.GetTimeStamp(dateTime);
        }

        /// <summary>
        /// <inheritdoc cref="StringUtil.GetTimeStamp(string)"/> 或<inheritdoc cref="StringUtil.GetDateTime(object)"/>
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns></returns>
        public static DateTime? ToDateTime(this string value)
        {
            DateTime? result = null;
            try
            {
                result = StringUtil.GetTimeStamp(value);
            }
            catch
            {
                result = StringUtil.GetDateTime(value);
            }
            return result;
        }
        #endregion

        #region 序列化

        /// <summary>
        /// <inheritdoc cref="SerializeUtil.ToBinary(object)"/>
        /// </summary>
        /// <param name="serializableObj">可序列化对象</param>
        /// <returns></returns>
        public static byte[] ObjectToBinary(this object serializableObj)
        {
            return SerializeUtil.ToBinary(serializableObj);
        }

        /// <summary>
        /// <inheritdoc cref="SerializeUtil.FromBinary(byte[])"/>
        /// </summary>
        /// <typeparam name="TResult">对象类型</typeparam>
        /// <param name="binaryBytes">二进制数据</param>
        /// <returns></returns>
        public static TResult BinaryToObject<TResult>(this byte[] binaryBytes)
        {
            return SerializeUtil.FromBinary<TResult>(binaryBytes);
        }

        /// <summary>
        /// 将图片转换为Base64字符串
        /// </summary>
        /// <param name="image">图片</param>
        /// <param name="format">图片格式</param>
        /// <returns></returns>
        public static string ImageToBase64(this Image image,ImageFormat format)
        {
            string result = string.Empty;
            using (Bitmap bmp = new Bitmap(image))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, format);
                    byte[] imgBytes = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(imgBytes, 0, imgBytes.Length);
                    result = Convert.ToBase64String(imgBytes);
                }
            }
            return result;
        }

        /// <summary>
        /// 将base64字符串转换为图片
        /// </summary>
        /// <param name="base64Str">Base64字符串</param>
        /// <returns></returns>
        public static Image Base64ToImage(this string base64Str)
        {
            byte[] bytes = Convert.FromBase64String(base64Str);
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                Image img = Image.FromStream(ms);
                return new Bitmap(img);
            }
        }
        #endregion

        #region 加密
        /// <summary>
        /// <inheritdoc cref="EncryptionUtil.EncryptMD5(object)"/>
        /// </summary>
        /// <param name="content">待加密对象</param>
        /// <returns></returns>
        public static string MD5(this object content)
        {
            return EncryptionUtil.EncryptMD5(content);
        }

        /// <summary>
        /// <inheritdoc cref="EncryptionUtil.EncryptSHA1(object)"/>
        /// </summary>
        /// <param name="content">待加密对象</param>
        /// <returns></returns>
        public static string SHA1(this object content)
        {
            return EncryptionUtil.EncryptSHA1(content);
        }

        /// <summary>
        /// <inheritdoc cref="EncryptionUtil.EncryptSHA256(object)"/>
        /// </summary>
        /// <param name="content">待加密对象</param>
        /// <returns></returns>
        public static string SHA256(this object content)
        {
            return EncryptionUtil.EncryptSHA256(content);
        }
        #endregion

        #region 数据压缩
        /// <summary>
        /// <inheritdoc cref="ZipExecute.Compress(byte[])"/>
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static byte[] Compress(this byte[] bytes)
        {
            ZipExecute execute = new ZipExecute();
            return execute.Compress(bytes);
        }

        /// <summary>
        /// <inheritdoc cref="ZipExecute.Decompress(byte[])"/>
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static byte[] Decompress(this byte[] bytes)
        {
            ZipExecute execute = new ZipExecute();
            return execute.Decompress(bytes);
        }
        #endregion
    }
}
