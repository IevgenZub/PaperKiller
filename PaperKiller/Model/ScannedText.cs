using Newtonsoft.Json.Linq;

namespace PaperKiller.Model
{
    public class ScannedText
    {
        public string RawResponse { get; set; }

        public dynamic DynamicResponse { get; set; }

        public string Error { get; set; }
    }
}
