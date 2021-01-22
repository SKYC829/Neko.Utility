using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Neko.Utility.Data
{
    /// <summary>
    /// 键值对帮助类
    /// </summary>
    public sealed class DictionaryUtil
    {
        /// <summary>
        /// 从键值对中获取一个Key的值
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dictionary">字典</param>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static TValue Get<TKey,TValue>(IDictionary<TKey,TValue> dictionary,TKey key)
        {
            TValue result = default;
            if(dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary), "无法从空字典中取值!");
            }
            if (!dictionary.ContainsKey(key))
            {
                return result;
            }
            dictionary.TryGetValue(key, out result);
            return result;
        } 

        /// <summary>
        /// <inheritdoc cref="Get{TKey, TValue}(IDictionary{TKey, TValue}, TKey)"/>
        /// </summary>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dictionary">字典</param>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static TValue Get<TValue>(IDictionary<string,TValue> dictionary,string key)
        {
            return Get<string, TValue>(dictionary, key);
        }

        /// <summary>
        /// 将一个对象转换为字典
        /// </summary>
        /// <param name="parsableObj">对象</param>
        /// <returns></returns>
        public static IDictionary Parse(object parsableObj)
        {
            IDictionary result = default;
            if (parsableObj == null)
            {
                throw new NullReferenceException("参数parsableObj不允许为空!");
            }
            try
            {
                if(parsableObj is IDictionary)
                {
                    result = parsableObj as IDictionary;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(parsableObj);
                    result = JsonConvert.DeserializeObject<IDictionary>(json);
                }
            }
            finally
            {
                if(result == null)
                {
                    result = new Dictionary<string, object>();
                    if(parsableObj is JObject)
                    {
                        JObject jobj = parsableObj as JObject;
                        foreach (KeyValuePair<string,JToken> obj in jobj)
                        {
                            string key = obj.Key;
                            if(obj.Value is JValue)
                            {
                                JValue value = (JValue)obj.Value;
                                result[key] = value;
                            }
                            else
                            {
                                result[key] = null;
                            }
                        }
                    }
                    else if (parsableObj.GetType().IsClass)
                    {
                        PropertyInfo[] properties = parsableObj.GetType().GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (!property.CanRead)
                            {
                                continue;
                            }
                            result[property.Name] = property.GetValue(parsableObj);
                        }
                        FieldInfo[] fields = parsableObj.GetType().GetFields();
                        foreach (FieldInfo field in fields)
                        {
                            string key = field.Name;
                            if (result.Contains(key))
                            {
                                key = string.Format("fi_{0}", key);
                            }
                            result[key] = field.GetValue(parsableObj);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 排序一个字典的元素
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dictionary">字典</param>
        /// <returns></returns>
        public static List<KeyValuePair<TKey,TValue>> Sort<TKey,TValue>(IDictionary<TKey,TValue> dictionary) where TValue : struct
        {
            if(dictionary == null)
            {
                throw new NullReferenceException("参数dictionary不允许为空");
            }
            List<KeyValuePair<TKey, TValue>> result = new List<KeyValuePair<TKey, TValue>>();
            result.AddRange(dictionary);
            result.Sort(delegate (KeyValuePair<TKey, TValue> previewItem, KeyValuePair<TKey, TValue> nextItem)
            {
                dynamic previewValue = previewItem.Value;
                dynamic nextValue = nextItem.Value;
                return (int)(previewValue - nextValue);
            });
            return result;
        }
    }
}
