using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PaperKiller.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PaperKiller.Services
{
    public class CognitiveService : ICognitiveService
    {
        private readonly string _apiUri;
        private readonly string _apiKey;

        public CognitiveService(string apiUri, string apiKey)
        {
            _apiUri = apiUri;
            _apiKey = apiKey;
        }

        public async Task<OcrResponse> CallRecognitionApi(string imageUrl)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["language"] = "unk";
            queryString["detectOrientation "] = "true";

            var byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { Url = imageUrl }));

            HttpResponseMessage response;
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync($"{_apiUri}?{queryString}", content);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            return new OcrResponse
            {
                IsSuccess = response.IsSuccessStatusCode,
                ImageUrl = imageUrl,
                RawResponse = responseContent
            };
        }
    }
}
