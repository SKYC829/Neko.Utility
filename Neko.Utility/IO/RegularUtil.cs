using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace Neko.Utility.IO
{
    /// <summary>
    /// 正则帮助类
    /// </summary>
    public sealed class RegularUtil
    {
        /// <summary>
        /// 校验一个字符串是否复合正则表达式
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="regexExpression">正则表达式</param>
        /// <returns></returns>
        public static bool VerifyIsMatch(string value,string regexExpression)
        {
            return Regex.IsMatch(value, regexExpression);
        }

        /// <summary>
        /// 校验邮箱地址
        /// </summary>
        /// <param name="emailAddress">邮箱地址</param>
        /// <returns></returns>
        public static bool VerifyEmail(string emailAddress)
        {
            return VerifyIsMatch(emailAddress, RegularDefaults.EMAIL);
        }

        /// <summary>
        /// 验证手机号
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns></returns>
        public static bool VerifyPhone(string phoneNumber)
        {
            return VerifyIsMatch(phoneNumber, string.Format("^{0}|{1}|{2}|{3}$", RegularDefaults.MAINLAND_CELLPHONE, RegularDefaults.TAIWAN_CELLPHONE, RegularDefaults.HONGKONG_CELLPHONE, RegularDefaults.MACAO_CELLPHONE));
        }

        /// <summary>
        /// 验证IPv4地址
        /// </summary>
        /// <param name="ipAddress">IPv4地址字符串</param>
        /// <returns></returns>
        public static bool VerifyIPv4(string ipAddress)
        {
            return VerifyIsMatch(ipAddress, RegularDefaults.IPADDRESS);
        }

        /// <summary>
        /// 验证身份证号
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns></returns>
        public static bool VerifyIDCard(string idCard)
        {
            return VerifyIsMatch(idCard, string.Format("^{0}|{1}$", RegularDefaults.MAINLAND_IDCARD_15, RegularDefaults.MAINLAND_IDCARD_18));
        }

        /// <summary>
        /// 验证十八位身份证号
        /// <para>根据<a href="http://www.gb688.cn/bzgk/gb/newGbInfo?hcno=080D6FBF2BB468F9007657F26D60013E">GB11643-1999标准</a>进行强验证,如需模糊验证请使用<see cref="VerifyIDCard(string)"/></para>
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns></returns>
        public static bool VerifyIDCardStrong(string idCard)
        {
            if(idCard.Length != 18)
            {
                return false;
            }
            long num;
            if(long.TryParse(idCard.Remove(17),out num) == false || num < Math.Pow(10,16) || long.TryParse(idCard.Replace('x','0').Replace('X','0'),out num) == false)
            {
                return false; //数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if(address.IndexOf(idCard.Remove(2)) == -1)
            {
                return false; //省份验证
            }
            string birthday = idCard.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime birthdayTime;
            if(DateTime.TryParse(birthday,out birthdayTime) == false)
            {
                return false; //生日验证
            }
            string[] verifyCodes = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] indexCodes = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] idCodes = idCard.Remove(17).ToCharArray();
            /*
             * 校验码的计算方法：
　　一、将身份证号码前面的17位数分别乘以不同的系数。从第一位到第十七位的系数分别为：7－9－10－5－8－4－2－1－6－3－7－9－10－5－8－4－2。
　　二、将这17位数字和系数相乘的结果相加。
　　三、用加出来的和除以11，余数只可能有：0－1－2－3－4－5－6－7－8－9－10这11个数字。其分别对应的最后一位身份证的号码为：1－0－X －9－8－7－6－5－4－3－2。
　　四、如果余数是2，那么身份证的第18位数字就是 X 。
　　例如：某男性的身份证号码为【53010219200508011x】， 我们看看这个身份证是不是合法的身份证。
　　首先我们得出前17位的乘积和【(5*7)+(3*9)+(0*10)+(1*5)+(0*8)+(2*4)+(1*2)+(9*1)+(2*6)+(0*3)+(0*7)+(5*9)+(0*10)+(8*5)+(0*8)+(1*4)+(1*2)】是189，然后用189除以11得出的结果是189/11=17----2，也就是说其余数是2。最后通过对应规则就可以知道余数2对应的检验码是X。所以，可以判定这是一个正确的身份证号码。 
             */
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(indexCodes[i]) * int.Parse(idCodes[i].ToString());
            }
            int verifyIndex = sum % 11;
            if(verifyCodes[verifyIndex] != idCard.Substring(17, 1).ToLower())
            {
                return false; //校验码验证
            }
            return true;
        }

        /// <summary>
        /// 验证网址
        /// </summary>
        /// <param name="webUrl">网址</param>
        /// <returns></returns>
        public static bool VerifyWebUrl(string webUrl)
        {
            return VerifyIsMatch(webUrl, RegularDefaults.WEB_URL);
        }

        /// <summary>
        /// 从字符串中取出符合正则表达式的第一个字符串
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="regexExpression">正则表达式</param>
        /// <returns></returns>
        public static string Get(string value,string regexExpression)
        {
            return GetAll(value, regexExpression).FirstOrDefault();
        }

        /// <summary>
        /// 从字符串中取出符合正则表达式的所有字符串
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="regexExpression">正则表达式</param>
        /// <returns></returns>
        public static IEnumerable<string> GetAll(string value,string regexExpression)
        {
            return Regex.Matches(value, regexExpression).Cast<Match>().Select(p => p.Value).AsEnumerable<string>();
        }
    }
}
