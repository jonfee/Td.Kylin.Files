using System.Text.RegularExpressions;

namespace Td.Kylin.Files.Core
{
    /// <summary>
    /// 缩略图扩展
    /// </summary>
    public class ThumbnailHelper
    {
        /// <summary>
        /// 缩略图起始标记
        /// </summary>
        public const string START_THUMBNAIL_TAG = "t_";

        /// <summary>
        /// 缩略图带裁剪起始标记
        /// </summary>
        public const string START_THUMBNAIL_CUT_TAG = "tc_";

        /// <summary>
        /// 缩略图起始标记正则模式字符串
        /// </summary>
        public const string START_TAG_PATTERN = "tc?_";

        /// <summary>
        /// 是否固定尺寸正则模式字符串
        /// </summary>
        public const string SIZE_FIX_PATTERN = "fix_";

        /// <summary>
        /// 获取缩略图路径
        /// </summary>
        /// <param name="orginImagePath">图片原图路径</param>
        /// <returns></returns>
        public static string GetThumbnailPath(string orginImagePath, int width, int height, bool cut = false)
        {
            if (string.IsNullOrWhiteSpace(orginImagePath)) return string.Empty;

            string thumbTag = string.Format(@"{0}({1})?w{2}h{3}_", cut ? START_THUMBNAIL_CUT_TAG : START_THUMBNAIL_TAG, SIZE_FIX_PATTERN, width, height);

            //相对路径规则
            Regex absPathReg = new Regex(@"^(?<begin>/?.+?/)(?<name>.+?)(?<extension>\.[^\.]+)$", RegexOptions.IgnoreCase);

            string path = orginImagePath;

            if (absPathReg.IsMatch(orginImagePath))
            {
                path = absPathReg.Replace(orginImagePath, "${begin}" + thumbTag + "${name}${extension}");
            }
            else
            {
                //物理路径规则
                Regex rawPathReg = new Regex(@"^(?<begin>[a-z]:[\\]+.+?[\\]+)(?<name>.+?)(?<extension>\.[^\.]+)$", RegexOptions.IgnoreCase);

                if (rawPathReg.IsMatch(orginImagePath))
                {
                    path = rawPathReg.Replace(orginImagePath, "${begin}" + thumbTag + "${name}${extension}");
                }
            }

            return path;
        }

        /// <summary>
        /// 获取缩略图的原图路径
        /// </summary>
        /// <param name="thumbnailPath">缩略图路径</param>
        /// <returns></returns>
        public static string GetOrginImagePath(string thumbnailPath)
        {
            if (string.IsNullOrWhiteSpace(thumbnailPath)) return string.Empty;

            string strPattern = string.Format(@"{0}({1})?w\d+h\d+_", START_TAG_PATTERN, SIZE_FIX_PATTERN);

            Regex reg = new Regex(strPattern, RegexOptions.IgnoreCase);

            return reg.Replace(thumbnailPath, "");
        }
    }
}
