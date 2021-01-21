using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Neko.Utility.Common
{
    /// <summary>
    /// 程序集反射帮助类
    /// </summary>
    public sealed class ReferenceUtil
    {
        /// <summary>
        /// 获取当前程序的程序集
        /// </summary>
        /// <returns></returns>
        public static Assembly GetDefaultAssembly()
        {
            return Assembly.GetEntryAssembly();
        }

        /// <summary>
        /// 根据名称获取程序集
        /// </summary>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns></returns>
        public static Assembly GetAssemblyByName(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                return GetDefaultAssembly();
            }
            assemblyName = assemblyName.Trim();
            Assembly assembly = Assembly.Load(assemblyName);
            return assembly;
        }

        /// <summary>
        /// 从文件加载程序集
        /// </summary>
        /// <param name="dllName">文件路径</param>
        /// <returns></returns>
        public static Assembly GetAssemblyByDll(string dllName)
        {
            if (string.IsNullOrEmpty(dllName))
            {
                return GetDefaultAssembly();
            }
            dllName = dllName.Trim();
            if (!File.Exists(dllName))
            {
                throw new FileNotFoundException("程序集" + dllName + "不存在或已删除!", dllName);
            }
            Assembly result = Assembly.LoadFrom(dllName);
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="GetAssemblyByName(string)"/>或<inheritdoc cref="GetAssemblyByDll(string)"/>
        /// </summary>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns></returns>
        public static Assembly GetAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                return GetDefaultAssembly();
            }
            else if (assemblyName.Trim().ToLower().EndsWith(".dll"))
            {
                return GetAssemblyByDll(assemblyName);
            }
            else
            {
                return GetAssemblyByName(assemblyName);
            }
        }

        /// <summary>
        /// 获取程序集下特定类型
        /// </summary>
        /// <param name="typeFullName">类型全称</param>
        /// <returns></returns>
        public static Type GetType(string typeFullName)
        {
            if (string.IsNullOrEmpty(typeFullName))
            {
                throw new ArgumentNullException(nameof(typeFullName), "获取程序集下特定类型时,类型全称不允许为空!");
            }
            string assemblyName = string.Empty;
            if (typeFullName.IndexOf(',') > -1)
            {
                List<string> typeNameCollection = new List<string>(typeFullName.Split(','));
                typeFullName = typeNameCollection.FirstOrDefault();
                typeNameCollection.RemoveAt(0);
                assemblyName = string.Join(",", typeNameCollection.ToArray()).Trim();
            }
            typeFullName = typeFullName.Trim();
            Assembly assembly = GetAssembly(assemblyName);
            Type result = GetType(assembly, typeFullName);
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="GetType(string)"/>
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="typeName">类型名称</param>
        /// <returns></returns>
        public static Type GetType(Assembly assembly, string typeName)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly), "获取程序集下特定类型时,程序集不允许为空!");
            }
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException(nameof(typeName), "获取程序集下特定类型时,类型全称不允许为空!");
            }
            typeName = typeName.Trim();
            Type result = assembly.GetTypes().FirstOrDefault(p => p.FullName.Equals(typeName) || p.Name.Equals(typeName));
            return result;
        }

        /// <summary>
        /// 根据类型名称实例化对象
        /// </summary>
        /// <param name="typeFullName">类型全称</param>
        /// <param name="constructParams">构造函数参数</param>
        /// <returns></returns>
        public static object Instantiate(string typeFullName,params object[] constructParams)
        {
            Type type = GetType(typeFullName);
            if(type == null)
            {
                throw new InvalidOperationException("无法获取指定类型" + typeFullName);
            }
            if(constructParams == null)
            {
                throw new ArgumentNullException(nameof(constructParams), string.Format("生成构造函数时,构造参数不允许为空!"));
            }
            Type[] paramTypes = new Type[constructParams.Length];
            for (int i = 0; i < constructParams.Length; i++)
            {
                object param = constructParams.ElementAt(i);
                if(param == null)
                {
                    throw new ArgumentNullException(nameof(param), string.Format("生成构造函数时,构造参数不允许为空!\r\n第{0}个参数生成异常!", i));
                }
                Type paraType = param.GetType();
                paramTypes[i] = paraType;
            }
            ConstructorInfo constructor = type.GetConstructor(paramTypes);
            if(constructor == null)
            {
                throw new InvalidOperationException("未能正确加载对象类型" + typeFullName + ",无法获取到有效的构造函数!");
            }
            object result = constructor.Invoke(constructParams);
            return result;
        }

        /// <summary>
        /// <inheritdoc cref="Instantiate(string, object[])"/>
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="typeFullName">类型全称</param>
        /// <param name="constructParams">构造函数参数</param>
        /// <returns></returns>
        public static T Instantiate<T>(string typeFullName, params object[] constructParams) where T : class
        {
            object result = Instantiate(typeFullName, constructParams);
            if(result == null)
            {
                return null;
            }
            return result as T;
        }
    }
}
