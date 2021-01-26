using System;
using System.Collections.Generic;
using System.Text;

namespace Neko.Utility
{
    /// <summary>
    /// 比对结果枚举
    /// </summary>
    public enum CompareResult
    {
        /// <summary>
        /// 无法比对
        /// </summary>
        Null = 0,

        /// <summary>
        /// 较小
        /// </summary>
        Small = 1,

        /// <summary>
        /// 相等
        /// </summary>
        Equals = 2,

        /// <summary>
        /// 较大
        /// </summary>
        Big = 3
    }
}
