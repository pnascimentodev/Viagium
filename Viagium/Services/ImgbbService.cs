using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Viagium.Services
{
    public class ImgbbService
    {
        private readonly string _apiKey = "4fcd5ca8e14896779b62e1b17f61ba64";
        private readonly HttpClient _httpClient;

        public ImgbbService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            using var ms = new MemoryStream();
            await imageFile.CopyToAsync(ms);
            var base64Image = Convert.ToBase64String(ms.ToArray());

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(_apiKey), "key");
            form.Add(new StringContent(base64Image), "image");

            var response = await _httpClient.PostAsync("https://api.imgbb.com/1/upload", form);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonDocument.Parse(json);
            var url = obj.RootElement.GetProperty("data").GetProperty("url").GetString();
            return url;
        }
    }
}

