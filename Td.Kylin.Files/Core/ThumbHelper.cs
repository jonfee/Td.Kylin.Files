// ===============================================================================
// 功能描述: 
// 代码作者：伍朝辉
// 联 系QQ： 337883612
// 创建时间：2012-5-9 14:14:18
// 更新记录:     

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Td.Kylin.Files.Core
{
    /// <summary>
    /// 缩略图处理
    /// </summary>
    public static class ThumbHelper
    {
        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originaFile">文件名称和路径，缩略图将覆盖原图</param>
        /// <param name="toWidth">缩略图宽度</param>
        /// <param name="toHeight">缩略图高度</param>
        public static void MakeThumbnail(string originaFile, int toWidth, int toHeight)
        {
            MakeThumbnail(originaFile, originaFile, toWidth, toHeight);
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originaFile">文件名称和路径</param>
        /// <param name="thumbnailFile">缩略图文件名称和路径</param>
        /// <param name="toWidth">缩略图宽度</param>
        /// <param name="toHeight">缩略图高度</param>
        public static void MakeThumbnail(string originaFile, string thumbnailFile, int toWidth, int toHeight)
        {
            int w, h;

            MakeThumbnail(originaFile, thumbnailFile, toWidth, toHeight, out w, out h);
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originaFile">文件名称和路径</param>
        /// <param name="thumbnailFile">缩略图文件名称和路径</param>
        /// <param name="toWidth">缩略图宽度</param>
        /// <param name="toHeight">缩略图高度</param>
        /// <param name="originaWidth">原始宽度</param>
        /// <param name="originaHeight">原始高度</param>
        public static void MakeThumbnail(string originaFile, string thumbnailFile, int toWidth, int toHeight, out int originaWidth, out int originaHeight)
        {
            MakeThumbnail(originaFile, thumbnailFile, toWidth, toHeight, null, null, null, null, out originaWidth, out originaHeight, false);
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originaFile">文件名称和路径</param>
        /// <param name="thumbnailFile">缩略图文件名称和路径</param>
        /// <param name="toWidth">缩略图宽度</param>
        /// <param name="toHeight">缩略图高度</param>
        /// <param name="cutX">切割的 X 坐标</param>
        /// <param name="cutY">切割的 Y 坐标</param>
        /// <param name="cutWidth">切割的宽度</param>
        /// <param name="cutHeight">切割的高度</param>
        /// <param name="cut">是否切割多余部分，为 False 则保留原图所有部分，不足的部分填白。</param>
        public static void MakeThumbnail(string originaFile, string thumbnailFile, int toWidth, int toHeight, int? cutX, int? cutY, int? cutWidth, int? cutHeight, bool cut)
        {
            int originaWidth;
            int originaHeight;
            MakeThumbnail(originaFile, thumbnailFile, toWidth, toHeight, cutX, cutY, cutWidth, cutHeight, out originaWidth, out originaHeight,
                cut);
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originaFile">文件名称和路径</param>
        /// <param name="thumbnailFile">缩略图文件名称和路径</param>
        /// <param name="toWidth">缩略图宽度</param>
        /// <param name="toHeight">缩略图高度</param>
        /// <param name="cutX">切割的 X 坐标</param>
        /// <param name="cutY">切割的 Y 坐标</param>
        /// <param name="cutWidth">切割的宽度</param>
        /// <param name="cutHeight">切割的高度</param>
        /// <param name="originaWidth">原始宽度</param>
        /// <param name="originaHeight">原始高度</param>
        /// <param name="cut">是否切割多余部分，为 False 则保留原图所有部分，不足的部分填白。</param>
        public static void MakeThumbnail(string originaFile, string thumbnailFile, int toWidth, int toHeight, int? cutX, int? cutY, int? cutWidth, int? cutHeight, out int originaWidth, out int originaHeight, bool cut)
        {
            Image originaImage = Image.FromFile(originaFile);

            originaWidth = originaImage.Width; originaHeight = originaImage.Height;

            if (originaWidth == toWidth && originaHeight == toHeight)
            {
                File.Copy(originaFile, thumbnailFile, true);

                return;
            }

            int drawX = 0; int drawY = 0; int drawW = toWidth; int drawH = toHeight;

            #region 计算绘制尺寸

            if (!cutX.HasValue || !cutY.HasValue || !cutWidth.HasValue || !cutHeight.HasValue)
            {
                if (originaWidth < toWidth && originaHeight < toHeight)
                {
                    cutWidth = originaWidth; cutHeight = originaHeight; cutX = cutY = 0;

                    drawX = (toWidth - originaWidth) / 2;
                    drawY = (toHeight - originaHeight) / 2;

                    drawW = originaWidth; drawH = originaHeight;
                }
                else
                {
                    double multipleWidth = (double)originaWidth / (double)toWidth;
                    double multipleHeight = (double)originaHeight / (double)toHeight;

                    if (multipleWidth < multipleHeight)
                    {
                        if (cut)
                        {
                            cutWidth = originaWidth;
                            cutHeight = (int)(toHeight * multipleWidth);

                            cutX = 0;
                            cutY = (originaHeight - cutHeight) / 2;
                        }
                        else
                        {
                            cutHeight = toHeight;
                            cutWidth = (int)(originaWidth / multipleHeight);

                            cutX = cutY = 0;
                        }
                    }
                    else
                    {
                        if (cut)
                        {
                            cutWidth = (int)(toWidth * multipleHeight);
                            cutHeight = originaHeight;

                            cutX = (originaWidth - cutWidth) / 2;
                            cutY = 0;
                        }
                        else
                        {
                            cutHeight = (int)(originaHeight / multipleWidth);
                            cutWidth = toWidth;

                            cutX = cutY = 0;
                        }
                    }
                }
            }

            if (!cut)
            {
                drawX = (toWidth - cutWidth.Value)/2;
                drawY = (toHeight - cutHeight.Value)/2;

                drawW = cutWidth.Value;
                drawH = cutHeight.Value;

                cutWidth = originaWidth;
                cutHeight = originaHeight;
            }

            #endregion

            #region 创建缩略图

            Image bitmap = new Bitmap(toWidth, toHeight);

            Graphics g = Graphics.FromImage(bitmap);

            g.CompositingQuality = CompositingQuality.HighQuality;

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.SmoothingMode = SmoothingMode.HighQuality;

            g.Clear(Color.White);

            g.DrawImage(originaImage, new Rectangle(drawX, drawY, drawW, drawH), new Rectangle(cutX.Value, cutY.Value, cutWidth.Value, cutHeight.Value), GraphicsUnit.Pixel);

            #endregion

            #region 保存缩略图

            try
            {
                if (originaImage.RawFormat == ImageFormat.Gif)
                {
                    bitmap.Save(thumbnailFile, ImageFormat.Gif);
                }
                else
                {
                    ImageCodecInfo imageEncoder = GetEncoder(ImageFormat.Jpeg);

                    EncoderParameters myEncoderParameters = new EncoderParameters(1);

                    myEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 80L);

                    bitmap.Save(thumbnailFile, imageEncoder, myEncoderParameters);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                originaImage.Dispose();

                bitmap.Dispose();

                g.Dispose();
            }

            #endregion
        }

        /// <summary>
        /// 获取图像编码器
        /// </summary>
        /// <param name="format">图像文件格式</param>
        /// <returns>图片解码器</returns>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
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
    }
}



