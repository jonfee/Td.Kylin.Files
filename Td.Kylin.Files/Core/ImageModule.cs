// ===============================================================================
// 功能描述: 
// 代码作者：伍朝辉
// 联 系QQ： 337883612
// 创建时间：2012-5-9 13:46:39
// 更新记录:     

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace Td.Kylin.Files.Core
{
    /// <summary>
    /// 图片Module处理（按照符合组件的规则生成缩略图）
    /// </summary>
    public class ImageModule : IHttpModule
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init(HttpApplication appContext)
        {
            appContext.BeginRequest += new EventHandler(httpApp_BeginRequest);
        }

        /// <summary>
        /// BeginRequest事件发生时的第一个处理
        /// </summary>
        /// <param name="application">HttpApplication</param>
        /// <returns>是否继续执行的表示，True为继续执行，False表示不继续处理后续请求</returns>
        public virtual bool RequestFirst(HttpApplication application)
        {
            return true;
        }

        /// <summary>
        /// 生成指定规格的图片（如果原图存在的情况）
        /// </summary>
        /// <param name="application">HttpApplication</param>
        public virtual void MakeThumbImage(HttpApplication application)
        {
            //获取请求的物理路径
            string fileName = application.Request.PhysicalPath;

            if (string.IsNullOrEmpty(fileName)) return;

            //文件物理路径大写表示
            fileName = fileName.ToUpper();

            // 文件已经存在，不做处理
            if (File.Exists(fileName)) return;

            //最后一个文件夹分隔字符索引位置（从零开始）
            var lastSplitIndex = fileName.LastIndexOf("\\");

            //获取文件名称
            var name = fileName.Substring(lastSplitIndex + 1);

            if (string.IsNullOrEmpty(name)) return; // 为空，不做处理。

            string regStr =
                string.Format(
                    @"^{0}W(?<width>\d+)H(?<height>\d+)_([^\.]*\.)+(jpg|jpeg|gif|bmp|png|ico|pcx|tiff|tga|exif|fpx|svg|psd|cdr|pcd|dxf|ufo|eps|ai|hdri|raw)$",
                    ThumbIdentify.Identify);
            Regex reg = new Regex(regStr, RegexOptions.IgnoreCase);

            Match m = reg.Match(name);

            if (!m.Success) return; //非特定图片格式，不处理

            //原图文件
            Regex originalReg = new Regex(string.Format(@"{0}W\d+H\d+_", ThumbIdentify.Identify),
                RegexOptions.IgnoreCase);
            var originalName = originalReg.Replace(name, "");

            string originalFileName = fileName.Remove(lastSplitIndex + 1) + originalName;

            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName)); // 目标目录不存在，创建。
            }

            try
            {
                int w = int.Parse(m.Groups["width"].Value);

                int h = int.Parse(m.Groups["height"].Value);

                ThumbHelper.MakeThumbnailByAutoCut(originalFileName, fileName, w, h);
            }
            catch
            {
                return;
            }
        }

        private void httpApp_BeginRequest(object sender, EventArgs e)
        {
            //定义应用程序相关对象
            var app = sender as HttpApplication;

            //获取请求的物理路径
            string fileName = app.Request.PhysicalPath;

            if (string.IsNullOrEmpty(fileName)) return;

            Regex reg = new Regex(@"^([^\.]*\.)+(jpg|jpeg|gif|bmp|png|ico|pcx|tiff|tga|exif|fpx|svg|psd|cdr|pcd|dxf|ufo|eps|ai|hdri|raw)$", RegexOptions.IgnoreCase);

            if (!reg.IsMatch(fileName)) return;//非图片格式，不处理

            //第一步处理
            var isContinue = RequestFirst(app);

            if (isContinue)
            {
                MakeThumbImage(app);
            }
        }
    }
}



