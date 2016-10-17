using Microsoft.AspNetCore.Http;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Td.Kylin.Files.Models;

namespace Td.Kylin.Files.Core
{
    public static class UploadHelper
    {
        /// <summary>
        /// 默认上传文件目录
        /// </summary>
        private static string defaultSavePath { get; set; }

        /// <summary>
        /// 上传文件大小限制（单位：KB）
        /// </summary>
        private static int limitSize { get; set; }

        /// <summary>
        /// 默认压缩参数
        /// </summary>
        private static EncoderParameters defaultEncoderParams { get; set; }

        static UploadHelper()
        {
            defaultSavePath = Startup.Configuration["Default:SavePath"];

            limitSize = int.Parse(Startup.Configuration["Default:LimitSize"]);

            long qualityValue = long.Parse(Startup.Configuration["Default:EncoderQualityValue"]);

            long colorDepthValue = long.Parse(Startup.Configuration["Default:EncoderColorDepthValue"]);

            defaultEncoderParams = GetEncoderParameters(qualityValue, colorDepthValue);
        }

        #region 图片上传

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="image">图片对象数据</param>
        /// <param name="filesize">文件对象数据大小</param>
        /// <param name="beOverride">文件存在时是否覆盖</param>
        /// <returns></returns>
        public static UploadResult Save(this Image image, long filesize, bool beOverride = false)
        {
            return Save(image, filesize, null, beOverride);
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="image">图片对象数据</param>
        /// <param name="filesize">文件对象数据大小</param>
        /// <param name="savepath">文件存储目录</param>
        /// <param name="beOverride">文件存在时是否覆盖</param>
        /// <returns></returns>
        public static UploadResult Save(this Image image, long filesize, string savepath, bool beOverride = false)
        {
            return Save(image, filesize, savepath, null, beOverride);
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="image">图片对象数据</param>
        /// <param name="filesize">文件对象数据大小</param>
        /// <param name="savepath">文件存储目录</param>
        /// <param name="filename">指定文件名，为空时使用随机名</param>
        /// <param name="beOverride">文件存在时是否覆盖</param>
        /// <returns></returns>
        public static UploadResult Save(this Image image, long filesize, string savepath, string filename, bool beOverride = false)
        {
            return Save(image, filesize, savepath, true, filename, null, beOverride);
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="image">图片对象数据</param>
        /// <param name="filesize">文件对象数据大小</param>
        /// <param name="savepath">文件存储目录</param>
        /// <param name="filename">指定文件名，为空时使用随机名</param>
        /// <param name="format">存储格式</param>
        /// <param name="beOverride">文件存在时是否覆盖</param>
        /// <returns></returns>
        public static UploadResult Save(this Image image, long filesize, string savepath, string filename, ImageFormat format, bool beOverride = false)
        {
            return Save(image, filesize, savepath, true, filename, format, beOverride);
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="image">图片对象数据</param>
        /// <param name="filesize">文件对象数据大小</param>
        /// <param name="savepath">文件存储目录</param>
        /// <param name="fixedpath">是否固定存储在指定的savepath目录下，为false时在savepath目录后添加日期目录</param>
        /// <param name="filename">指定文件名，为空时使用随机名</param>
        /// <param name="beOverride">文件存在时是否覆盖</param>
        /// <returns></returns>
        public static UploadResult Save(this Image image, long filesize, string savepath, bool fixedpath, string filename, bool beOverride = false)
        {
            return Save(image, filesize, savepath, fixedpath, filename, null, beOverride);
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="image">图片对象数据</param>
        /// <param name="filesize">文件对象数据大小</param>
        /// <param name="savepath">文件存储目录</param>
        /// <param name="fixedpath">是否固定存储在指定的savepath目录下，为false时在savepath目录后添加日期目录</param>
        /// <param name="filename">指定文件名，为空时使用随机名</param>
        /// <param name="format">存储格式</param>
        /// <param name="beOverride">文件存在时是否覆盖</param>
        /// <param name="compressIfGreaterSize">超过该大小时进行质量压缩（单位：KB），该参数只对图片上传有效，图片超出限制宽高时失效</param>
        /// <param name="maxWidth">最大宽，该参数只对图片上传有效</param>
        /// <param name="maxHeight">最大高，该参数只对图片上传有效</param>
        /// <param name="cutIfOut">超过最大宽高时是否裁剪（为false时不足部分留白），该参数只对图片上传有效</param>
        /// <returns></returns>
        public static UploadResult Save(this Image image, long filesize, string savepath, bool fixedpath, string filename, ImageFormat format, bool beOverride = false, long compressIfGreaterSize = 0, int maxWidth = 0, int maxHeight = 0, bool cutIfOut = false)
        {
            var result = new UploadResult();

            if (filesize == 0)
            {
                result.Message = "请上传有效的文件";
                return result;
            }

            if (filesize > limitSize * 1024)
            {
                result.Message = string.Format("服务器限制上传文件最大不能超过{0}KB", limitSize);
                return result;
            }
            var filetype = GetImageRawType(image);

            if (string.IsNullOrEmpty(filetype))
            {
                result.Message = "上传的不是有效的图片文件";
                return result;
            }

            //固定在指定的上传目录
            if (fixedpath)
            {
                if (string.IsNullOrWhiteSpace(savepath)) savepath = defaultSavePath;
            }
            //非固定指定上传目录时，在上传目录中加入时期目录层
            else
            {
                savepath = savepath.AppendFileDirectoryByDate();
            }

            //文件保存后文件全名（如：/upload/2016/12/10/231.jpg）
            var absFileName = savepath.GetFileName(filename, format);

            //新上传后的存储路径（如：D:\\upload\\2016\\12\\10\\45341.jpg）
            var rawFilePath = GetUploadPhysicalPath(absFileName);

            if (File.Exists(rawFilePath))
            {
                if (beOverride)
                {
                    File.Delete(rawFilePath);
                }
                else
                {
                    GetLastVersionFile(absFileName, 1, false, out absFileName, out rawFilePath);
                }
            }

            //物理目录
            var rawDirectory = Path.GetDirectoryName(rawFilePath);

            try
            {
                //目录不存在时创建
                if (!Directory.Exists(rawDirectory))
                {
                    Directory.CreateDirectory(rawDirectory);
                }
            }
            catch
            {
                result.Message = "创建目录失败";
                return result;
            }

            string saveMark = "保存文件";
            try
            {
                //是否超出图片限制尺寸
                bool needCrop = (maxWidth > 0 || maxHeight > 0) && (image.Width > maxWidth || image.Height > maxHeight);

                //压缩尺寸
                if (needCrop)
                {
                    saveMark = "压缩尺寸（含质量）存储";

                    image.ImageCrop(rawFilePath, maxWidth, maxHeight, null, null, null, null, cutIfOut, true);
                }
                else
                {
                    //是否需要压缩
                    bool needCompress = compressIfGreaterSize > 0 && filesize > compressIfGreaterSize;

                    //按指定格式存储并压缩
                    if (needCompress)
                    {
                        saveMark = string.Format("压缩{0}存储", format != null ? "并转换格式" : "");

                        image.CompressSave(rawFilePath, format, defaultEncoderParams);
                    }
                    //存储指定格式且不压缩
                    else if (format != null)
                    {
                        saveMark = "转换格式存储";
                        image.Save(rawFilePath, format);
                    }
                    //存储原图格式且不压缩
                    else
                    {
                        image.Save(rawFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = string.Format("{0}失败({1})", saveMark, ex.Message);
            }

            result.FilePath = absFileName;

            return result;
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="filePath">文件名称和路径</param>
        /// <param name="savePath">缩略图文件名称和路径</param>
        /// <param name="toWidth">缩略图宽度</param>
        /// <param name="toHeight">缩略图高度</param>
        /// <param name="lessOrEqualOrginSize">是否必须小于或等于原图尺寸</param>
        /// <param name="cutX">切割的 X 坐标</param>
        /// <param name="cutY">切割的 Y 坐标</param>
        /// <param name="cutW">切割的宽度</param>
        /// <param name="cutH">切割的高度</param>
        /// <param name="cut">是否切割多余部分，为 False 则保留原图所有部分，不足的部分填白。</param>
        /// /// <param name="keepToSize">是否始终保留指定的缩略图尺寸</param>
        public static UploadResult ImageCrop(this string filePath, string savePath, int toWidth, int toHeight, int? cutX, int? cutY, int? cutW, int? cutH, bool cut, bool? keepToSize = null, bool? lessOrEqualOrginSize = null)
        {
            try
            {
                Image origin = Image.FromFile(filePath);

                return origin.ImageCrop(savePath, toWidth, toHeight, cutX, cutY, cutW, cutH, cut, keepToSize, lessOrEqualOrginSize);
            }
            catch
            {
                return new UploadResult
                {
                    Message = "加载源文件失败"
                };
            }
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="origin"><seealso cref="Image"/>原图</param>
        /// <param name="savePath">缩略图文件名称和路径</param>
        /// <param name="toWidth">缩略图宽度</param>
        /// <param name="toHeight">缩略图高度</param>
        /// <param name="lessOrEqualOrginSize">是否必须小于或等于原图尺寸</param>
        /// <param name="cutX">切割的 X 坐标</param>
        /// <param name="cutY">切割的 Y 坐标</param>
        /// <param name="cutW">切割的宽度</param>
        /// <param name="cutH">切割的高度</param>
        /// <param name="cut">是否切割多余部分，为 False 则保留原图所有部分，不足的部分填白。</param>
        /// <param name="keepToSize">是否始终保留指定的缩略图尺寸</param>
        public static UploadResult ImageCrop(this Image origin, string savePath, int toWidth, int toHeight, int? cutX, int? cutY, int? cutW, int? cutH, bool cut, bool? keepToSize = null, bool? lessOrEqualOrginSize = null)
        {
            if (origin == null) throw new ArgumentNullException(nameof(origin), "缩略图原始文件不能为空");

            var result = new UploadResult();

            //物理目录
            var rawDirectory = Path.GetDirectoryName(savePath);

            try
            {
                //目录不存在时创建
                if (!Directory.Exists(rawDirectory))
                {
                    Directory.CreateDirectory(rawDirectory);
                }
            }
            catch
            {
                result.Message = "创建目录失败";
                return result;
            }

            #region 校正压缩尺寸

            int thumbWidth = toWidth;   //缩略图宽
            int thumbHeight = toHeight; //缩略图高

            //是否按最大连等比缩放后裁剪多余部分
            bool isMaxThumbAfterCut = cut && (!cutX.HasValue || !cutY.HasValue || !cutW.HasValue || !cutH.HasValue);

            //压缩宽高均为0时，保持原图尺寸
            if (toWidth <= 0 && toHeight <= 0)
            {
                toWidth = origin.Width;
                toHeight = origin.Height;
            }
            else
            {
                //必须小于或等于原图尺寸限制时处理
                if ((!keepToSize.HasValue || !keepToSize.Value) && (!lessOrEqualOrginSize.HasValue || lessOrEqualOrginSize.Value))
                {
                    if (toWidth > origin.Width) toWidth = origin.Width;//缩略图宽最大为原始图片宽
                    if (toHeight > origin.Height) toHeight = origin.Height;//缩略图高最大为原始图片高
                }

                //计算应压缩的宽高
                double multipleW = (double)toWidth / (double)origin.Width;
                double multipleH = (double)toHeight / (double)origin.Height;

                if (multipleW == 0)
                {
                    toWidth = (int)(origin.Width * multipleH);
                }
                else if (multipleH == 0)
                {
                    toHeight = (int)(origin.Height * multipleW);
                }
                else if (multipleW > multipleH)
                {
                    if (isMaxThumbAfterCut)
                    {
                        toHeight = (int)(origin.Height * multipleW);
                    }
                    else
                    {
                        toWidth = (int)(origin.Width * multipleH);
                    }
                }
                else if (multipleH > multipleW)
                {
                    if (isMaxThumbAfterCut)
                    {
                        toWidth = (int)(origin.Width * multipleH);
                    }
                    else
                    {
                        toHeight = (int)(origin.Height * multipleW);
                    }
                }
            }

            //校正最终的缩略图应保持的宽高
            if (keepToSize.HasValue && keepToSize.Value)
            {
                if (thumbWidth <= 0) thumbWidth = toWidth;
                if (thumbHeight <= 0) thumbHeight = toHeight;
            }
            else
            {
                thumbWidth = toWidth;
                thumbHeight = toHeight;
            }

            #endregion

            int drawX = 0; int drawY = 0; int drawW = toWidth; int drawH = toHeight;
            if (!cutX.HasValue || !cutY.HasValue || !cutW.HasValue || !cutH.HasValue)
            {
                if (origin.Width < toWidth && origin.Height < toHeight)
                {
                    cutW = origin.Width; cutH = origin.Height; cutX = cutY = 0;

                    drawX = (toWidth - origin.Width) / 2;
                    drawY = (toHeight - origin.Height) / 2;

                    drawW = origin.Width; drawH = origin.Height;

                }
                else
                {
                    double multipleWidth = (double)origin.Width / (double)toWidth;
                    double multipleHeight = (double)origin.Height / (double)toHeight;

                    if (multipleWidth < multipleHeight)
                    {
                        if (cut)
                        {
                            cutW = origin.Width;
                            cutH = (int)(toHeight * multipleWidth);

                            cutX = 0;
                            cutY = (origin.Height - cutH) / 2;
                        }
                        else
                        {
                            cutH = toHeight;
                            cutW = (int)(origin.Width / multipleHeight);

                            cutX = cutY = 0;
                        }
                    }
                    else if (multipleHeight > multipleWidth)
                    {
                        if (cut)
                        {
                            cutW = (int)(toWidth * multipleHeight);
                            cutH = origin.Height;

                            cutX = (origin.Width - cutW) / 2;
                            cutY = 0;
                        }
                        else
                        {
                            cutH = (int)(origin.Height / multipleWidth);
                            cutW = toWidth;

                            cutX = cutY = 0;
                        }
                    }
                    else
                    {
                        cut = false;
                        cutW = thumbWidth; cutH = thumbHeight; cutX = cutY = 0;
                    }
                }
            }

            if (!cut)
            {
                drawX = (thumbWidth - cutW.Value) / 2;
                drawY = (thumbWidth - cutH.Value) / 2;

                drawW = cutW.Value;
                drawH = cutH.Value;

                cutW = origin.Width;
                cutH = origin.Height;
            }
            else
            {
                if (thumbWidth > toWidth) drawX = (thumbWidth - toWidth) / 2;
                if (thumbHeight > toHeight) drawY = (thumbHeight - toHeight) / 2;
            }

            #region 创建缩略图

            Image bitmap = new Bitmap(thumbWidth, thumbHeight);

            Graphics g = Graphics.FromImage(bitmap);

            g.CompositingQuality = CompositingQuality.HighQuality;

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.SmoothingMode = SmoothingMode.HighQuality;

            g.Clear(Color.White);

            g.DrawImage(origin, new Rectangle(drawX, drawY, drawW, drawH), new Rectangle(cutX.Value, cutY.Value, cutW.Value, cutH.Value), GraphicsUnit.Pixel);

            #endregion

            #region 保存缩略图

            try
            {
                if (origin.RawFormat.Guid == ImageFormat.Gif.Guid)
                {
                    bitmap.Save(savePath, ImageFormat.Gif);
                }
                else
                {
                    ImageCodecInfo imageEncoder = GetEncoderInfo(ImageFormat.Jpeg);

                    bitmap.Save(savePath, imageEncoder, defaultEncoderParams);
                }
                result.FilePath = savePath.GetFilePathByRawDirection();
            }
            catch
            {
                result.Message = "裁剪图片保存失败";
                throw;
            }
            finally
            {
                origin.Dispose();

                bitmap.Dispose();

                g.Dispose();
            }

            #endregion

            return result;
        }

        /// <summary>
        /// 压缩存储
        /// </summary>
        /// <param name="image"><seealso cref="Image"/>对象</param>
        /// <param name="format">存储的图片格式</param>
        /// <param name="encoderParams">用于压缩的图片解码器提供值</param>
        /// <param name="savepath">保存路径</param>
        /// 
        public static void CompressSave(this Image image, string savepath, ImageFormat format, EncoderParameters encoderParams)
        {
            if (format == null) format = image.GetImageRawType().GetImageFormat();

            ImageCodecInfo encoder = GetEncoderInfo(format);//image/jpeg

            if (encoder == null) encoder = GetEncoderInfo(ImageFormat.Jpeg);

            if (encoderParams == null)
            {
                encoderParams = defaultEncoderParams;
            }

            image.Save(savepath, encoder, encoderParams);
        }

        #endregion

        #region 文件上传

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="file"><seealso cref="IFormFile"/>文件</param>
        /// <param name="savepath">上传目录</param>
        /// <param name="fixedpath">是否固定存储在指定的savepath目录下，为false时在savepath目录后添加日期目录</param>
        /// <param name="name">保存文件名（不含扩展名）</param>
        /// <param name="beOverride">文件存在时是否覆盖</param>
        /// <returns></returns>
        public static UploadResult Save(this IFormFile file, string savepath, bool fixedpath, string name, bool beOverride = false)
        {
            var result = new UploadResult();

            if (null == file)
            {
                result.Message = "没有可上传的内容";
                return result;
            }

            if (file.Length > limitSize * 1024)
            {
                result.Message = string.Format("上传的文件不能超过{0}KB", limitSize);
                return result;
            }

            //标识字段名
            result.FieldName = file.GetFieldName();

            //文件名
            string fileName = file.FileName;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = RandomCode.GetDtString(3);
            }
            else
            {
                name = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
            }

            //扩展名
            string extension = Path.GetExtension(file.FileName);

            //固定在指定的上传目录
            if (fixedpath)
            {
                if (string.IsNullOrWhiteSpace(savepath)) savepath = defaultSavePath;
            }
            //非固定指定上传目录时，在上传目录中加入时期目录层
            else
            {
                savepath = savepath.AppendFileDirectoryByDate();
            }

            //文件保存后文件全名（如：/upload/2016/12/10/231.jpg）
            var absFileName = savepath.GetFileName(name, extension);

            //新上传后的存储路径（如：D:\\upload\\2016\\12\\10\\45341.jpg）
            var rawFilePath = Path.Combine(Startup.PhysicalUploadRoot, absFileName.TrimStart('/').Replace("/", "\\"));

            if (File.Exists(rawFilePath))
            {
                if (beOverride)
                {
                    File.Delete(rawFilePath);
                }
                else
                {
                    GetLastVersionFile(absFileName, 1, false, out absFileName, out rawFilePath);
                }
            }

            //物理目录
            var rawDirectory = Path.GetDirectoryName(rawFilePath);

            try
            {
                if (!Directory.Exists(rawDirectory))
                {
                    Directory.CreateDirectory(rawDirectory);
                }
            }
            catch
            {
                result.Message = "创建目录失败";
                return result;
            }

            try
            {
                using (FileStream fs = File.Create(rawFilePath))
                {
                    file.CopyToAsync(fs).Wait();
                }

                result.FilePath = absFileName;
            }
            catch
            {
                result.Message = "文件存储失败";
            }

            return result;
        }

        #endregion

        #region 支撑方法

        /// <summary>
        /// 获取图像编码器
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == mimeType)
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取图像编码器
        /// </summary>
        /// <param name="format">图像文件格式</param>
        /// <returns>图片解码器</returns>
        public static ImageCodecInfo GetEncoderInfo(this ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>
        /// 根据当前日期，指定跟目录和文件扩展名获取一个完整的存储路径和名称
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name">文件名称，为null时使用随机文件名</param>
        /// <param name="extension">文件扩展名</param>
        /// <returns></returns>
        public static string AppendFileNameByDate(this string path, string name, string extension)
        {
            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException(nameof(extension));

            if (extension.StartsWith(".")) extension = extension.TrimStart('.');

            path = path.AppendFileDirectoryByDate();

            if (string.IsNullOrWhiteSpace(name)) name = RandomCode.GetDtString(6);

            StringBuilder fileName = new StringBuilder(path);

            fileName.AppendFormat("/{0}/{1}.{2}", path, name, extension);

            return fileName.ToString();
        }

        /// <summary>
        /// 由上传路径、文件名及扩展名，构造一个完整的存储路径和名称
        /// </summary>
        /// <param name="path">上传路径</param>
        /// <param name="name">文件名</param>
        /// <param name="extension">扩展名</param>
        /// <returns></returns>
        public static string GetFileName(this string path, string name, ImageFormat format)
        {
            return GetFileName(path, name, format.GetImageType());
        }

        /// <summary>
        /// 由上传路径、文件名及扩展名，构造一个完整的存储路径和名称
        /// </summary>
        /// <param name="path">上传路径</param>
        /// <param name="name">文件名</param>
        /// <param name="extension">扩展名</param>
        /// <returns></returns>
        public static string GetFileName(this string path, string name, string extension)
        {
            if (string.IsNullOrWhiteSpace(path)) path = path.AppendFileDirectoryByDate();

            if (path.StartsWith("/")) path = path.TrimStart('/');//移除头部"/"字符

            if (path.EndsWith("/")) path = path.TrimEnd('/');//移除尾部"/"字符

            if (string.IsNullOrWhiteSpace(name)) name = RandomCode.GetTimeString(6);

            if (extension.StartsWith(".")) extension = extension.TrimStart('.');

            return string.Format("/{0}/{1}.{2}", path, name, extension);
        }

        /// <summary>
        /// 在路径后追加日期目录
        /// </summary>
        /// <param name="path">原上传路径，为null时使用默认上传路径</param>
        /// <returns></returns>
        public static string AppendFileDirectoryByDate(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = defaultSavePath;
            }

            if (path.StartsWith("/")) path = path.TrimStart('/');//移除头部"/"字符

            if (path.EndsWith("/")) path = path.TrimEnd('/');//移除尾部"/"字符

            StringBuilder fileName = new StringBuilder(path);

            fileName.Append(DateTime.Now.ToString("/yyyy/MM/dd"));

            return fileName.ToString();
        }

        /// <summary>
        /// 从上传后的物理路径中获取相对路径
        /// </summary>
        /// <param name="rawDirection"></param>
        /// <returns></returns>
        public static string GetFilePathByRawDirection(this string rawDirection)
        {
            if (string.IsNullOrWhiteSpace(rawDirection)) return string.Empty;

            Regex reg = new Regex(@"^(?<begin>[a-z]:[\\]+.+?)(?<extension>\.[^\.]+)$", RegexOptions.IgnoreCase);

            string filepath = string.Empty;

            if (reg.IsMatch(rawDirection))
            {
                filepath = rawDirection.Replace(Startup.PhysicalUploadRoot, "").Replace(@"\\", "/");
            }

            return filepath;
        }

        /// <summary>
        /// 获取图片的真实类型
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string GetImageRawType(this Image image)
        {
            if (image.RawFormat.Guid == ImageFormat.Png.Guid)
            {
                return "png";
            }
            else if (image.RawFormat.Guid == ImageFormat.Jpeg.Guid)
            {
                return "jpg";
            }
            else if (image.RawFormat.Guid == ImageFormat.Bmp.Guid)
            {
                return "bmp";
            }
            else if (image.RawFormat.Guid == ImageFormat.Gif.Guid)
            {
                return "gif";
            }
            else if (image.RawFormat.Guid == ImageFormat.Icon.Guid)
            {
                return "ico";
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetImageType(this ImageFormat format)
        {
            if (format.Guid == ImageFormat.Png.Guid)
            {
                return "png";
            }
            else if (format.Guid == ImageFormat.Jpeg.Guid)
            {
                return "jpg";
            }
            else if (format.Guid == ImageFormat.Bmp.Guid)
            {
                return "bmp";
            }
            else if (format.Guid == ImageFormat.Gif.Guid)
            {
                return "gif";
            }
            else if (format.Guid == ImageFormat.Icon.Guid)
            {
                return "ico";
            }
            else
            {
                return string.Empty;
            }
        }

        public static ImageFormat GetImageFormat(this string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return ImageFormat.Jpeg;
            }

            type = type.ToLower();

            if (type.Contains("png"))
            {
                return ImageFormat.Png;
            }
            else if (type.Contains("bmp"))
            {
                return ImageFormat.Bmp;
            }
            else if (type.Contains("gif"))
            {
                return ImageFormat.Gif;
            }
            else if (type.Contains("ico"))
            {
                return ImageFormat.Icon;
            }
            else
            {
                return ImageFormat.Jpeg;
            }
        }

        /// <summary>
        /// 获取图片压缩所需要的参数
        /// </summary>
        /// <param name="qualityValue"></param>
        /// <param name="colorDepthValue"></param>
        /// <returns></returns>
        public static EncoderParameters GetEncoderParameters(long qualityValue, long colorDepthValue)
        {
            var encoderParams = new EncoderParameters(2);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qualityValue);
            encoderParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, colorDepthValue);
            return encoderParams;
        }

        /// <summary>
        /// 获取同名文件最后可使用的版本文件名称
        /// </summary>
        /// <param name="absFileName">文件</param>
        /// <param name="version">当前检测的版本序号</param>
        /// <param name="deleteIfExists">存在时是否删除</param>
        /// <returns></returns>
        private static void GetLastVersionFile(string absFileName, int version, bool deleteIfExists, out string lastAbsFileName, out string lastRawFilePath)
        {
            lastAbsFileName = absFileName;
            lastRawFilePath = Path.Combine(Startup.PhysicalUploadRoot, lastAbsFileName.TrimStart('/').Replace("/", "\\"));

            try
            {
                Regex reg = new Regex(@"^(?<begin>/?.+?)(?<extension>\.[^\.]+)$", RegexOptions.IgnoreCase);

                if (reg.IsMatch(absFileName))
                {
                    string versionFileName = absFileName;

                    if (version > 0)
                    {
                        versionFileName = reg.Replace(absFileName, "${begin}(" + version + ")" + "${extension}");
                    }

                    string versionFilePath = GetUploadPhysicalPath(versionFileName);

                    if (File.Exists(versionFilePath))
                    {
                        GetLastVersionFile(absFileName, ++version, deleteIfExists, out lastAbsFileName, out lastRawFilePath);

                        if (deleteIfExists)
                        {
                            File.Delete(versionFilePath);
                        }
                    }
                    else
                    {
                        lastAbsFileName = versionFileName;
                        lastRawFilePath = versionFilePath;
                    }
                }
            }
            catch
            {
                //nothing
            }
        }

        /// <summary>
        /// 获取上传文件的最终物理路径
        /// </summary>
        /// <param name="filepath">文件相对路径</param>
        /// <returns></returns>
        public static string GetUploadPhysicalPath(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath)) return string.Empty;

            return Path.Combine(Startup.PhysicalUploadRoot, filepath.TrimStart('/').Replace("/", "\\"));
        }

        #endregion
    }
}