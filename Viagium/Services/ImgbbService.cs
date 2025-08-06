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
            // Configurar timeout para evitar requisições que ficam "penduradas"
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            // Validação básica
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Arquivo de imagem inválido");

            // Limitar tamanho do arquivo (ImgBB tem limite de 32MB)
            if (imageFile.Length > 32 * 1024 * 1024)
                throw new ArgumentException("Arquivo muito grande. Máximo 32MB");

            try
            {
                var form = new MultipartFormDataContent();
                form.Add(new StringContent(_apiKey), "key");
                
                // Upload direto do stream sem converter para Base64 primeiro
                var imageContent = new StreamContent(imageFile.OpenReadStream());
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType ?? "image/jpeg");
                form.Add(imageContent, "image", imageFile.FileName);

                var response = await _httpClient.PostAsync("https://api.imgbb.com/1/upload", form);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var obj = JsonDocument.Parse(json);
                
                // Verificar se a resposta contém os dados esperados
                if (!obj.RootElement.TryGetProperty("data", out var dataElement) ||
                    !dataElement.TryGetProperty("url", out var urlElement))
                {
                    throw new InvalidOperationException("Resposta inválida da API ImgBB");
                }
                
                var url = urlElement.GetString();
                return url ?? throw new InvalidOperationException("URL da imagem não encontrada na resposta");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new TimeoutException("Timeout ao fazer upload da imagem. Tente novamente.", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Erro na requisição para ImgBB: {ex.Message}", ex);
            }
        }
    }
}
