using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neko.Utility.Common
{
    /// <summary>
    /// 委托调用代码帮助类
    /// </summary>
    public sealed class InvokeCode
    {
        private readonly List<Delegate> _codeStacks;

        /// <summary>
        /// 待执行方法队列
        /// </summary>
        public IEnumerable<Delegate> CodeStacks { get => _codeStacks; }

        /// <summary>
        /// 是否中断队列执行
        /// </summary>
        public bool IsBreak { get; set; }

        /// <summary>
        /// 是否已执行到队列的最后一个方法
        /// </summary>
        public bool IsFinaly { get => _codeStacks != null && _codeStacks.Count == 0; }

        public InvokeCode()
        {
            _codeStacks = new List<Delegate>();
        }

        /// <summary>
        /// 添加一个委托方法到执行队列的末尾
        /// </summary>
        /// <param name="executeCode">委托方法</param>
        public void Add(Delegate executeCode)
        {
            lock (_codeStacks)
            {
                _codeStacks.Add(executeCode);
            }
        }

        /// <summary>
        /// 将一个委托方法插入到执行队列的列头
        /// </summary>
        /// <param name="executeCode">委托方法</param>
        public void Shift(Delegate executeCode)
        {
            Insert(0, executeCode);
        }

        /// <summary>
        /// 在执行队列的指定位置插入一个委托方法
        /// </summary>
        /// <param name="index">位置索引</param>
        /// <param name="executeCode">委托方法</param>
        public void Insert(int index,Delegate executeCode)
        {
            lock (_codeStacks)
            {
                _codeStacks.Insert(index, executeCode);
            }
        }

        /// <summary>
        /// 获取一个委托方法在队列的位置
        /// </summary>
        /// <param name="executeCode">委托方法</param>
        /// <returns></returns>
        public int IndexOf(Delegate executeCode)
        {
            lock (_codeStacks)
            {
                return _codeStacks.IndexOf(executeCode);
            }
        }

        /// <summary>
        /// 根据位置索引从执行队列中取出一个委托方法
        /// </summary>
        /// <param name="index">位置索引</param>
        /// <returns></returns>
        public Delegate ElementAt(int index)
        {
            lock (_codeStacks)
            {
                return _codeStacks.ElementAt(index);
            }
        }

        /// <summary>
        /// 删除执行队列中指定位置索引的委托方法
        /// </summary>
        /// <param name="index">位置索引</param>
        public void RemoveAt(int index)
        {
            Delegate executeCode = ElementAt(index);
            Remove(executeCode);
        }

        /// <summary>
        /// 从执行队列中删除一个委托方法
        /// </summary>
        /// <param name="executeCode">委托方法</param>
        public void Remove(Delegate executeCode)
        {
            lock (_codeStacks)
            {
                _codeStacks.Remove(executeCode);
            }
        }

        /// <summary>
        /// 清空执行队列
        /// </summary>
        public void Clear()
        {
            lock (_codeStacks)
            {
                _codeStacks.Clear();
            }
        }

        /// <summary>
        /// 以异步的方式<inheritdoc cref="ExecuteNext"/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteNextAsync()
        {
            if(_codeStacks.Count == 0)
            {
                return;
            }
            Delegate executeCode = null;
            lock (_codeStacks)
            {
                executeCode = _codeStacks.FirstOrDefault();
            }
            if(executeCode == null)
            {
                return;
            }
            Remove(executeCode);
            await Task.Run(new Action(() =>
            {
                executeCode.DynamicInvoke();
            }));
        }

        /// <summary>
        /// 以异步的方式<inheritdoc cref="Execute"/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            if(_codeStacks.Count == 0)
            {
                return;
            }
            while (_codeStacks.Count > 0)
            {
                if (IsBreak)
                {
                    break;
                }
                await ExecuteNextAsync();
            }
        }

        /// <summary>
        /// 执行队列中的第一个委托方法
        /// </summary>
        public void ExecuteNext()
        {
            ExecuteNextAsync().Wait();
        }

        /// <summary>
        /// 逐一执行队列中的委托方法
        /// </summary>
        public void Execute()
        {
            ExecuteAsync().Wait();
        }
    }
}
