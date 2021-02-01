using Neko.Utility.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Neko.Utility.IO
{
    /// <summary>
    /// Xml帮助类
    /// </summary>
    public sealed partial class XmlUtil
    {
        /// <summary>
        /// 将一个对象序列化为Xml格式的二进制数据
        /// </summary>
        /// <param name="serializableObj">可序列化对象</param>
        /// <returns></returns>
        public static byte[] ToXml(object serializableObj)
        {
            if (ObjectUtil.IsEmpty(serializableObj))
            {
                throw new ArgumentNullException(nameof(serializableObj), "参数serializableObject不允许为空!");
            }
            byte[] result = new byte[0];
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(serializableObj.GetType());
                xmlSerializer.Serialize(ms, serializableObj);
                result = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(result, 0, result.Length);
            }
            return result;
        }

        /// <summary>
        /// 将一个对象序列化为Xml格式的字符串
        /// </summary>
        /// <param name="serializableObj">可序列化对象</param>
        /// <returns></returns>
        public static string ToXmlString(object serializableObj)
        {
            byte[] res = ToXml(serializableObj);
            return Encoding.UTF8.GetString(res);
        }

        /// <summary>
        /// 将一个xml数据反序列化为一个对象
        /// </summary>
        /// <param name="xmlStream">xml数据</param>
        /// <param name="objType">对象类型</param>
        /// <returns></returns>
        public static object FromXml(Stream xmlStream, Type objType)
        {
            if (xmlStream == null)
            {
                throw new ArgumentNullException(nameof(xmlStream), "参数xmlStream不允许为空!");
            }
            if (objType == null)
            {
                throw new ArgumentNullException(nameof(objType), "参数objType不允许为空!");
            }
            XmlSerializer xmlSerializer = new XmlSerializer(objType);
            return xmlSerializer.Deserialize(xmlStream);
        }

        /// <summary>
        /// <inheritdoc cref="FromXml(Stream, Type)"/>
        /// </summary>
        /// <param name="xmlBytes">xml数据</param>
        /// <param name="objType">对象类型</param>
        /// <returns></returns>
        public static object FromXml(byte[] xmlBytes, Type objType)
        {
            if (xmlBytes == null || xmlBytes.Length <= 0)
            {
                throw new ArgumentNullException(nameof(xmlBytes), "参数xmlBytes不允许为空!");
            }
            if (objType == null)
            {
                throw new ArgumentNullException(nameof(objType), "参数objType不允许为空!");
            }
            using (MemoryStream ms = new MemoryStream(xmlBytes))
            {
                return FromXml(ms, objType);
            }
        }

        /// <summary>
        /// <inheritdoc cref="FromXml(byte[], Type)"/>
        /// </summary>
        /// <param name="xmlString">xml数据</param>
        /// <param name="objType">对象类型</param>
        /// <returns></returns>
        public static object FromXml(string xmlString, Type objType)
        {
            if (string.IsNullOrEmpty(xmlString))
            {
                throw new ArgumentNullException(nameof(xmlString), "参数xmlString不允许为空!");
            }
            if (objType == null)
            {
                throw new ArgumentNullException(nameof(objType), "参数objType不允许为空!");
            }
            return FromXml(Encoding.UTF8.GetBytes(xmlString), objType);
        }

        /// <summary>
        /// <inheritdoc cref="FromXml(Stream, Type)"/>
        /// </summary>
        /// <typeparam name="TResult">对象类型</typeparam>
        /// <param name="xmlStream">xml数据</param>
        /// <returns></returns>
        public static TResult FromXml<TResult>(Stream xmlStream)
        {
            return StringUtil.Get<TResult>(FromXml(xmlStream, typeof(TResult)));
        }

        /// <summary>
        /// <inheritdoc cref="FromXml(byte[], Type)"/>
        /// </summary>
        /// <typeparam name="TResult">对象类型</typeparam>
        /// <param name="xmlBytes">xml数据</param>
        /// <returns></returns>
        public static TResult FromXml<TResult>(byte[] xmlBytes)
        {
            return StringUtil.Get<TResult>(FromXml(xmlBytes, typeof(TResult)));
        }

        /// <summary>
        /// <inheritdoc cref="FromXml(string, Type)"/>
        /// </summary>
        /// <typeparam name="TResult">对象类型</typeparam>
        /// <param name="xmlString">xml数据</param>
        /// <returns></returns>
        public static TResult FromXml<TResult>(string xmlString)
        {
            return StringUtil.Get<TResult>(FromXml(xmlString, typeof(TResult)));
        }
    }

    /// <summary>
    /// Xml文档节点操作部分
    /// </summary>
    public sealed partial class XmlUtil
    {
        /// <summary>
        /// 判断一个xml节点列表是否为空
        /// </summary>
        /// <param name="nodeList">xml节点列表</param>
        /// <returns></returns>
        public static bool IsEmpty(XmlNodeList nodeList)
        {
            return nodeList == null || nodeList.Count == 0;
        }

        /// <summary>
        /// 获取xml节点
        /// </summary>
        /// <param name="xmlNode">根节点</param>
        /// <param name="attributeName">要获取的节点包含的特性</param>
        /// <param name="attributeValue">要获取的节点包含的特性的值</param>
        /// <returns></returns>
        public static XmlElement GetElement(XmlNode xmlNode, string attributeName, string attributeValue)
        {
            if (xmlNode == null)
            {
                throw new ArgumentNullException(nameof(xmlNode), "参数xmlNode不允许为空!");
            }
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new ArgumentNullException(nameof(attributeName), "参数attributeName不允许为空!");
            }
            XmlElement result = xmlNode.ChildNodes.Cast<XmlElement>().Where(p => p != null && p.HasAttribute(attributeName) && p.GetAttribute(attributeName).Equals(attributeValue)).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// 根据标签获取xml节点
        /// </summary>
        /// <param name="xmlNode">根节点</param>
        /// <param name="tags">标签</param>
        /// <returns></returns>
        public static IEnumerable<XmlElement> GetElementsByTag(XmlNode xmlNode, params string[] tags)
        {
            if (xmlNode == null)
            {
                throw new ArgumentNullException(nameof(xmlNode), "参数xmlNode不允许为空!");
            }
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags), "参数tags不允许为空!");
            }
            IEnumerable<XmlElement> result = xmlNode.ChildNodes.Cast<XmlElement>().Where(p => Array.IndexOf(tags, p.Name) > -1);
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="GetElementsByTag(XmlNode, string[])"/>
        /// </summary>
        /// <param name="xmlNode">根节点</param>
        /// <param name="tag">标签</param>
        /// <returns></returns>
        public static XmlElement GetElementByTag(XmlNode xmlNode, string tag)
        {
            return GetElementsByTag(xmlNode, new string[] { tag }).FirstOrDefault();
        }
    }

    /// <summary>
    /// 值操作部分
    /// </summary>
    public sealed partial class XmlUtil
    {
        /// <summary>
        /// 设置xml元素特性的值
        /// </summary>
        /// <param name="xmlElement">xml元素</param>
        /// <param name="attributeName">特性名称</param>
        /// <param name="attributeValue">特性的值</param>
        public static void Set(XmlElement xmlElement, string attributeName, object attributeValue)
        {
            if (xmlElement == null)
            {
                throw new ArgumentNullException(nameof(xmlElement), "参数xmlElement不允许为空!");
            }
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new ArgumentNullException(nameof(attributeName), "参数attributeName不允许为空!");
            }
            xmlElement.SetAttribute(attributeName, StringUtil.GetString(attributeValue));
        }

        /// <summary>
        /// 获取xml元素特性的值
        /// </summary>
        /// <param name="xmlElement">xml元素</param>
        /// <param name="attributeName">特性名称</param>
        /// <returns></returns>
        public static string Get(XmlElement xmlElement, string attributeName)
        {
            if (xmlElement == null)
            {
                throw new ArgumentNullException(nameof(xmlElement), "参数xmlElement不允许为空!");
            }
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new ArgumentNullException(nameof(attributeName), "参数attributeName不允许为空!");
            }
            return xmlElement.GetAttribute(attributeName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(XmlElement, string)"/>
        /// </summary>
        /// <typeparam name="TResult">值类型</typeparam>
        /// <param name="xmlElement">xml元素</param>
        /// <param name="attributeName">特性名称</param>
        /// <returns></returns>
        public static TResult Get<TResult>(XmlElement xmlElement, string attributeName)
        {
            TResult result = default;
            string value = Get(xmlElement, attributeName);
            if (!string.IsNullOrEmpty(value))
            {
                result = StringUtil.Get<TResult>(value);
            }
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="Get(XmlElement, string)"/>
        /// </summary>
        /// <param name="xmlElement">xml元素</param>
        /// <param name="attributeName">特性名称</param>
        /// <returns></returns>
        public static bool GetBoolean(XmlElement xmlElement, string attributeName)
        {
            return Get<bool>(xmlElement, attributeName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(XmlElement, string)"/>
        /// </summary>
        /// <param name="xmlElement">xml元素</param>
        /// <param name="attributeName">特性名称</param>
        /// <returns></returns>
        public static int GetInt(XmlElement xmlElement, string attributeName)
        {
            return Get<int>(xmlElement, attributeName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(XmlElement, string)"/>
        /// </summary>
        /// <param name="xmlElement">xml元素</param>
        /// <param name="attributeName">特性名称</param>
        /// <returns></returns>
        public static double GetDouble(XmlElement xmlElement, string attributeName)
        {
            return Get<double>(xmlElement, attributeName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(XmlElement, string)"/>
        /// </summary>
        /// <param name="xmlElement">xml元素</param>
        /// <param name="attributeName">特性名称</param>
        /// <returns></returns>
        public static float GetFloat(XmlElement xmlElement, string attributeName)
        {
            return Get<float>(xmlElement, attributeName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(XmlElement, string)"/>
        /// </summary>
        /// <param name="xmlElement">xml元素</param>
        /// <param name="attributeName">特性名称</param>
        /// <returns></returns>
        public static short GetShort(XmlElement xmlElement, string attributeName)
        {
            return Get<short>(xmlElement, attributeName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(XmlElement, string)"/>
        /// </summary>
        /// <param name="xmlElement">xml元素</param>
        /// <param name="attributeName">特性名称</param>
        /// <returns></returns>
        public static decimal GetDecimal(XmlElement xmlElement, string attributeName)
        {
            return Get<decimal>(xmlElement, attributeName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(XmlElement, string)"/>
        /// </summary>
        /// <param name="xmlElement">xml元素</param>
        /// <param name="attributeName">特性名称</param>
        /// <returns></returns>
        public static DateTime? GetDateTime(XmlElement xmlElement, string attributeName)
        {
            return Get<DateTime>(xmlElement, attributeName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(XmlElement, string)"/>
        /// </summary>
        /// <param name="xmlElement">xml元素</param>
        /// <param name="attributeName">特性名称</param>
        /// <returns></returns>
        public static BigInteger GetBigInteger(XmlElement xmlElement, string attributeName)
        {
            return Get<BigInteger>(xmlElement, attributeName);
        }
    }
}
