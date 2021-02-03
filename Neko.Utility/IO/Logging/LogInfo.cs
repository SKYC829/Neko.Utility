using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neko.Utility.IO.Logging
{
    /// <summary>
    /// 日志信息实体类
    /// </summary>
    [Serializable]
    internal class LogInfo
    {
        /// <summary>
        /// 记录日志时间
        /// </summary>
        public DateTime LogTime { get; set; }

        /// <summary>
        /// 日志信息
        /// </summary>
        public string LogMessage { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception InnerException { get; set; }

        /// <summary>
        /// 短时间内同一日志出现次数
        /// </summary>
        public int LogCount { get; set; }

        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// 日志记录周期耗时(单位:毫秒)
        /// </summary>
        public double TimeConsuming { get; set; }

        /// <summary>
        /// 日志记录总耗时(单位:毫秒)
        /// </summary>
        public double TotalTimeConsuming { get; set; }

        public LogInfo(LogLevel logLevel, DateTime logTime, Exception innerException) : this(logLevel, logTime, string.Empty, innerException)
        {

        }

        public LogInfo(LogLevel logLevel, DateTime logTime, string logMessage) : this(logLevel, logTime, logMessage, null)
        {
        }

        public LogInfo(LogLevel logLevel, DateTime logTime, string logMessage, Exception innerException)
        {
            LogLevel = logLevel;
            LogTime = logTime;
            LogMessage = logMessage;
            InnerException = innerException;
        }
    }
}
