using Neko.Utility.Data;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
