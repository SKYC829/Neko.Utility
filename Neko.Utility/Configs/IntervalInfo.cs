using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Neko.Utility
{
    /// <summary>
    /// 线程间隔信息
    /// </summary>
    public class IntervalInfo
    {
        /// <summary>
        /// 当前线程
        /// </summary>
        public Thread Current { get; set; }

        /// <summary>
        /// 循环执行的间隔时间(单位:毫秒)
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// 线程执行的方法
        /// </summary>
        public ExecuteCode ExecuteCode { get; set; }

        /// <summary>
        /// 是否中断执行
        /// </summary>
        public bool IsBreak { get; set; }
    }
}
