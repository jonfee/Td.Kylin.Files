using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Td.Kylin.Files.Core
{
    /// <summary>
    /// 缩略图中间件程序
    /// </summary>
    public class ThumbnailMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly string _imageExtensions;

        public ThumbnailMiddleware(RequestDelegate next)
        {
            _imageExtensions = Startup.Configuration["Default:ImgExtensions"]?.Replace(".", "");

            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            ThumbnailImageRequest(context);

            return _next(context);
        }

        /// <summary>
        /// 缩略图处理
        /// </summary>
        /// <param name="context"></param>
        private void ThumbnailImageRequest(HttpContext context)
        {
            try
            {
                //请求文件路径
                string reqUrl = context.Request.Path;

                //为空时不处理
                if (string.IsNullOrWhiteSpace(reqUrl)) return;

                //文件物理路径
                string physicalPath = UploadHelper.GetUploadPhysicalPath(reqUrl);

                //文件名，如：myphoto.jpg
                string filename = Path.GetFileName(reqUrl);

                //检测文件是否为图片
                string regPattern = string.Format(@"^(?<thumbTag>{0})w(?<width>\d+)h(?<height>\d+)_([^\.]*\.)+({1})$", ThumbnailHelper.StartTagPattern, _imageExtensions);

                //缩略图文件名规则
                Regex reg = new Regex(regPattern, RegexOptions.IgnoreCase);

                //寻找匹配
                Match m = reg.Match(filename);

                //非缩略图格式文件，不处理
                if (!m.Success) return;

                //如果物理文件存在，则不处理
                if (File.Exists(physicalPath)) return;

                //获取原图物理路径
                string orginPhysicalPath = ThumbnailHelper.GetOrginImagePath(physicalPath);

                //如果原图物理文件不存在，则不处理
                if (!File.Exists(orginPhysicalPath)) return;

                //缩略图宽
                int thumbWidth = int.Parse(m.Groups["width"].Value);

                //缩略图高
                int thumbHeight = int.Parse(m.Groups["height"].Value);

                //缩略标记
                string thumbTag = m.Groups["thumbTag"].Value.ToLower();

                //是否需要裁剪
                bool hasCut = thumbTag.Equals(ThumbnailHelper.StartThumbCutTag);

                //根据指定规格生成缩略图
                orginPhysicalPath.ImageCrop(physicalPath, thumbWidth, thumbHeight, true, null, null, null, null, hasCut, true);
            }
            catch
            {
                //TODO 异常时处理
            }
        }
    }
}
