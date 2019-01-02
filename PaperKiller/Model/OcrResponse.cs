namespace PaperKiller.Model
{
    public class OcrResponse
    {
        public bool IsSuccess { get; set; }
        public string ImageUrl { get; set; }
        public string RawResponse { get; set; }
    }
}