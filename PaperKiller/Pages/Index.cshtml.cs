using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaperKiller.Model;

namespace PaperKiller.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHostingEnvironment _environment;
        private readonly IConfiguration _configuration;

        [BindProperty(SupportsGet = true)]
        public Paper Paper { get; set; }

        [BindProperty(SupportsGet = true)]
        public ScannedText Text { get; set; }

        public IndexModel(IHostingEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            if (Request.Query.TryGetValue("url", out StringValues urls))
            {
                Paper.Url = urls[0];
            }

            if (Request.Query.TryGetValue("error", out StringValues errors))
            {
                Text.Error = errors[0];
            }
            
            if (Request.Query.TryGetValue("name", out StringValues names))
            {
                var rawResponse = await LoadScannedText(names[0]);

                Text.ApiResponse = JObject.Parse(rawResponse);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = new HttpClient();

            var apiKey = _configuration.GetSection("CognitiveService").GetValue<string>("ApiKey");
            var serviceUri = _configuration.GetSection("CognitiveService").GetValue<string>("Uri");

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["language"] = "unk";
            queryString["detectOrientation "] = "true";

            var byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Paper));

            HttpResponseMessage response;
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync($"{serviceUri}?{queryString}", content);
            }
            
            if (response.IsSuccessStatusCode)
            {
                var paperName = await SaveScannedText(response);
                return RedirectToPage("/Index", new { name = paperName, url = Paper.Url });
            }

            return RedirectToPage("/Index",  new { error = $"Error. Check if file exists on specified url: {Paper.Url}" });
        }

        private async Task<string> LoadScannedText(string fileName)
        {
            var webRoot = _environment.WebRootPath; ;
            var path = $"{webRoot}\\data\\{fileName}.json";

            return await System.IO.File.ReadAllTextAsync(path);
        }

        private async Task<string> SaveScannedText(HttpResponseMessage response)
        {
            var webRoot = _environment.WebRootPath;
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var name = Guid.NewGuid().ToString();
            var path = $"{webRoot}/data/{name}.json";

            System.IO.File.WriteAllText(path, jsonResponse);

            return name;
        }
    }
}
