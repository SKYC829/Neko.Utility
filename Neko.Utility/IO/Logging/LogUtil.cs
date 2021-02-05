using Microsoft.Extensions.Logging;
using Neko.Utility.Data;
using Neko.Utility.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Neko.Utility.IO.Logging
{
    /// <summary>
    /// 日志帮助类
    /// </summary>
    public sealed class LogUtil
    {

        /// <summary>
        /// 重复日志缓存
        /// </summary>
        private static Dictionary<string, LogInfo> _logCache;

        /// <summary>
        /// 日志队列
        /// </summary>
        private static Queue<LogInfo> _logInfos;

        static LogUtil()
        {
            _logCache = new Dictionary<string, LogInfo>();
            _logInfos = new Queue<LogInfo>();
            StartLog();
        }

        /// <summary>
        /// 开启线程循环输出日志到文件
        /// </summary>
        private static void StartLog()
        {
            ThreadUtil.RunThreadLoop(new IntervalInfo()
            {
                Interval = 100,
                ExecuteCode = LogToFile
            }).Name = "输出日志线程";
        }

        /// <summary>
        /// 输出日志到文件
        /// </summary>
        private static void LogToFile()
        {
            if (_logInfos.Count < 1)
            {
                return;
            }
            do
            {
                StreamWriter writer = null;
                try
                {
                    if (_logInfos.Count == 0)
                    {
                        break;
                    }
                    LogInfo log = null;
                    lock (_logInfos)
                    {
                        log = _logInfos.Dequeue();
                    }
                    if (log == null)
                    {
                        continue;
                    }
                    string logPath = AppDomain.CurrentDomain.BaseDirectory;
                    logPath = Path.Combine(logPath, "Temp", "Logs", log.LogLevel.ToString());
                    if (!Directory.Exists(logPath))
                    {
                        Directory.CreateDirectory(logPath);
                    }
                    string logFileName = string.Format("{0:yyyyyMMdd}.log", DateTime.Today);
                    logFileName = Path.Combine(logPath, logFileName);
                    writer = new StreamWriter(logFileName, true, Encoding.UTF8);
                    if (log.InnerException != null)
                    {
                        if (string.IsNullOrEmpty(log.LogMessage))
                        {
                            log.LogMessage = string.Format("{0}\r\n{1}", log.InnerException.Message, log.InnerException);
                        }
                        else
                        {
                            log.LogMessage = string.Format("{0}\r\n错误信息:{1}\r\n异常堆栈:{2}", log.LogMessage, log.InnerException.Message, log.InnerException.StackTrace);
                        }
                        string cacheKey = EncryptionUtil.EncryptMD5(log.LogMessage);
                        LogInfo cache = DictionaryUtil.Get<LogInfo>(_logCache, cacheKey);
                        if (cache == null)
                        {
                            _logCache[cacheKey] = log;
                        }
                        else
                        {
                            cache.LogCount++;
                            if ((log.LogTime - cache.LogTime).TotalMinutes < 5)
                            {
                                continue;
                            }
                            log.LogMessage = string.Format("{0}(5分钟内触发了{1}次)", log.LogMessage, cache.LogCount);
                            cache.LogCount = 0;
                            cache.LogTime = log.LogTime;
                        }
                    }
                    if (string.IsNullOrEmpty(log.LogMessage))
                    {
                        continue;
                    }
                    string logMessage = string.Format("[{0}]{1:HH:mm:ss}:{2}.({3:F2}ms/{4:F2}ms)\r\n", log.LogLevel, log.LogTime, log.LogMessage, log.TimeConsuming, log.TotalTimeConsuming);
                    if(writer != null)
                    {
                        writer.WriteLine(logMessage);
                    }
                }
                catch (Exception ex)
                {
                    WriteException("记录日志时发生异常!", ex);
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                        writer.Dispose();
                        writer = null;
                    }
                }
            } while (true);
        }

        /// <summary>
        /// <inheritdoc cref="WriteLog(LogLevel, string, Exception)"/>
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="args">日志信息参数</param>
        public static void WriteDebug(string logMessage, params object[] args)
        {
            string message = logMessage;
            if (args != null)
            {
                message = string.Format(logMessage, args);
            }
            WriteLog(LogLevel.Debug, message, null);
        }

        /// <summary>
        /// <inheritdoc cref="WriteLog(LogLevel, string, Exception)"/>
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="args">日志信息参数</param>
        public static void WriteException(string logMessage, params object[] args)
        {
            string message = logMessage;
            if (args != null)
            {
                message = string.Format(logMessage, args);
            }
            WriteException(message, new Exception(message));
        }

        /// <summary>
        /// <inheritdoc cref="WriteLog(LogLevel, string, Exception)"/>
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="exception">异常信息</param>
        public static void WriteException(string logMessage,Exception exception)
        {
            WriteLog(LogLevel.Error, logMessage, exception);
        }

        /// <summary>
        /// <inheritdoc cref="WriteLog(LogLevel, string, Exception)"/>
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="args">日志信息参数</param>
        public static void WriteInformation(string logMessage, params object[] args)
        {
            string message = logMessage;
            if (args != null)
            {
                message = string.Format(logMessage, args);
            }
            WriteLog(LogLevel.Information, message, null);
        }

        /// <summary>
        /// <inheritdoc cref="WriteLog(LogLevel, string, Exception)"/>
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="args">日志信息参数</param>
        public static void WriteTrace(string logMessage, params object[] args)
        {
            string message = logMessage;
            if (args != null)
            {
                message = string.Format(logMessage, args);
            }
            WriteLog(LogLevel.Trace, message, null);
        }

        /// <summary>
        /// <inheritdoc cref="WriteLog(LogLevel, string, Exception)"/>
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="args">日志信息参数</param>
        public static void WriteWarning(string logMessage, params object[] args)
        {
            string message = logMessage;
            if (args != null)
            {
                message = string.Format(logMessage, args);
            }
            WriteLog(LogLevel.Warning, message, null);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="logLevel">日志等级</param>
        /// <param name="logMessage">日志消息</param>
        /// <param name="innerException">异常信息</param>
        public static void WriteLog(LogLevel logLevel,string logMessage,Exception innerException)
        {
            LogInfo log = new LogInfo(logLevel, DateTime.Now, logMessage, innerException);
            log.TimeConsuming = 0;
            log.TotalTimeConsuming = 0;
            WriteLog(log);
        }

        /// <summary>
        /// 添加日志到队列准备输出
        /// </summary>
        /// <param name="info">日志信息</param>
        internal static void WriteLog(LogInfo info)
        {
            if(info == null)
            {
                return;
            }
            lock (_logInfos)
            {
                _logInfos.Enqueue(info);
            }
        }
    }
}
