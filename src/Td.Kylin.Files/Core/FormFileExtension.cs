using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;

namespace Td.Kylin.Files.Core
{
    public static class FormFileExtension
    {
        /// <summary>
        /// 获取文件的内容头部信息
        /// </summary>
        public static ContentDispositionHeaderValue GetHeaderValue(this IFormFile file)
        {
            return ContentDispositionHeaderValue.Parse(file.ContentDisposition);
        }

        /// <summary>
        /// 是否为图片文件
        /// </summary>
        public static bool IsImage(this IFormFile file)
        {
            string contentType = file.ContentType;

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// 获取文件标识字段名
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFieldName(this IFormFile file)
        {
            var headerValue = file.GetHeaderValue();

            return headerValue?.Name;
        }
    }
}
