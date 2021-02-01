using System;
using System.Collections.Generic;
using System.Text;

namespace Neko.Utility
{
    /// <summary>
    /// 一些默认的正则表达式
    /// </summary>
    public static class RegularDefaults
    {
        /// <summary>
        /// 匹配邮箱地址的正则表达式
        /// </summary>
        public const string EMAIL = @"\w[-\w.+]*@([A-Za-z0-9][-A-Za-z0-9]+\.)+[A-Za-z]{2,14}";

        /// <summary>
        /// 中国大陆手机号的正则表达式
        /// </summary>
        public const string MAINLAND_CELLPHONE = @"1[3-8][0-9]{9}";

        /// <summary>
        /// 中国台湾手机号的正则表达式
        /// </summary>
        public const string TAIWAN_CELLPHONE = @"09[0-9]{8}";

        /// <summary>
        /// 中国香港手机号的正则表达式
        /// </summary>
        public const string HONGKONG_CELLPHONE = @"(5|6|8|9)[0-9]{7}";

        /// <summary>
        /// 中国澳门手机号的正则表达式
        /// </summary>
        public const string MACAO_CELLPHONE = @"6(6|8)[0-9]{5}";

        /// <summary>
        /// IPV4的正则表达式
        /// </summary>
        public const string IPADDRESS = @"(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)\.(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)";

        /// <summary>
        /// 中华人民共和国居民身份证二代身份证(18位)的正则表达式
        /// </summary>
        public const string MAINLAND_IDCARD_18 = @"[1-9]\d{5}(18|19|([23]\d))\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\d{3}[0-9Xx]";

        /// <summary>
        /// 中华人民共和国居民身份证一代身份证(15位)的正则表达式
        /// </summary>
        public const string MAINLAND_IDCARD_15 = @"[1-9]\d{5}\d{2}((0[1-9])|(10|11|12))(([0-2][1-9])|10|20|30|31)\d{3}";

        /// <summary>
        /// 网址的正则表达式
        /// </summary>
        public const string WEB_URL = @"((https|http|ftp|rtsp|mms)?:\/\/)[^\s]+";

        /// <summary>
        /// 中文字符
        /// </summary>
        public const string CHINESE_CHARACTER = @"[\u4e00-\u9fa5]+";
    }
}
