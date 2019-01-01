using Newtonsoft.Json.Linq;

namespace PaperKiller.Model
{
    public class ScannedText
    {
        public dynamic ApiResponse { get; set; }

        public string FileName { get; set; }

        public string Error { get; set; }
    }
}
