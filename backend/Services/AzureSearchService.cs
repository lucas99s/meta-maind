using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Backend.Services
{
    public class AzureSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _searchServiceName;
        private readonly string _indexerName;
        private readonly string _indexName;
        private readonly string _apiKey;

        public AzureSearchService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _searchServiceName = configuration["AzureSearch:ServiceName"]
                ?? throw new ArgumentNullException("ServiceName não configurado");

            _indexerName = configuration["AzureSearch:IndexerName"]
                ?? throw new ArgumentNullException("IndexerName não configurado");

            _indexName = configuration["AzureSearch:IndexName"] ?? throw new ArgumentNullException("IndexName não configurado");

            _apiKey = configuration["AzureSearch:ApiKey"]
                ?? throw new ArgumentNullException("ApiKey não configurado");
        }

        public async Task<bool> RunIndexerAsync()
        {
            string url = $"https://{_searchServiceName}.search.windows.net/indexers/{_indexerName}/run?api-version=2023-07-01-Preview";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("api-key", _apiKey);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return true; // Indexação iniciada com sucesso
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao iniciar a indexação: {errorMessage}");
            }
        }

        public async Task<string> SearchDocumentsAsync(string query)
        {
            string url = $"https://{_searchServiceName}.search.windows.net/indexes/{_indexName}/docs?api-version=2023-07-01-Preview"
                       + $"&search={Uri.EscapeDataString(query)}"
                       + $"&$top=1";
                       //+ $"&searchMode=all";  // Garante que todas as palavras da query sejam consideradas
                       //+ $"&scoringProfile=boosted-relevance"  // Usa o scoring profile criado anteriormente

            //string url = $"https://{_searchServiceName}.search.windows.net/indexes/{_indexName}/docs?api-version=2023-07-01-Preview"
            //           + $"&search={Uri.EscapeDataString(query)}"
            //           + $"&$top=3"  // Retorna apenas os 3 documentos mais relevantes
            //           + $"&queryType=semantic"  // Usa um modelo mais avançado para entender a busca
            //           + $"&semanticConfiguration=default"  // Garante que usa o modelo semântico configurado
            //           + $"&searchMode=all";  // Obriga a busca a considerar todas as palavras-chave

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("api-key", _apiKey);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao buscar documentos: {errorMessage}");
            }

            return await response.Content.ReadAsStringAsync();
        }

    }
}
