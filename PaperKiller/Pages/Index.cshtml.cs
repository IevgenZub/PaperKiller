using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaperKiller.Model;

namespace PaperKiller.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHostingEnvironment _env;

        [BindProperty(SupportsGet = true)]
        public Paper Paper { get; set; }

        [BindProperty(SupportsGet = true)]
        public ScannedText Text { get; set; }

        public IndexModel(IHostingEnvironment env)
        {
            _env = env;
        }

        public void OnGet()
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
                var fileName = names[0];
                var webRoot = _env.WebRootPath;;
                var path = $"{webRoot}\\data\\{fileName}.json";

                Text.RawResponse = System.IO.File.ReadAllText(path);
                Text.DynamicResponse = JObject.Parse(Text.RawResponse);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "6205aee7a61d42ddb95452750164c688");

            queryString["language"] = "unk";
            queryString["detectOrientation "] = "true";
            var uri = "https://westeurope.api.cognitive.microsoft.com/vision/v1.0/ocr?" + queryString;

            HttpResponseMessage response;

            byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Paper));

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
            }
            
            if (response.IsSuccessStatusCode)
            {
                var paperName = await SaveScannedText(response);
                return RedirectToPage("/Index", new { name = paperName, url = Paper.Url });
            }

            return RedirectToPage("/Index",  new { error = $"Error. Check if file exists on specified url: {Paper.Url}" });
        }

        private async Task<string> SaveScannedText(HttpResponseMessage response)
        {
            var webRoot = _env.WebRootPath;
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var name = Guid.NewGuid().ToString();
            var path = $"{webRoot}/data/{name}.json";

            System.IO.File.WriteAllText(path, jsonResponse);

            return name;
        }
    }
}
