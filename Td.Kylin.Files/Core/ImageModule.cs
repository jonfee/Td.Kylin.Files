// ===============================================================================
// 功能描述: 
// 代码作者：伍朝辉
// 联 系QQ： 337883612
// 创建时间：2012-5-9 13:46:39
// 更新记录:     

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace Td.Kylin.Files.Core
{
    /// <summary>
    /// 图片Module处理（按照符合组件的规则生成缩略图）
    /// </summary>
    public class ImageModule : IHttpModule
    {
        static string imgExtensions = "jpg|jpeg|gif|bmp|png|ico|pcx|tiff|tga|exif|fpx|svg|psd|cdr|pcd|dxf|ufo|eps|ai|hdri|raw";

        public void Dispose()
        {
            // throw new NotImplementedException();
        }

        public void Init(HttpApplication appContext)
        {
            appContext.BeginRequest += new EventHandler(httpApp_BeginRequest);
        }

        /// <summary>
        /// 生成指定规格的图片（如果原图存在的情况）
        /// </summary>
        /// <param name="physicalPath">访问图片的物理路径</param>
        public virtual void MakeThumbImage(string physicalPath)
        {
            //文件物理路径大写表示
            physicalPath = physicalPath.ToUpper();

            var filename = Path.GetFileName(physicalPath);

            //原图文件替换标识正则
            Regex originalReg = new Regex(string.Format(@"{0}W\d+H\d+_", ThumbIdentify.Identify), RegexOptions.IgnoreCase);

            //原图文件
            string originalFileName = originalReg.Replace(physicalPath, "");

            if (!Directory.Exists(Path.GetDirectoryName(physicalPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)); // 目标目录不存在，创建。
            }

            try
            {
                string regStr = string.Format(@"^{0}W(?<width>\d+)H(?<height>\d+)_(?:(?!\.{1}).)+(\.({1}))+$", ThumbIdentify.Identify, imgExtensions);
                Regex reg = new Regex(regStr, RegexOptions.IgnoreCase);

                Match m = reg.Match(filename);

                if (null != m)
                {
                    int w = int.Parse(m.Groups["width"].Value);

                    int h = int.Parse(m.Groups["height"].Value);

                    ThumbHelper.MakeThumbnailByAutoCut(originalFileName, physicalPath, w, h);
                }
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
            string physicalPath = app.Request.PhysicalPath;

            var work = GetWork(physicalPath);

            switch (work)
            {
                case WorkType.TransferFormat:
                    TransferFormat(physicalPath);
                    break;
                case WorkType.MakeThumbImage:
                    MakeThumbImage(physicalPath);
                    break;
            }
        }

        private void TransferFormat(string physicalPath)
        {
            //原图文件
            string oldFile = physicalPath.Remove(physicalPath.LastIndexOf('.'));

            //扩展名
            string extension = Path.GetExtension(physicalPath);

            Image image = Image.FromFile(oldFile);

            using (MemoryStream ms = new MemoryStream())
            {
                ImageFormat format = ImageFormat.Jpeg;
                ImageCodecInfo codecInfo = ImageCodecInfo.GetImageEncoders().Where(p => p.FilenameExtension.Contains(extension.ToUpper())).FirstOrDefault();
                if (codecInfo != null)
                {
                    PropertyInfo prop = typeof(ImageFormat).GetProperties().Where(p => p.Name.Equals(codecInfo.FormatDescription, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (prop != null)
                    {
                        format = prop.GetValue(prop, null) as ImageFormat;
                    }
                }

                image.Save(ms, format);

                Bitmap bitmap = new Bitmap(ms);

                bitmap.Save(physicalPath);
            }
        }

        private WorkType GetWork(string physicalPath)
        {
            if (string.IsNullOrWhiteSpace(physicalPath)) return WorkType.NoAction;

            //文件已经存在，不做处理
            if (File.Exists(physicalPath)) return WorkType.NoAction;

            //获取文件名称
            var filename = Path.GetFileName(physicalPath);

            if (string.IsNullOrEmpty(filename)) return WorkType.NoAction;

            //检测是否为图片
            bool isImage = IsImage(filename);

            //非图片格式，不处理
            if (!isImage) return WorkType.NoAction;

            //是否为转换格式
            bool isTransType = false;
            Regex regTrans = new Regex(string.Format(@"(\.({0})){1}$", imgExtensions, "{2,}"), RegexOptions.IgnoreCase);
            if (regTrans.IsMatch(filename))
            {
                string beforeTransferFile = GetBeforeTransferFilePath(physicalPath);
                if (File.Exists(beforeTransferFile)) isTransType = true;
            }

            //转换格式操作
            if (isTransType) return WorkType.TransferFormat;

            string regStr = string.Format(@"^{0}W\d+H\d+_(?:(?!\.{1}).)+(\.({1}))+$", ThumbIdentify.Identify, imgExtensions);
            Regex reg = new Regex(regStr, RegexOptions.IgnoreCase);
            if (reg.IsMatch(filename))
            {
                return WorkType.MakeThumbImage;
            }

            return WorkType.NoAction;
        }

        /// <summary>
        /// 是否为图片
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool IsImage(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) return false;

            Regex reg = new Regex(string.Format(@"\.({0})$", imgExtensions), RegexOptions.IgnoreCase);

            return reg.IsMatch(filename);
        }

        /// <summary>
        /// 获取扩展名
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string GetExtension(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return string.Empty;

            return Path.GetExtension(filePath);
        }

        /// <summary>
        /// 获取转换前的原文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetBeforeTransferFilePath(string filename)
        {
            Regex reg = new Regex(string.Format(@"(\.({0})){1}$", imgExtensions, "{2,}"), RegexOptions.IgnoreCase);

            if (reg.IsMatch(filename))
            {
                return filename.Remove(filename.LastIndexOf('.'));
            }

            return string.Empty;
        }

        /// <summary>
        /// 工作类型
        /// </summary>
        enum WorkType
        {
            /// <summary>
            /// 无操作
            /// </summary>
            NoAction,
            /// <summary>
            /// 生成缩略图
            /// </summary>
            MakeThumbImage,
            /// <summary>
            /// 转换格式
            /// </summary>
            TransferFormat
        }
    }
}



