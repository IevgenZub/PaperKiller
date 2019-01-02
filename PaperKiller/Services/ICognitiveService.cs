using PaperKiller.Model;
using System.Threading.Tasks;

namespace PaperKiller.Services
{
    public interface ICognitiveService
    {
        Task<OcrResponse> CallRecognitionApi(string imageUrl);
    }
}
