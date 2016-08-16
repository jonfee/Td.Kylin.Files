namespace Td.Kylin.Files.Models
{
    /// <summary>
    /// 上传结果
    /// </summary>
    public class UploadResult
    {
        /// <summary>
        /// 文件标记字段名称
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 上传后的文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}
