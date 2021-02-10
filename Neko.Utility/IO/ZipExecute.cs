using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Neko.Utility.IO
{
    /// <summary>
    /// Zip压缩文件帮助类
    /// </summary>
    public sealed class ZipExecute
    {
        /// <summary>
        /// 当前压缩包内文件/文件夹列表
        /// </summary>
        private static ICollection<FileSystemInfo> _zipStore;

        /// <summary>
        /// 正在压缩事件
        /// </summary>
        public event CompressDelegateCode OnCompress;

        /// <summary>
        /// 正在解压事件
        /// </summary>
        public event CompressDelegateCode OnDecompress;

        /// <summary>
        /// <inheritdoc cref="_zipStore"/>
        /// </summary>
        public IEnumerable<FileSystemInfo> ZipStore { get => _zipStore; }

        public ZipExecute()
        {
            _zipStore = new List<FileSystemInfo>();
        }

        /// <summary>
        /// 向压缩包添加一个文件或文件夹
        /// </summary>
        /// <param name="fileSystem">文件或文件夹信息</param>
        public void Add(FileSystemInfo fileSystem)
        {
            if(fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem), "参数fileSystem不允许为空!");
            }
            if (_zipStore.ToList().Exists(p => p.FullName.Equals(fileSystem.FullName)))
            {
                _zipStore.Remove(fileSystem);
                Add(fileSystem);
            }
            _zipStore.Add(fileSystem);
        }

        /// <summary>
        /// <inheritdoc cref="Add(FileSystemInfo)"/>
        /// </summary>
        /// <param name="fileName">文件或文件夹信息</param>
        public void AddFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName), "参数fileName不允许为空!");
            }
            Add(new FileInfo(fileName));
        }

        /// <summary>
        /// <inheritdoc cref="Add(FileSystemInfo)"/>
        /// </summary>
        /// <param name="directoryName">文件或文件夹信息</param>
        public void AddDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
            {
                throw new ArgumentNullException(nameof(directoryName), "参数fileName不允许为空!");
            }
            Add(new DirectoryInfo(directoryName));
        }

        /// <summary>
        /// 从压缩包内移除一个文件或文件夹
        /// </summary>
        /// <param name="fileSystem">文件或文件夹信息</param>
        public void Remove(FileSystemInfo fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem), "参数fileSystem不允许为空!");
            }
            if (!_zipStore.Contains(fileSystem))
            {
                throw new FileNotFoundException("文件不存在!", fileSystem.Name);
            }
            _zipStore.Remove(fileSystem);
        }

        /// <summary>
        /// <inheritdoc cref="Remove(FileSystemInfo)"/>
        /// </summary>
        /// <param name="fileName">文件或文件夹信息</param>
        public void RemoveFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName), "参数fileName不允许为空!");
            }
            Remove(new FileInfo(fileName));
        }

        /// <summary>
        /// <inheritdoc cref="Remove(FileSystemInfo)"/>
        /// </summary>
        /// <param name="directoryName">文件或文件夹信息</param>
        public void RemoveDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
            {
                throw new ArgumentNullException(nameof(directoryName), "参数fileName不允许为空!");
            }
            Remove(new DirectoryInfo(directoryName));
        }

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="fileName">压缩文件名</param>
        /// <param name="password">压缩文件密码</param>
        public void Compress(string fileName,string password = "")
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName), "参数fileName不允许为空!");
            }
            fileName = FixFileName(fileName);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            try
            {
                using (FileStream outputFile = new FileStream(fileName,FileMode.Append,FileAccess.Write,FileShare.ReadWrite))
                {
                    using (ZipOutputStream outputStream = new ZipOutputStream(outputFile))
                    {
                        outputStream.SetLevel(8);
                        outputStream.Password = password;
                        foreach (var zipFile in _zipStore)
                        {
                            AppendEntry(outputStream, zipFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                throw ex;
            }
        }

        /// <summary>
        /// 压缩字节数组
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public byte[] Compress(byte[] bytes)
        {
            if(bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "参数bytes不允许为空!");
            }
            byte[] result = bytes;
            if(bytes.Length <= 0)
            {
                return result;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipOutputStream outputStream = new GZipOutputStream(ms))
                {
                    outputStream.SetLevel(8);
                    outputStream.Write(bytes, 0, bytes.Length);
                }
                result = ms.ToArray();
            }
            return result;
        }

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="fileName">压缩文件名</param>
        /// <param name="unzipDirectory">解压文件夹</param>
        /// <param name="password">压缩文件密码</param>
        public void Decompress(string fileName,string unzipDirectory,string password = "")
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName), "参数fileName不允许为空!");
            }
            fileName = FixFileName(fileName);
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("文件不存在!", fileName);
            }
            if (!Directory.Exists(unzipDirectory))
            {
                Directory.CreateDirectory(unzipDirectory);
            }
            try
            {
                using (FileStream zipFileStream = new FileStream(fileName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
                {
                    using (ZipInputStream inputStream = new ZipInputStream(zipFileStream))
                    {
                        inputStream.Password = password;
                        ZipEntry entry = null;
                        while ((entry = inputStream.GetNextEntry()) != null)
                        {
                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                continue;
                            }
                            string fullUnzipDirectory = Path.Combine(unzipDirectory, entry.Name);
                            if (entry.IsDirectory)
                            {
                                FileAttributes fileAttributes = FileAttributes.Normal;
                                Directory.CreateDirectory(fullUnzipDirectory);
                                if (entry.Name.StartsWith("."))
                                {
                                    File.SetAttributes(fullUnzipDirectory, fileAttributes | FileAttributes.Hidden);
                                }
                                OnDecompress?.Invoke(entry.Name, 1d, 1d, true);
                            }
                            else if (entry.IsFile)
                            {
                                using (FileStream outputStream = new FileStream(fullUnzipDirectory,FileMode.Append,FileAccess.Write,FileShare.ReadWrite))
                                {
                                    int readNum = 0;
                                    double writeNum = 0;
                                    byte[] dataBytes = new byte[10240];
                                    do
                                    {
                                        readNum = inputStream.Read(dataBytes, 0, dataBytes.Length);
                                        writeNum += (double)readNum;
                                        OnDecompress?.Invoke(entry.Name, writeNum, (double)entry.Size, false);
                                        outputStream.Write(dataBytes, 0, dataBytes.Length);
                                    } while (readNum > 0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Directory.Exists(unzipDirectory))
                {
                    Directory.Delete(unzipDirectory, true);
                }
                throw ex;
            }
        }

        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public byte[] Decompress(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "参数bytes不允许为空!");
            }
            byte[] result = bytes;
            if (bytes.Length <= 0)
            {
                return result;
            }
            using (MemoryStream inputStream = new MemoryStream(bytes))
            {
                using (GZipInputStream unzipSyream = new GZipInputStream(inputStream))
                {
                    using (MemoryStream outputStream = new MemoryStream())
                    {
                        byte[] dataBytes = new byte[10240];
                        int readNum = 0;
                        do
                        {
                            readNum = unzipSyream.Read(dataBytes, 0, dataBytes.Length);
                            outputStream.Write(dataBytes, 0, readNum);
                        } while (readNum > 0);
                        result = outputStream.ToArray();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 追加文件到压缩文件夹内
        /// </summary>
        /// <param name="outputStream">压缩文件输出流</param>
        /// <param name="fileSystem">文件或文件夹</param>
        /// <param name="root">文件或文件夹的根目录</param>
        private void AppendEntry(ZipOutputStream outputStream, FileSystemInfo fileSystem, string root = "")
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream), "参数outputStream不允许为空!");
            }
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem), "参数fileSystem不允许为空!");
            }
            if (!fileSystem.Exists)
            {
                throw new FileNotFoundException("文件不存在!", fileSystem.FullName);
            }
            ZipEntry entry = null;
            if (fileSystem.Attributes.HasFlag(FileAttributes.Directory))
            {
                entry = new ZipEntry(Path.Combine(root, fileSystem.Name + "\\"));
                outputStream.PutNextEntry(entry);
                OnCompress?.Invoke(fileSystem.Name, 1d, 1d, true);
                (fileSystem as DirectoryInfo).GetFileSystemInfos().ToList().ForEach((directory) =>
                {
                    AppendEntry(outputStream, directory, entry.Name);
                });
            }
            else
            {
                entry = new ZipEntry(Path.Combine(root, fileSystem.Name));
                outputStream.PutNextEntry(entry);
                using (FileStream readStream = new FileStream(fileSystem.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] dataBytes = new byte[10240];
                    int readNum = 0;
                    double readSize = 0d;
                    while (readStream.Position < readStream.Length)
                    {
                        readNum = readStream.Read(dataBytes, 0, dataBytes.Length);
                        readSize += (double)readNum;
                        OnCompress?.Invoke(fileSystem.Name, readSize, (double)readStream.Length, false);
                        outputStream.Write(dataBytes, 0, dataBytes.Length);
                    }
                }
            }
        }

        private string FixFileName(string fileName)
        {
            if (!Path.GetExtension(fileName).ToLower().Equals(".zip"))
            {
                fileName = string.Format("{0}.zip", fileName);
            }
            return fileName;
        }
    }
}
