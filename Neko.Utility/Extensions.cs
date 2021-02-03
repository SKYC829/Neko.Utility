using Neko.Utility.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neko.Utility.IO.Logging;
using Microsoft.AspNetCore.Http;

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
        #endregion
    }
}
