using System;
using System.Collections.Generic;
using System.Text;

namespace Neko.Utility.Data
{
    /// <summary>
    /// 枚举帮助类
    /// </summary>
    public sealed class EnumUtil
    {
        /// <summary>
        /// 转换一个对象为枚举类型
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <param name="value">对象</param>
        /// <param name="defaultValue">无法转换时的默认值</param>
        /// <returns></returns>
        public static object Convert(Type enumType,string value,object defaultValue)
        {
            object result = defaultValue;
            if (!enumType.IsEnum)
            {
                throw new InvalidCastException("转换类型必须是枚举类型!");
            }
            if (!string.IsNullOrEmpty(value))
            {
                result = Enum.Parse(enumType, value);
            }
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="Convert(Type, string, object)"/>
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <param name="value">对象</param>
        /// <returns></returns>
        public static object Convert(Type enumType, string value)
        {
            return Convert(enumType, value, 0);
        }

        /// <summary>
        /// <inheritdoc cref="Convert(Type, string, object)"/>
        /// </summary>
        /// <typeparam name="TResult">枚举类型</typeparam>
        /// <param name="value">对象</param>
        /// <param name="defaultValue">无法转换时的默认值</param>
        /// <returns></returns>
        public static TResult Convert<TResult>(string value, TResult defaultValue) where TResult : struct
        {
            return (TResult)Convert(typeof(TResult), value, defaultValue);
        }

        /// <summary>
        /// <inheritdoc cref="Convert(Type, string, object)"/>
        /// </summary>
        /// <typeparam name="TResult">枚举类型</typeparam>
        /// <param name="value">对象</param>
        /// <returns></returns>
        public static TResult Convert<TResult>(string value) where TResult : struct
        {
            return Convert<TResult>(value, default(TResult));
        }
    }
}
