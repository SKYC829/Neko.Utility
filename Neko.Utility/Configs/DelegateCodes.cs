﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Neko.Utility
{
    /// <summary>
    /// 压缩通知委托方法
    /// </summary>
    /// <param name="fileName">当前文件或文件夹名称</param>
    /// <param name="compressedSize">已压缩或解压的文件大小</param>
    /// <param name="fileSize">文件总大小</param>
    /// <param name="isFolder">当前是否是文件夹(true:文件夹,flase:文件)</param>
    public delegate void CompressDelegateCode(string fileName,double compressedSize, double fileSize,bool isFolder);

    /// <summary>
    /// 空参数委托方法
    /// </summary>
    public delegate void ExecuteCode();

    /// <summary>
    /// 有返回值的<inheritdoc cref="ExecuteCode"/>
    /// </summary>
    /// <typeparam name="TResult">返回值类型</typeparam>
    /// <returns></returns>
    public delegate TResult ExecuteCode<TResult>();
}
