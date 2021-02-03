using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neko.Utility.IO.Logging
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    public sealed class LogDate : INekoLogger
    {
        /// <summary>
        /// 获取当前时间的委托方法
        /// </summary>
        private static ExecuteCode<DateTime> _getNow;

        /// <summary>
        /// 开始记录日志的时间
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// 提交记录日志的时间
        /// </summary>
        private DateTime _endTime;

        /// <summary>
        /// 日志记录周期耗时(单位:毫秒)
        /// </summary>
        private double _timeConsuming;

        /// <summary>
        /// 日志记录总耗时(单位:毫秒)
        /// </summary>
        private double _totalTimeConsuming;

        /// <summary>
        /// 每次记录的日志
        /// </summary>
        private Queue<LogInfo> _logInfos;

        public LogLevel LogLevel { get; set; }
        public double RecordMinimunInterval { get; set; }

        static LogDate()
        {
            _getNow = delegate ()
            {
                return DateTime.Now;
            };
        }

        public LogDate() : this(null)
        {

        }

        public LogDate(string title)
        {
            Reset();
            Begin();
            if (!string.IsNullOrEmpty(title))
            {
                Log(LogLevel.None, 0, title, null, delegate (string state, Exception ex)
                {
                    return string.Format("------------------------------------{0}------------------------------------", title);
                });
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Commit()
        {
            _endTime = _getNow();
            double interval = (_endTime - _startTime).TotalMilliseconds;
            _timeConsuming = interval - _totalTimeConsuming;
            _totalTimeConsuming = interval;
        }

        internal void Commit(LogLevel logLevel, string logMessage, Exception innerException)
        {
            Commit();
            if (!IsEnabled(logLevel))
            {
                return;
            }
            Console.WriteLine("[{0}]{1:HH:mm:ss}:{2}.({3:F2}ms/{4:F2}ms)", logLevel, DateTime.Now, logMessage, _timeConsuming, _totalTimeConsuming);
            lock (_logInfos)
            {
                LogInfo info = new LogInfo(logLevel, DateTime.Now, logMessage, innerException);
                info.TimeConsuming = _timeConsuming;
                info.TotalTimeConsuming = _totalTimeConsuming;
                _logInfos.Enqueue(info);
            }
        }

        public void CommitDebug(string logMessage, params object[] args)
        {
            string message = logMessage;
            if (args != null)
            {
                message = string.Format(logMessage, args);
            }
            Commit(LogLevel.Debug, message, null);
        }

        public void CommitException(string logMessage, params object[] args)
        {
            string message = logMessage;
            if (args != null)
            {
                message = string.Format(logMessage, args);
            }
            CommitException(new Exception(message));
        }

        public void CommitException(Exception exception)
        {
            Commit(LogLevel.Error, exception?.Message, exception);
        }

        public void CommitInformation(string logMessage, params object[] args)
        {
            string message = logMessage;
            if (args != null)
            {
                message = string.Format(logMessage, args);
            }
            Commit(LogLevel.Information, message, null);
        }

        public void CommitTrace(string logMessage, params object[] args)
        {
            string message = logMessage;
            if (args != null)
            {
                message = string.Format(logMessage, args);
            }
            Commit(LogLevel.Trace, message, null);
        }

        public void CommitWarning(string logMessage, params object[] args)
        {
            string message = logMessage;
            if (args != null)
            {
                message = string.Format(logMessage, args);
            }
            Commit(LogLevel.Warning, message, null);
        }

        public void Dispose()
        {
            Commit();
            WriteLog();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            Commit(logLevel, message, exception);
        }

        public void WriteLog()
        {
            if(_totalTimeConsuming < RecordMinimunInterval)
            {
                return;
            }
            try
            {
                do
                {
                    if(_logInfos.Count == 0)
                    {
                        break;
                    }
                    lock (_logInfos)
                    {
                        LogInfo log = _logInfos.Dequeue();
                        LogUtil.WriteLog(log);
                    }
                } while (_logInfos.Count > 0);
            }
            catch
            {
                throw;
            }
            finally
            {
                Reset();
            }
        }

        /// <summary>
        /// 重置计时
        /// </summary>
        private void Reset()
        {
            DateTime resetTime = _getNow();
            _startTime = resetTime;
            _endTime = resetTime;
            _timeConsuming = 0d;
            _totalTimeConsuming = 0d;
            _logInfos = new Queue<LogInfo>();
        }

        /// <summary>
        /// 开始记录日志,记录耗时
        /// </summary>
        private void Begin()
        {
            _startTime = _getNow();
        }
    }
}
