using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/openai")]
    public class AzureController : ControllerBase
    {
        private readonly OpenAiService _openAiService;
        private readonly AzureSearchService _azureSearchService;
        private readonly AzureStorageService _azureStorageService;

        public AzureController(OpenAiService openAiService, AzureSearchService azureSearchService, AzureStorageService azureStorageService)
        {
            _openAiService = openAiService;
            _azureSearchService = azureSearchService;
            _azureStorageService = azureStorageService;
        }

        //OpenAi

        //[HttpPost("chat")]
        //public async Task<IActionResult> Chat([FromBody] ChatRequestModel request)
        //{
        //    var response = await _openAiService.GetChatResponse(request);
        //    return Ok(response);
        //}

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestModel request)
        {
            try
            {
                var response = await _openAiService.GetChatResponse(request, _azureSearchService);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao processar chat.", error = ex.Message });
            }
        }

        //BlobStorage

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Arquivo inválido ou vazio");

            try
            {
                using var stream = file.OpenReadStream();

                // Criar metadados personalizados para indexação
                var metadata = new Dictionary<string, string>
                {
                    { "title", file.FileName },
                    { "uploadDate", DateTime.UtcNow.ToString("o") }
                };

                await _azureStorageService.UploadStreamAsync(file.FileName, stream, file.ContentType, metadata);

                bool success = await _azureSearchService.RunIndexerAsync();

                if (!success)
                    return StatusCode(500, new { message = "Falha ao iniciar a indexação." });

                return Ok(new { message = "Upload concluído com sucesso!", fileName = file.FileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao fazer upload do arquivo", error = ex.Message });
            }
        }

        //SearchAi

        [HttpPost("indexer/run")]
        public async Task<IActionResult> RunIndexer()
        {
            try
            {
                bool success = await _azureSearchService.RunIndexerAsync();

                if (success)
                    return Ok(new { message = "Indexação iniciada com sucesso!" });
                else
                    return StatusCode(500, new { message = "Falha ao iniciar a indexação." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao rodar indexação.", error = ex.Message });
            }
        }
    }
}
