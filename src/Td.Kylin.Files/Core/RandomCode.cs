using System;
using System.Text;

namespace Td.Kylin.Files.Core
{
    /// <summary>
    /// 随机码
    /// </summary>
    public static class RandomCode
    {
        /// <summary>
        /// 生成随机码（按当前时间）
        /// </summary>
        /// <param name="randomLen">在时间（毫秒）后加入的随机数长度</param>
        /// <returns></returns>
        public static string GetDtString(int randomLen = 0)
        {
            var code = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            if (randomLen < 1) randomLen = 0;

            if (randomLen > 0)
            {
                code += Get("0123456789", randomLen);
            }

            return code;
        }

        /// <summary>
        /// 生成随机码（按当前时间）
        /// </summary>
        /// <param name="randomLen">在时间（毫秒）后加入的随机数长度</param>
        /// <returns></returns>
        public static string GetTimeString(int randomLen = 0)
        {
            var code = DateTime.Now.ToString("HHmmssfff");

            if (randomLen < 1) randomLen = 0;

            if (randomLen > 0)
            {
                code += Get("0123456789", randomLen);
            }

            return code;
        }

        /// <summary>
        /// 生成随机码
        /// </summary>
        /// <param name="length">随机码的长度</param>
        /// <returns></returns>
        public static string Get(int length)
        {
            return Get(strChars: null, length: length);
        }

        /// <summary>
        /// 生成随机码
        /// </summary>
        /// <param name="strChars">指定随机码的基元素（如：0123456789）</param>
        /// <param name="length">随机码的长度</param>
        /// <returns></returns>
        public static string Get(string strChars, int length)
        {
            if (string.IsNullOrWhiteSpace(strChars)) strChars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";

            StringBuilder sb = new StringBuilder(length);

            for (var i = 0; i < length; i++)
            {
                int index = new Random(Guid.NewGuid().GetHashCode()).Next(0, strChars.Length);

                sb.Append(strChars[index]);
            }

            return sb.ToString();
        }
    }
}
