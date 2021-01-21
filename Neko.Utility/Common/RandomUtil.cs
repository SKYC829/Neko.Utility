using System;
using System.Collections.Generic;
using System.Text;

namespace Neko.Utility.Common
{
    /// <summary>
    /// 随机数操作帮助类
    /// </summary>
    public sealed class RandomUtil
    {
        /// <summary>
        /// 生成一组随机数
        /// </summary>
        /// <param name="random">随机数生成器</param>
        /// <param name="count">随机数生成数量</param>
        /// <param name="canRepeat">是否允许重复</param>
        /// <returns></returns>
        public static int[] Next(Random random,int count,bool canRepeat = false)
        {
            int[] results = new int[count];
            if(random != null)
            {
                try
                {
                    List<int> cache = new List<int>();
                    for (int i = 0; i < count; i++)
                    {
                        int item = random.Next(count + i);
                        if(cache.Contains(item) && !canRepeat)
                        {
                            i--;
                            continue;
                        }
                        results[i] = item;
                        cache.Add(item);
                    }
                }
                catch
                {

                }
            }
            return results;
        }

        /// <summary>
        /// 生成一组随机数
        /// </summary>
        /// <param name="count">随机数生成数量</param>
        /// <param name="canRepeat">是否允许重复</param>
        /// <returns></returns>
        public static int[] Next(int count,bool canRepeat = false)
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            return Next(random, count, canRepeat);
        }

        /// <summary>
        /// 生成固定长度的随机字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <returns></returns>
        public static string GenerateRandomNo(int length)
        {
            int randomDay = new Random().Next(-31, 31);
            double randomMs = (double)new Random().Next(-9999, 9999);
            int seed = Math.Abs(DateTime.Now.AddDays(randomDay).AddMilliseconds(randomMs).ToString("yyyyyMMddHHmmssfffffff.FFFFFFF").GetHashCode());
            int seedTemp = Math.Abs(Guid.NewGuid().GetHashCode());
            seed = seed + seedTemp;
            Random random = new Random(seed);
            int[] cache = new int[length];
            for (int i = 0; i < length; i++)
            {
                int value = new Random((i + 1).GetHashCode()).Next();
                cache[i] = value;
            }
            int[] results = new int[length];
            int max = length;
            string result = string.Empty;
            try
            {
                for (int i = 0; i < length; i++)
                {
                    int cursor = random.Next(0, max - 1);
                    results[i] = cache[cursor];
                    cache[cursor] = cache[max - 1];
                    max--;
                }
                for (int i = 0; i < results.Length; i++)
                {
                    result += results[i].ToString();
                }
                if(result.Length > length)
                {
                    result = result.Substring(0, length);
                }
            }
            catch
            {

            }
            return result;
        }
    }
}
