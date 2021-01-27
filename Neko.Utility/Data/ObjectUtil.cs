using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace Neko.Utility.Data
{
    /// <summary>
    /// 对象帮助类
    /// <para>并不能帮你找到对象</para>
    /// </summary>
    public sealed partial class ObjectUtil
    {
        /// <summary>
        /// 类型对比
        /// </summary>
        /// <param name="typeA">类型A</param>
        /// <param name="typeName">类型B的全称</param>
        /// <returns></returns>
        public static bool TypeEquals(Type typeA, string typeName)
        {
            if(typeA == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(typeName))
            {
                return false;
            }
            if (typeA.FullName.Equals(typeName) || typeA.Name.Equals(typeName))
            {
                return true;
            }
            else if (typeA.FullName.Equals(typeof(object).FullName) || typeA.Name.Equals(typeof(object).Name))
            {
                return false;
            }
            else
            {
                return TypeEquals(typeA.BaseType, typeName);
            }
        }

        /// <summary>
        /// <inheritdoc cref="TypeEquals(Type, string)"/>
        /// </summary>
        /// <param name="typeA">类型A</param>
        /// <param name="typeB">类型B的全称</param>
        /// <returns></returns>
        public static bool TypeEquals(Type typeA,Type typeB)
        {
            return TypeEquals(typeA, typeB.FullName);
        }

        /// <summary>
        /// 判断一个对象是否为空
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static bool IsEmpty(object obj)
        {
            return obj == null || string.IsNullOrEmpty(obj.ToString()) || string.IsNullOrWhiteSpace(obj.ToString());
        }

        /// <summary>
        /// 确定是否可以将指定类型对象分配给当前类型
        /// </summary>
        /// <param name="typeA">与对象类型比较的类型</param>
        /// <param name="value">对象</param>
        /// <returns></returns>
        public static bool IsAssignableTo(Type typeA,object value)
        {
            if (!typeA.IsClass)
            {
                return true;
            }
            if (TypeEquals(typeA, value.GetType()))
            {
                return true;
            }
            if (IsEmpty(value))
            {
                return true;
            }
            if(value is JObject && typeA.IsClass)
            {
                return true;
            }
            return typeA.IsAssignableFrom(value.GetType());
        }

        /// <summary>
        /// 将一个未知类型的数组转换为已知类型的数组
        /// <para>元素无法转换时将会转换为元素类型的默认值</para>
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="list">数组</param>
        /// <returns></returns>
        public static IList<T> ConvertList<T>(IList list)
        {
            List<T> result = new List<T>();
            foreach (var item in list)
            {
                if(item is JValue)
                {
                    result.Add((T)(item as JValue).Value);
                }
                else
                {
                    try
                    {
                        result.Add((T)item);
                    }
                    catch (InvalidCastException)
                    {
                        T res = default;
                        try
                        {
                            res = (T)System.Convert.ChangeType(item, typeof(T));
                        }
                        finally
                        {
                            result.Add(res);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 将对象A的字段和属性的值写入对象B
        /// </summary>
        /// <param name="objectA">对象A</param>
        /// <param name="objectB">对象B</param>
        /// <returns></returns>
        public static object WriteTo(object objectA,object objectB)
        {
            if(objectA == null)
            {
                throw new ArgumentNullException(nameof(objectA), "参数objectA不允许为空!");
            }
            if(objectB == null)
            {
                throw new InvalidOperationException("无法将对象写入空对象!");
            }
            Type typeA = objectA.GetType();
            Type typeB = objectB.GetType();
            foreach (PropertyInfo property in typeA.GetProperties())
            {
                object propertyValue = Get(objectA, property.Name);
                if(propertyValue != null && propertyValue.GetType() == typeof(DBNull))
                {
                    propertyValue = null;
                }
                if(typeB.GetProperty(property.Name) != null && IsAssignableTo(typeB.GetProperty(property.Name).PropertyType, propertyValue))
                {
                    Set(objectB, property.Name, propertyValue);
                }
            }
            foreach (FieldInfo field in typeA.GetFields())
            {
                object fieldValue = Get(objectA, field.Name);
                if(fieldValue != null && fieldValue.GetType() == typeof(DBNull))
                {
                    fieldValue = null;
                }
                if(typeB.GetField(field.Name) != null && IsAssignableTo(typeB.GetField(field.Name).FieldType, fieldValue))
                {
                    Set(objectB, field.Name, fieldValue);
                }
            }
            return objectB;
        }

        /// <summary>
        /// 将对象A转换为对象B
        /// </summary>
        /// <typeparam name="TObject">对象B类型</typeparam>
        /// <param name="objectA">对象A</param>
        /// <param name="objectB">对象B</param>
        /// <returns></returns>
        public static TObject Convert<TObject>(object objectA, TObject objectB) where TObject : class, new()
        {
            if(objectA == null)
            {
                throw new ArgumentNullException(nameof(objectA), "无法转换空对象!");
            }
            if(objectB == null)
            {
                objectB = new TObject();
            }
            if(objectA is TObject)
            {
                return objectA as TObject;
            }
            WriteTo(objectA, objectB);
            return objectB;
        }

        /// <summary>
        /// 将对象转换为另一个类型
        /// </summary>
        /// <typeparam name="TObject">对象类型</typeparam>
        /// <param name="obj">待转换对象</param>
        /// <returns></returns>
        public static TObject Convert<TObject>(object obj) where TObject : class, new()
        {
            if(obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "无法转换空对象!");
            }
            if(obj is TObject)
            {
                return obj as TObject;
            }
            return Convert<TObject>(obj, default(TObject));
        }
    }

    /// <summary>
    /// <remark>取值部分代码</remark>
    /// </summary>
    public sealed partial class ObjectUtil
    {
        /// <summary>
        /// 获取一个对象的指定属性或字段的值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <returns></returns>
        public static object Get(object obj,string fieldName)
        {
            if(obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "无法从空对象中取值!");
            }
            if (IsEmpty(fieldName))
            {
                throw new ArgumentNullException(nameof(fieldName), "参数fieldName不允许为空!");
            }
            object result = null;
            Type objType = obj.GetType();
            if(objType.IsValueType || objType.IsEnum)
            {
                return result;
            }
            else if(obj is IDictionary)
            {
                result = DictionaryUtil.Get<object>(obj as IDictionary<string,object>, fieldName);
            }
            else if(obj is DataRow)
            {
                result = RowUtil.Get((DataRow)obj, fieldName);
            }
            else if(obj is JObject)
            {
                result = Get((obj as JObject)[fieldName]);
            }
            else if (objType.IsClass)
            {
                PropertyInfo property = objType.GetProperty(fieldName);
                if(property != null)
                {
                    result = property.GetValue(obj);
                }
                else
                {
                    FieldInfo field = objType.GetField(fieldName);
                    if (field != null)
                    {
                        result = field.GetValue(obj);
                    }
                }
            }

            if(result != null)
            {
                Type resType = result.GetType();
                Type resGenericType = null;
                if (resType.IsGenericType)
                {
                    resGenericType = resType.GetGenericArguments().FirstOrDefault();
                }
                if(result is JArray ||
                   result is ArrayList ||
                   (resGenericType != null && (TypeEquals(resGenericType,typeof(JObject)) || resGenericType.Equals(typeof(object)))))
                {
                    IList resList = result as IList;
                    List<object> objList = ConvertList<object>(resList).ToList();
                    if(!(obj is JObject))
                    {
                        Set(obj, fieldName, objList);
                    }
                    result = objList;
                }
                else if(result is JObject)
                {
                    result = DictionaryUtil.Parse(result);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取一个<see cref="JToken"/>或<see cref="JValue"/>或<see cref="JProperty"/>的值
        /// </summary>
        /// <param name="obj"><see cref="JToken"/>或<see cref="JValue"/>或<see cref="JProperty"/></param>
        /// <returns></returns>
        internal static object Get(object obj)
        {
            if(obj == null)
            {
                return null;
            }
            if(obj is JToken)
            {
                JToken token = obj as JToken;
                if(token == null)
                {
                    return null;
                }
                else if(token is JValue)
                {
                    JValue value = token as JValue;
                    return value.Value;
                }
                else if(token is JProperty)
                {
                    JProperty property = token as JProperty;
                    return property.Value;
                }
            }
            return obj;
        }

        /// <summary>
        /// <inheritdoc cref="Get(object, string)"/>的指定类型
        /// </summary>
        /// <typeparam name="TResult">值类型</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <returns></returns>
        public static TResult Get<TResult>(object obj,string fieldName)
        {
            object res = Get(obj, fieldName);
            return StringUtil.Get<TResult>(res);
        }

        /// <summary>
        /// <inheritdoc cref="Get(object, string)"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <returns></returns>
        public static bool GetBoolean(object obj,string fieldName)
        {
            return Get<bool>(obj, fieldName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(object, string)"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <returns></returns>
        public static string GetString(object obj,string fieldName)
        {
            return Get<string>(obj, fieldName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(object, string)"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <returns></returns>
        public static int GetInt(object obj,string fieldName)
        {
            return Get<int>(obj, fieldName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(object, string)"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <returns></returns>
        public static double GetDouble(object obj,string fieldName)
        {
            return Get<double>(obj, fieldName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(object, string)"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <returns></returns>
        public static float GetFloat(object obj,string fieldName)
        {
            return Get<float>(obj, fieldName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(object, string)"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <returns></returns>
        public static short GetShort(object obj,string fieldName)
        {
            return Get<short>(obj, fieldName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(object, string)"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <returns></returns>
        public static decimal GetDecimal(object obj,string fieldName)
        {
            return Get<decimal>(obj, fieldName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(object, string)"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <returns></returns>
        public static BigInteger GetBigInteger(object obj,string fieldName)
        {
            return Get<BigInteger>(obj, fieldName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(object, string)"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object obj,string fieldName)
        {
            return Get<DateTime>(obj, fieldName);
        }
    }

    /// <summary>
    /// <remark>设置值部分代码</remark>
    /// </summary>
    public sealed partial class ObjectUtil
    {
        /// <summary>
        /// 设置一个对象的某个属性或字段的值
        /// </summary>
        /// <param name="objType">对象类型</param>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <param name="value">值</param>
        public static void Set(Type objType,object obj,string fieldName,object value)
        {
            PropertyInfo property = objType.GetProperty(fieldName);
            if(property != null)
            {
                property.SetValue(obj, value);
                return;
            }
            FieldInfo field = objType.GetField(fieldName);
            if(field != null)
            {
                field.SetValue(obj, value);
            }
        }

        /// <summary>
        /// <inheritdoc cref="Set(Type, object, string, object)"/>
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">属性或字段的名称</param>
        /// <param name="value">值</param>
        public static void Set(object obj,string fieldName,object value)
        {
            if(obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "参数obj不允许为空!");
            }
            if (IsEmpty(fieldName))
            {
                throw new ArgumentNullException(nameof(fieldName), "参数fieldName不允许为空!");
            }
            Type objType = obj.GetType();
            if(objType.IsValueType || objType.IsEnum)
            {
                throw new NotSupportedException("值类型和枚举类型无法设置值!");
            }
            else if(obj is IDictionary)
            {
                (obj as IDictionary)[fieldName] = value;
            }
            else if(obj is DataRow)
            {
                RowUtil.Set((DataRow)obj, fieldName, value);
            }
            else if(obj is DataTable)
            {
                DataTable dataTable = obj as DataTable;
                if(dataTable != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        Set(row, fieldName, value);
                    }
                }
            }
            else if(obj is JObject)
            {
                (obj as JObject)[fieldName] = new JValue(value);
            }
            else if (objType.IsClass)
            {
                Set(objType, obj, fieldName, value);
            }
        }
    }
}
