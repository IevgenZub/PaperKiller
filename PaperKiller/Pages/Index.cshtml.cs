using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using PaperKiller.Model;
using PaperKiller.Services;

namespace PaperKiller.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHostingEnvironment _environment;
        private readonly ICognitiveService _cognitiveService;
        
        [BindProperty(SupportsGet = true)]
        public Paper Paper { get; set; }

        [BindProperty(SupportsGet = true)]
        public ScannedText Text { get; set; }

        public IndexModel(ICognitiveService cognitiveService, IHostingEnvironment environment)
        {
            _environment = environment;
            _cognitiveService = cognitiveService;
        }

        public async Task OnGetAsync()
        {
            if (Request.Query.TryGetValue("url", out StringValues urls))
            {
                Paper.Url = urls[0];
            }
            
            if (Request.Query.TryGetValue("file", out StringValues files))
            {
                var rawResponse = await LoadScannedText(files[0]);

                Text.ApiResponse = JObject.Parse(rawResponse);
            }

            if (Request.Query.TryGetValue("error", out StringValues errors))
            {
                Text.Error = errors[0];
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var response = await _cognitiveService.CallRecognitionApi(Paper.Url);

            if (response.IsSuccess)
            {
                var fileName = await SaveScannedText(response.RawResponse);
                return RedirectToPage("/Index", new { file = fileName, url = Paper.Url });
            }

            return RedirectToPage("/Index", new { error = $"Error. Check if file exists on specified url: {Paper.Url}" });
        }

        private async Task<string> LoadScannedText(string fileName)
        {
            var webRoot = _environment.WebRootPath;
            var path = $"{webRoot}\\data\\{fileName}.json";

            return await System.IO.File.ReadAllTextAsync(path);
        }

        private async Task<string> SaveScannedText(string text)
        {
            var webRoot = _environment.WebRootPath;
            var name = Guid.NewGuid().ToString();
            var path = $"{webRoot}/data/{name}.json";

            await System.IO.File.WriteAllTextAsync(path, text);

            return name;
        }
    }
}
