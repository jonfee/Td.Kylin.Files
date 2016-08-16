using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Td.Kylin.Files.Core;
using Td.Kylin.Files.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Td.Kylin.Files.Controllers
{
    [Route("api/uploadify")]
    public class UploadifyController : Controller
    {
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="savepath">文件存储目录</param>
        /// <param name="fixedpath">是否固定存储在指定的savepath目录下，为false时在savepath目录后添加日期目录</param>
        /// <param name="name">指定文件名，为空时使用随机名</param>
        /// <param name="extension">需要保存的文件格式扩展名（如：jpg，为空时表示与源文件一致，此参数对需要压缩及图片超出限制宽高时无效）</param>
        /// <param name="beOverride">文件存在时是否覆盖</param>
        /// <param name="compressIfGreaterSize">超过该大小时进行质量压缩（单位：KB），该参数只对图片上传有效，图片超出限制宽高时失效</param>
        /// <param name="maxWidth">最大宽，该参数只对图片上传有效</param>
        /// <param name="maxHeight">最大高，该参数只对图片上传有效</param>
        /// <param name="cutIfOut">超过最大宽高时是否裁剪（为false时不足部分留白），该参数只对图片上传有效</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Upload(string savepath, bool fixedpath, string name = null, string extension = null, bool beOverride = false, long compressIfGreaterSize = 0, int maxWidth = 0, int maxHeight = 0, bool cutIfOut = false)
        {
            List<UploadResult> result = null;

            IFormFileCollection files = HttpContext.Request.Form.Files;

            if (null != files && files.Count > 0)
            {
                result = new List<UploadResult>();

                foreach (IFormFile file in files)
                {
                    UploadResult itemResult = UploadItem(file, savepath, fixedpath, name, extension, beOverride, compressIfGreaterSize, maxWidth, maxHeight, cutIfOut);

                    result.Add(itemResult);
                }
            }

            return Json(result);
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="savepath">文件存储目录</param>
        /// <param name="fixedpath">是否固定存储在指定的savepath目录下，为false时在savepath目录后添加日期目录</param>
        /// <param name="name">指定文件名，为空时使用随机名</param>
        /// <param name="extension">需要保存的文件格式扩展名（如：jpg，为空时表示与源文件一致，此参数对需要压缩及图片超出限制宽高时无效）</param>
        /// <param name="beOverride">文件存在时是否覆盖</param>
        /// <param name="compressIfGreaterSize">超过该大小时进行质量压缩（单位：KB），该参数只对图片上传有效，图片超出限制宽高时失效</param>
        /// <param name="maxWidth">最大宽，该参数只对图片上传有效</param>
        /// <param name="maxHeight">最大高，该参数只对图片上传有效</param>
        /// <param name="cutIfOut">超过最大宽高时是否裁剪（为false时不足部分留白），该参数只对图片上传有效</param>
        /// <returns></returns>
        private UploadResult UploadItem(IFormFile file, string savepath, bool fixedpath, string name = null, string extension = null, bool beOverride = false, long compressIfGreaterSize = 0, int maxWidth = 0, int maxHeight = 0, bool cutIfOut = false)
        {
            UploadResult result = new UploadResult();

            //标识字段名
            result.FieldName = file.GetFieldName();

            if (file == null)
            {
                result.Message = "文件对象为空";
            }
            else
            {
                //是否为图片文件
                bool isImage = file.IsImage();

                if (isImage)
                {
                    Image image = Image.FromStream(file.OpenReadStream(), true, true);

                    result = image.Save(file.Length, savepath, fixedpath, name, extension.GetImageFormat(), beOverride, compressIfGreaterSize, maxWidth, maxHeight, cutIfOut);
                }
                else
                {
                    result = file.Save(savepath, fixedpath, name, beOverride);
                }
            }

            //结果为null
            if (null == result)
            {
                result = new UploadResult
                {
                    Message = "上传失败"
                };
            }

            //标识字段名
            if (string.IsNullOrWhiteSpace(result.FieldName))
                result.FieldName = file.GetFieldName();

            return result;
        }
    }
}
