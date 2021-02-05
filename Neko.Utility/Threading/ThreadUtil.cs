using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Neko.Utility.Threading
{
    /// <summary>
    /// 线程帮助类
    /// </summary>
    public sealed class ThreadUtil
    {
        /// <summary>
        /// <inheritdoc cref="Thread.Sleep(int)"/>
        /// </summary>
        /// <param name="interval">休眠时间(单位:毫秒)</param>
        public static void Sleep(int interval)
        {
            Thread.Sleep(interval);
        }

        /// <summary>
        /// 创建并运行一个新线程
        /// </summary>
        /// <param name="executeCode">线程执行的方法</param>
        /// <returns></returns>
        public static Thread RunThread(ExecuteCode executeCode)
        {
            if (executeCode == null)
            {
                throw new ArgumentNullException(nameof(executeCode), "参数executeCode不允许为空!");
            }
            ThreadStart thread = new ThreadStart(delegate ()
            {
                executeCode.Invoke();
            });
            Thread executeThread = new Thread(thread);
            executeThread.IsBackground = true;
            executeThread.Start();
            return executeThread;
        }

        /// <summary>
        /// 创建并循环运行一个新线程
        /// </summary>
        /// <param name="intervalInfo">线程间隔信息</param>
        /// <returns></returns>
        public static Thread RunThreadLoop(IntervalInfo intervalInfo)
        {
            if (intervalInfo == null)
            {
                throw new ArgumentNullException(nameof(intervalInfo), "参数intervalInfo不允许为空!");
            }
            Thread executeThread = RunThread(delegate ()
            {
                do
                {
                    if (intervalInfo.ExecuteCode == null)
                    {
                        intervalInfo.IsBreak = true;
                    }
                    intervalInfo.ExecuteCode.Invoke();
                } while (!intervalInfo.IsBreak);
            });
            intervalInfo.Current = executeThread;
            return executeThread;
        }
    }
}
