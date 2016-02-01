// ===============================================================================
// 功能描述: 
// 代码作者：伍朝辉
// 联 系QQ： 337883612
// 创建时间：2012-5-9 14:05:47
// 更新记录:     

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Td.Kylin.Files.Core
{
    /// <summary>
    /// 缩略图标识
    /// </summary>
    public static class ThumbIdentify
    {
        /// <summary>
        /// 缩略图标识
        /// </summary>
        public static readonly string Identify = "T_";

        public static string GetThumbnailPath(this string imgUrl, int width, int height)
        {
            if (string.IsNullOrEmpty(imgUrl)) return string.Empty;


            string[] arr = imgUrl.Split(new[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);

            int len = arr.Length;

            string fn = arr[len - 1];

            arr[len - 1] = string.Format("T_W{0}H{1}_{2}", width, height, fn);

            return Path.Combine(arr).ToLower();
        }
    }
}



