using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Neko.Utility.IO.Logging
{
    /// <summary>
    /// 日志记录器接口
    /// </summary>
    public interface INekoLogger : ILogger, IDisposable
    {
        /// <summary>
        /// 最小记录间隔(单位:毫秒)
        /// </summary>
        double RecordMinimunInterval { get; set; }

        /// <summary>
        /// 记录日志的等级
        /// <para>小于此等级的日志不会被记录</para>
        /// </summary>
        LogLevel LogLevel { get; set; }

        /// <summary>
        /// 记录<see cref="LogLevel.Trace"/>日志
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="args">日志信息参数</param>
        void CommitTrace(string logMessage, params object[] args);

        /// <summary>
        /// 记录<see cref="LogLevel.Information"/>日志
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="args">日志参数</param>
        void CommitInformation(string logMessage, params object[] args);

        /// <summary>
        /// 记录<see cref="LogLevel.Warning"/>日志
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="args">日志参数</param>
        void CommitWarning(string logMessage, params object[] args);

        /// <summary>
        /// 记录<see cref="LogLevel.Error"/>日志
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="args">日志参数</param>
        void CommitException(string logMessage, params object[] args);

        /// <summary>
        /// <inheritdoc cref="CommitException(string, object[])"/>
        /// </summary>
        /// <param name="exception">异常信息</param>
        void CommitException(Exception exception);

        /// <summary>
        /// 记录<see cref="LogLevel.Debug"/>日志
        /// </summary>
        /// <param name="logMessage">日志信息</param>
        /// <param name="args">日志参数</param>
        void CommitDebug(string logMessage, params object[] args);

        /// <summary>
        /// 记录日志
        /// </summary>
        void Commit();

        /// <summary>
        /// 输出日志内容到文件
        /// </summary>
        void WriteLog();
    }
}
