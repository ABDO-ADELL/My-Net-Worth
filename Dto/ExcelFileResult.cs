namespace PRISM.Dto
{
    public class ExcelFileResult
    {
        public byte[] Content { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public string FileName { get; set; } = default!;
    }

}
