using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Neko.Utility.Data
{
    /// <summary>
    /// 字符串帮助类
    /// </summary>
    public sealed partial class StringUtil
    {
        /// <summary>
        /// 比较两个字符串是否一致
        /// </summary>
        /// <param name="valueA">字符串A</param>
        /// <param name="valueB">字符串B</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns></returns>
        public static bool SafeEquals(string valueA,string valueB,bool ignoreCase = false)
        {
            return !string.IsNullOrEmpty(valueA) &&
                   !string.IsNullOrEmpty(valueB) &&
                   valueA.Length.Equals(valueB.Length) &&
                   (string.Compare(valueA, valueB, ignoreCase, System.Globalization.CultureInfo.InvariantCulture).Equals(0));
        }

        /// <summary>
        /// 比较两个对象转换为字符串后是否一致
        /// </summary>
        /// <param name="objectA">对象A</param>
        /// <param name="objectB">对象B</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns></returns>
        public static bool SafeEquals(object objectA,object objectB,bool ignoreCase = false)
        {
            if(objectA == null)
            {
                throw new ArgumentNullException(nameof(objectA), "参数objectA不允许为空!");
            }
            if(objectB == null)
            {
                throw new ArgumentNullException(nameof(objectA), "参数objectB不允许为空!");
            }
            return SafeEquals(objectA.ToString(), objectB.ToString(), ignoreCase);
        }

        /// <summary>
        /// 获取一个时间的Unix时间戳
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public static string GetTimeStamp(DateTime time)
        {
            if(time == null)
            {
                throw new ArgumentNullException(nameof(time), "参数time不允许为空!");
            }
            TimeSpan timeSpan = time.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(timeSpan.TotalMilliseconds).ToString();
        }

        /// <summary>
        /// 获取当前时间的Unix时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            return GetTimeStamp(DateTime.UtcNow);
        }

        /// <summary>
        /// 将Unix时间戳转换为<see cref="DateTime">日期时间</see>
        /// </summary>
        /// <param name="timestamp">Unix时间戳</param>
        /// <returns></returns>
        public static DateTime GetTimeStamp(string timestamp)
        {
            if (string.IsNullOrEmpty(timestamp))
            {
                throw new ArgumentNullException(nameof(timestamp), "无法从空值获取内容!");
            }
            long unixMs = Convert.ToInt64(timestamp);
            DateTime timeStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime result = timeStart.AddMilliseconds(unixMs).ToLocalTime();
            return result;
        }

        /// <summary>
        /// 获取一个N类型的<see cref="Guid"/>
        /// </summary>
        /// <returns></returns>
        public static string GetGUID()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 获取一个日期的开始时间
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns></returns>
        public static DateTime GetDayStart(DateTime date)
        {
            if(date == null)
            {
                throw new ArgumentNullException(nameof(date), "参数date不允许为空!");
            }
            return DateTime.Parse(date.ToString("yyyy-MM-dd 00:00:00"));
        }

        /// <summary>
        /// 获取一个日期的结束时间
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns></returns>
        public static DateTime GetDayEnd(DateTime date)
        {
            if (date == null)
            {
                throw new ArgumentNullException(nameof(date), "参数date不允许为空!");
            }
            return GetDayStart(date).AddDays(1).AddSeconds(-1);
        }

        /// <summary>
        /// 比较日期
        /// </summary>
        /// <param name="fromDate">开始日期</param>
        /// <param name="toDate">结束日期</param>
        /// <returns></returns>
        public static CompareResult CompareDate(DateTime fromDate,DateTime toDate)
        {
            if(fromDate == null || toDate == null)
            {
                return CompareResult.Null;
            }
            fromDate = GetDayStart(fromDate);
            toDate = GetDayStart(toDate);
            if(fromDate > toDate)
            {
                return CompareResult.Big;
            }
            else if(fromDate < toDate)
            {
                return CompareResult.Small;
            }
            else
            {
                return CompareResult.Equals;
            }
        }

        /// <summary>
        /// 比较时间
        /// </summary>
        /// <param name="fromTime">开始时间</param>
        /// <param name="toTime">结束时间</param>
        /// <returns></returns>
        public static CompareResult CompareTime(DateTime fromTime,DateTime toTime)
        {
            if(fromTime == null || toTime == null)
            {
                return CompareResult.Null;
            }
            TimeSpan fromSpan = fromTime.TimeOfDay;
            TimeSpan toSpan = toTime.TimeOfDay;
            if(fromSpan > toSpan)
            {
                return CompareResult.Big;
            }
            else if(fromSpan < toSpan)
            {
                return CompareResult.Small;
            }
            else
            {
                return CompareResult.Equals;
            }
        }
    }

    /// <summary>
    /// 值类型转换部分
    /// </summary>
    public sealed partial class StringUtil
    {
        /// <summary>
        /// 将对象转换为bool类型,当转换失败时,返回默认值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static bool GetBoolean(object obj,bool defaultValue)
        {
            bool result = defaultValue;
            if (!ObjectUtil.IsEmpty(obj))
            {
                result = !SafeEquals(obj, "0", true) &&
                         !SafeEquals(obj, "false", true) &&
                         !SafeEquals(obj, "f", true) &&
                         !SafeEquals(obj, "no", true) &&
                         !SafeEquals(obj, "n", true) &&
                         !SafeEquals(obj, "否", true) &&
                         !SafeEquals(obj, "错", true) &&
                         !SafeEquals(obj, "错误", true) &&
                         !SafeEquals(obj, "不", true) &&
                         !SafeEquals(obj, "不是", true);
            }
            return result;
        }


        /// <summary>
        /// 将对象转换为字符串类型
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string GetString(object obj)
        {
            obj = ObjectUtil.Get(obj);
            return obj == null ? string.Empty : obj.ToString();
        }

        /// <summary>
        /// 将一个对象转换为指定值类型
        /// </summary>
        /// <typeparam name="TResult">值类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static TResult GetValueType<TResult>(object obj) where TResult : struct
        {
            TResult result = default;
            if (!ObjectUtil.IsEmpty(obj))
            {
                if(result is bool)
                {
                    obj = GetBoolean(obj, false);
                }
                else if (typeof(TResult).IsEnum)
                {
                    obj = EnumUtil.Convert<TResult>(GetString(obj));
                }
                if(obj.GetType().GetInterface(nameof(IConvertible)) != null)
                {
                    result = (TResult)Convert.ChangeType(obj, typeof(TResult));
                }
                else
                {
                    result = (TResult)obj;
                }
            }
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="GetBoolean(object, bool)"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static bool GetBoolean(object obj)
        {
            return GetValueType<bool>(obj);
        }

        /// <summary>
        /// 将一个对象转换为int类型
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static int GetInt(object obj)
        {
            return GetValueType<int>(obj);
        }

        /// <summary>
        /// 将一个对象转换为double类型
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static double GetDouble(object obj)
        {
            return GetValueType<double>(obj);
        }

        /// <summary>
        /// 将一个对象转换为float类型
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static float GetFloat(object obj)
        {
            return GetValueType<float>(obj);
        }

        /// <summary>
        /// 将一个对象转换为short类型
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static short GetShort(object obj)
        {
            return GetValueType<short>(obj);
        }

        /// <summary>
        /// 将一个对象转换为decimal类型
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static decimal GetDecimal(object obj)
        {
            return GetValueType<decimal>(obj);
        }

        /// <summary>
        /// 将一个对象转换为biginteger类型,转换失败则返回<see cref="BigInteger.Zero"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static BigInteger GetBigInteger(object obj)
        {
            if(BigInteger.TryParse(GetString(obj),out BigInteger result))
            {
                return result;
            }
            return BigInteger.Zero;
        }

        /// <summary>
        /// 将一个对象转换为datetime类型
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object obj)
        {
            return GetValueType<DateTime>(obj);
        }

        /// <summary>
        /// 将一个对象转换为指定类型
        /// </summary>
        /// <param name="toType">转换类型</param>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static object Get(Type toType,object obj)
        {
            if (ObjectUtil.IsEmpty(obj))
            {
                //throw new ArgumentNullException(nameof(obj), "无法转换空对象!");
                return default;
            }
            object result = default;
            if(toType.IsClass && obj is JObject)
            {
                result = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj), toType);
            }
            else if (toType.IsEnum)
            {
                result = EnumUtil.Convert(toType, GetString(obj));
            }
            else if(ObjectUtil.TypeEquals(toType,typeof(DateTime)) || ObjectUtil.TypeEquals(toType, typeof(DateTime?)))
            {
                result = GetDateTime(obj);
            }
            else if (ObjectUtil.TypeEquals(toType, typeof(BigInteger)))
            {
                result = GetBigInteger(obj);
            }
            else if (ObjectUtil.TypeEquals(toType, typeof(decimal)))
            {
                result = GetDecimal(obj);
            }
            else if (ObjectUtil.TypeEquals(toType, typeof(short)))
            {
                result = GetShort(obj);
            }
            else if (ObjectUtil.TypeEquals(toType, typeof(float)))
            {
                result = GetFloat(obj);
            }
            else if (ObjectUtil.TypeEquals(toType, typeof(double)))
            {
                result = GetDouble(obj);
            }
            else if (ObjectUtil.TypeEquals(toType, typeof(int)))
            {
                result = GetInt(obj);
            }
            else if (ObjectUtil.TypeEquals(toType, typeof(bool)))
            {
                result = GetBoolean(obj);
            }
            else if (ObjectUtil.TypeEquals(toType, typeof(string)))
            {
                result = GetString(obj);
            }
            else
            {
                result = obj;
            }
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="Get(Type, object)"/>
        /// </summary>
        /// <typeparam name="TResult">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static TResult Get<TResult>(object obj)
        {
            return (TResult)Get(typeof(TResult), obj);
        }
    }
}
