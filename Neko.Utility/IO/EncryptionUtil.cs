using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using Neko.Utility.Data;
using Neko.Utility.Common;
using System.IO;

namespace Neko.Utility.IO
{
    /// <summary>
    /// 加密帮助类
    /// </summary>
    public sealed partial class EncryptionUtil
    {
        /// <summary>
        /// 获取和计算密钥
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="keyLength">密钥长度</param>
        /// <param name="iv"><see cref="SymmetricAlgorithm.IV"/></param>
        /// <returns></returns>
        private static byte[] GetKeyBytes(string key,int keyLength)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "参数key不允许为空!");
            }
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] result = new byte[keyLength];
            if(keyBytes.Length > keyLength)
            {
                Buffer.BlockCopy(keyBytes, 0, result, 0, keyLength);
            }
            else
            {
                Buffer.BlockCopy(keyBytes, 0, result, 0, keyBytes.Length);
            }
            return result;
        }

        /// <summary>
        /// 创建加密解密器工厂
        /// </summary>
        /// <typeparam name="TFactory">加密解密器类型</typeparam>
        /// <param name="key">密钥</param>
        /// <param name="keyLength">密钥长度</param>
        /// <returns></returns>
        private static TFactory CreateEncryptFactory<TFactory>(string key,int keyLength) where TFactory : SymmetricAlgorithm,new()
        {
            SymmetricAlgorithm provider = new TFactory();
            provider.Mode = CipherMode.CBC;
            provider.Padding = PaddingMode.PKCS7;
            if (string.IsNullOrEmpty(key))
            {
                key = "SK201217YC_NEKO0829";
            }
            provider.Key = GetKeyBytes(key, keyLength);
            provider.IV = GetKeyBytes(key, keyLength);
            return (TFactory)provider;
        }

        /// <summary>
        /// 加密字节数组
        /// </summary>
        /// <param name="cryptoTransform">加密器</param>
        /// <param name="value">字节数组</param>
        /// <returns></returns>
        public static byte[] Encrypt(ICryptoTransform cryptoTransform,byte[] value)
        {
            if(cryptoTransform == null)
            {
                throw new ArgumentNullException(nameof(cryptoTransform), "参数cryptoTransform不允许为空!");
            }
            if(value == null)
            {
                throw new ArgumentNullException(nameof(value), "参数value不允许为空!");
            }
            if(value.Length <= 0)
            {
                return value;
            }
            byte[] result = cryptoTransform.TransformFinalBlock(value, 0, value.Length);
            return result;
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="content">待加密的内容</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static byte[] EncryptAES(object content,string key)
        {
            if (ObjectUtil.IsEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            RijndaelManaged managed = CreateEncryptFactory<RijndaelManaged>(key, 16);
            byte[] contentBytes = Encoding.UTF8.GetBytes(SerializeUtil.ToJson(content));
            byte[] results = Encrypt(managed.CreateEncryptor(), contentBytes);
            managed.Dispose();
            return results;
        }

        /// <summary>
        /// <inheritdoc cref="EncryptAES(object, string)"/>
        /// </summary>
        /// <param name="content">待加密的内容</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string EncryptAESString(object content,string key)
        {
            byte[] res = EncryptAES(content, key);
            if(res == null || res.Length <= 0)
            {
                return string.Empty;
            }
            return Convert.ToBase64String(res);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="content">已加密的对象的二进制数组</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static object DeencryptAES(byte[] content,string key)
        {
            if (content == null || content.Length <= 0)
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            RijndaelManaged managed = CreateEncryptFactory<RijndaelManaged>(key, 16);
            byte[] result = Encrypt(managed.CreateDecryptor(), content);
            managed.Dispose();
            return SerializeUtil.FromJson(Encoding.UTF8.GetString(result));
        }

        /// <summary>
        /// <inheritdoc cref="DeencryptAES(byte[], string)"/>
        /// </summary>
        /// <typeparam name="TResult">解密对象类型</typeparam>
        /// <param name="content">已加密的对象</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static TResult DeencryptAES<TResult>(string content,string key)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            byte[] contentBytes = Convert.FromBase64String(content);
            object res = DeencryptAES(contentBytes, key);
            return StringUtil.Get<TResult>(res);
        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="content">待加密的内容</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static byte[] EncryptDES(object content,string key)
        {
            if (ObjectUtil.IsEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            DESCryptoServiceProvider managed = CreateEncryptFactory<DESCryptoServiceProvider>(key, 8);
            byte[] contentBytes = Encoding.UTF8.GetBytes(SerializeUtil.ToJson(content));
            byte[] result = Encrypt(managed.CreateEncryptor(), contentBytes);
            managed.Dispose();
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="EncryptDES(object, string)"/>
        /// </summary>
        /// <param name="content">待加密的内容</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string EncryptDESString(object content, string key)
        {
            byte[] res = EncryptDES(content, key);
            if(res == null || res.Length <= 0)
            {
                return string.Empty;
            }
            return Convert.ToBase64String(res);
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="content">已加密的对象的二进制数组</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static object DeencryptDES(byte[] content, string key)
        {
            if (content == null || content.Length <= 0)
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            DESCryptoServiceProvider managed = CreateEncryptFactory<DESCryptoServiceProvider>(key, 8);
            byte[] res = Encrypt(managed.CreateDecryptor(), content);
            managed.Dispose();
            return SerializeUtil.FromJson(Encoding.UTF8.GetString(res));
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <typeparam name="TResult">解密对象类型</typeparam>
        /// <param name="content">已加密的对象</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static TResult DeencryptDES<TResult>(string content,string key)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            byte[] contentBytes = Convert.FromBase64String(content);
            object res = DeencryptDES(contentBytes, key);
            return StringUtil.Get<TResult>(res);
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="content">待加密的对象</param>
        /// <returns></returns>
        public static string EncryptMD5(object content)
        {
            if (ObjectUtil.IsEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            MD5CryptoServiceProvider managed = new MD5CryptoServiceProvider();
            byte[] contentBytes = Encoding.UTF8.GetBytes(SerializeUtil.ToJson(content));
            byte[] res = managed.ComputeHash(contentBytes);
            string result = string.Empty;
            for (int i = 0; i < res.Length; i++)
            {
                result += res[i].ToString("x2");
            }
            managed.Dispose();
            return result;
        }

        /// <summary>
        /// SHA256加密
        /// </summary>
        /// <param name="content">待加密的对象</param>
        /// <returns></returns>
        public static string EncryptSHA256(object content)
        {
            if (ObjectUtil.IsEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            SHA256CryptoServiceProvider managed = new SHA256CryptoServiceProvider();
            byte[] contentBytes = Encoding.UTF8.GetBytes(SerializeUtil.ToJson(content));
            byte[] resultBytes = managed.ComputeHash(contentBytes);
            string result = string.Empty;
            for (int i = 0; i < resultBytes.Length; i++)
            {
                result += resultBytes[i].ToString("x2");
            }
            managed.Dispose();
            return result;
        }

        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="content">待加密的对象</param>
        /// <returns></returns>
        public static string EncryptSHA1(object content)
        {
            if (ObjectUtil.IsEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            SHA1CryptoServiceProvider managed = new SHA1CryptoServiceProvider();
            byte[] contentBytes = Encoding.UTF8.GetBytes(SerializeUtil.ToJson(content));
            byte[] resultBytes = managed.ComputeHash(contentBytes);
            string result = string.Empty;
            for (int i = 0; i < resultBytes.Length; i++)
            {
                result += resultBytes[i].ToString("x2");
            }
            managed.Dispose();
            return result;
        }
    }

    /// <summary>
    /// 非对称加密部分
    /// </summary>
    public sealed partial class EncryptionUtil
    {
        /// <summary>
        /// 生成一对RSA密钥
        /// <para>
        /// <see cref="ValueTuple{T1,T2}.Item1">返回值Item1:私钥</see>
        /// <see cref="ValueTuple{T1,T2}.Item1">返回值Item2:公钥</see>
        /// </para>
        /// </summary>
        /// <returns></returns>
        public static ValueTuple<string,string> GeneralRSAKey()
        {
            RSACryptoServiceProvider managed = new RSACryptoServiceProvider();
            return new ValueTuple<string, string>(managed.ToXmlString(true), managed.ToXmlString(false));
        }

        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="content">待加密的对象</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static byte[] EncryptRSA(object content,string publicKey)
        {
            if (ObjectUtil.IsEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            if (string.IsNullOrEmpty(publicKey))
            {
                throw new ArgumentNullException(nameof(publicKey), "参数公钥不允许为空!");
            }
            RSACryptoServiceProvider managed = new RSACryptoServiceProvider();
            try
            {
                managed.FromXmlString(publicKey);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("参数公钥注入失败!\r\n{0}", ex.Message), ex);
            }
            int bufferSize = (managed.KeySize / 8) - 11;
            byte[] buffer = new byte[bufferSize];
            byte[] result = new byte[0];
            using (MemoryStream contentStream = new MemoryStream(Encoding.UTF8.GetBytes(SerializeUtil.ToJson(content))))
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    while (contentStream.Position < contentStream.Length)
                    {
                        int readNum = contentStream.Read(buffer, 0, buffer.Length);
                        byte[] encryptBytes = new byte[readNum];
                        Buffer.BlockCopy(buffer, 0, encryptBytes, 0, readNum);
                        byte[] res = managed.Encrypt(encryptBytes, false);
                        outputStream.Write(res, 0, res.Length);
                    }
                    result = outputStream.ToArray();
                }
            }
            managed.Dispose();
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="EncryptRSA(object, string)"/>
        /// </summary>
        /// <param name="content">待加密的对象</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static string EncryptRSAString(object content,string publicKey)
        {
            byte[] res = EncryptRSA(content, publicKey);
            return Convert.ToBase64String(res);
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="content">已加密的对象的二进制数组</param>
        /// <param name="privateKey">私钥</param>
        /// <returns></returns>
        public static object DeencryptRSA(byte[] content,string privateKey)
        {
            if (content == null || content.Length <= 0)
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            if (string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentNullException(nameof(privateKey), "参数私钥不允许为空!");
            }
            RSACryptoServiceProvider managed = new RSACryptoServiceProvider();
            try
            {
                managed.FromXmlString(privateKey);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("参数私钥注入失败!\r\n{0}", ex.Message), ex);
            }
            byte[] resultBytes = new byte[0];
            int bufferSize = (managed.KeySize / 8);
            byte[] buffer = new byte[bufferSize];
            object result = null;
            using (MemoryStream contentStream = new MemoryStream(content))
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    while (contentStream.Position < contentStream.Length)
                    {
                        int readNum = contentStream.Read(buffer, 0, buffer.Length);
                        byte[] encryptBytes = new byte[readNum];
                        Buffer.BlockCopy(buffer, 0, encryptBytes, 0, encryptBytes.Length);
                        byte[] res = managed.Decrypt(encryptBytes, false);
                        outputStream.Write(res, 0, res.Length);
                    }
                    resultBytes = outputStream.ToArray();
                }
            }
            result = SerializeUtil.FromJson(Encoding.UTF8.GetString(resultBytes));
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="DeencryptRSA(byte[], string)"/>
        /// </summary>
        /// <typeparam name="TResult">已加密的对象的类型</typeparam>
        /// <param name="content">已加密的对象</param>
        /// <param name="privateKey">私钥</param>
        /// <returns></returns>
        public static TResult DeencryptRSA<TResult>(string content,string privateKey)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "参数content不允许为空!");
            }
            byte[] contentBytes = Convert.FromBase64String(content);
            object res = DeencryptRSA(contentBytes, privateKey);
            return StringUtil.Get<TResult>(res);
        }
    }
}
