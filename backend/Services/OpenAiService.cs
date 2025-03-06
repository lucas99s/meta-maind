using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Backend.Models;

namespace Backend.Services
{
    public class OpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAiEndpoint;
        private readonly string _apiKey;
        private readonly string _deploymentId;
        private readonly string _apiVersion;

        public OpenAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _openAiEndpoint = configuration["OpenAI:Endpoint"] ?? throw new ArgumentNullException("OpenAI Endpoint não configurado.");
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI ApiKey não configurado.");
            _deploymentId = configuration["OpenAI:DeploymentId"] ?? throw new ArgumentNullException("OpenAI DeploymentId não configurado.");
            _apiVersion = configuration["OpenAi:ApiVersion"] ?? throw new ArgumentNullException("OpenAI DeploymentId não configurado.");
        }

        public async Task<ChatResponseModel> GetChatResponse(ChatRequestModel request)
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "Você é um assistente útil." },
                    new { role = "user", content = request.Message }
                },
                max_tokens = request.MaxTokens,
                temperature = request.Temperature
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

            var response = await _httpClient.PostAsync($"{_openAiEndpoint}/openai/deployments/{_deploymentId}/chat/completions?api-version={_apiVersion}", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao chamar OpenAI: {jsonResponse}");
            }

            var responseObject = JsonSerializer.Deserialize<ChatResponseModel>(jsonResponse);
            return responseObject;
        }

        public async Task<ChatResponseModel> GetChatResponse(ChatRequestModel request, AzureSearchService azureSearchService)
        {
            // Busca documentos relevantes no Azure Search AI antes de chamar a OpenAI
            string searchResults = await azureSearchService.SearchDocumentsAsync(request.Message);

            // Extrai os documentos encontrados e formata um contexto para o OpenAI
            var searchData = JsonSerializer.Deserialize<AzureSearchResponseModel>(searchResults);
            string context = "Documentos relevantes encontrados:\n";

            foreach (var doc in searchData.Value)
            {
                context += $"- {doc.Title}: {doc.Content}\n";
            }

            // Envia o contexto para a OpenAI junto com a pergunta original do usuário
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "Você é um assistente útil. Use apenas as informações disponíveis nos documentos encontrados para responder as perguntas." },
                    new { role = "system", content = context },
                    new { role = "user", content = request.Message }
               },
                max_tokens = request.MaxTokens,
                temperature = request.Temperature
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

            var response = await _httpClient.PostAsync($"{_openAiEndpoint}/openai/deployments/{_deploymentId}/chat/completions?api-version={_apiVersion}", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao chamar OpenAI: {jsonResponse}");
            }

            var responseObject = JsonSerializer.Deserialize<ChatResponseModel>(jsonResponse);
            return responseObject;
        }


        public async Task<ChatResponseModel> GetChatResponseTest(ChatRequestModel request, AzureSearchService azureSearchService)
        {
            // Busca documentos relevantes no Azure Search AI antes de chamar a OpenAI
            string searchResults = await azureSearchService.SearchDocumentsAsync(request.Message);

            // Extrai os documentos encontrados e formata um contexto para o OpenAI
            var searchData = JsonSerializer.Deserialize<AzureSearchResponseModel>(searchResults);
            string context = "Documentos relevantes encontrados:\n";

            foreach (var doc in searchData.Value)
            {
                context += $"- {doc.Title}: {doc.Content}\n";
            }

            ChatResponseModel response = new ChatResponseModel
            {
                Choices = new List<Choice>
                {
                    new Choice
                    {
                        Message = new Message
                        {
                            Role = "system",
                            Content = context
                        }
                    }
                }.ToArray()
            };

            return response;
        }
    }
}
