using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Neko.Utility.Common
{
    /// <summary>
    /// 序列化帮助类
    /// </summary>
    public sealed class SerializeUtil
    {
        /// <summary>
        /// 将一个对象序列化为二进制数据
        /// </summary>
        /// <param name="serializableObj">可序列化对象</param>
        /// <returns></returns>
        public static byte[] ToBinary(object serializableObj)
        {
            if(serializableObj == null)
            {
                throw new ArgumentNullException(nameof(serializableObj), "无法序列化空对象!");
            }
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, serializableObj);
                byte[] result = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(result, 0, result.Length);
                return result;
            }
        }

        /// <summary>
        /// 将一个二进制数据流反序列化为一个对象
        /// </summary>
        /// <param name="binaryStream">二进制数据流</param>
        /// <returns></returns>
        public static object FromBinary(Stream binaryStream)
        {
            if(binaryStream == null)
            {
                throw new ArgumentNullException(nameof(binaryStream), "无法反序列化空数据流!");
            }
            BinaryFormatter formatter = new BinaryFormatter();
            object result = formatter.Deserialize(binaryStream);
            return result;
        }

        /// <summary>
        /// 将一组二进制数据反序列化为一个对象
        /// </summary>
        /// <param name="binaryBytes">二进制数据</param>
        /// <returns></returns>
        public static object FromBinary(byte[] binaryBytes)
        {
            if(binaryBytes == null)
            {
                throw new ArgumentNullException(nameof(binaryBytes), "无法反序列化空二进制数据!");
            }
            if(binaryBytes.Length <= 0)
            {
                throw new ArgumentException("二进制数据格式错误!", nameof(binaryBytes));
            }
            using (MemoryStream ms = new MemoryStream(binaryBytes))
            {
                return FromBinary(ms);
            }
        }

        /// <summary>
        /// <inheritdoc cref="FromBinary(byte[])"/>
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="binaryBytes">二进制数据</param>
        /// <returns></returns>
        public static T FromBinary<T>(byte[] binaryBytes)
        {
            if (binaryBytes == null)
            {
                throw new ArgumentNullException(nameof(binaryBytes), "无法反序列化空二进制数据!");
            }
            if (binaryBytes.Length <= 0)
            {
                throw new ArgumentException("二进制数据格式错误!", nameof(binaryBytes));
            }
            object serializeRes = FromBinary(binaryBytes);
            Type resType = serializeRes.GetType();
            if(resType.GetInterface(nameof(IConvertible)) != null)
            {
                return (T)Convert.ChangeType(serializeRes, typeof(T));
            }
            else
            {
                return (T)serializeRes;
            }
        }

        /// <summary>
        /// 将一个对象序列化为Json字符串
        /// </summary>
        /// <param name="serializableObj">可序列化对象</param>
        /// <returns></returns>
        public static string ToJson(object serializableObj)
        {
            return JsonConvert.SerializeObject(serializableObj);
        }

        /// <summary>
        /// 将一个Json字符串反序列化为一个对象
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public static object FromJson(string json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        /// <summary>
        /// <inheritdoc cref="FromJson(string)"/>
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 获取Json中一个节点的值
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <param name="key">节点名称</param>
        /// <returns></returns>
        public static object GetJsonValue(string json,string key)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json), "json格式错误!");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "节点名称不允许为空!");
            }
            object jobj = JsonConvert.DeserializeObject(json);
            if (!((JObject)jobj).ContainsKey(key))
            {
                throw new ArgumentException("节点不存在于json字符串中!", nameof(key));
            }
            object result = ((JValue)((JObject)jobj)[key]).Value;
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="GetJsonValue(string, string)"/>
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <param name="key">节点名称</param>
        /// <returns></returns>
        public static T GetJsonValue<T>(string json,string key)
        {
            object result = GetJsonValue(json, key);
            return (T)result;
        }
    }
}
