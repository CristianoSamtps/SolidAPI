using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SensorApp.Models;

namespace SensorApp;

// Classe que centraliza todas as comunicações com o SOLID (API REST)
public class SolidService
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;

    public SolidService(string baseUrl)
    {
        _client = new HttpClient();
        _baseUrl = baseUrl;
    }

    // GET - verifica se um recurso existe (ex.: "moisture-sensor/humidity")
    public async Task<bool> Exists(string path)
    {
        HttpResponseMessage response = await _client.GetAsync($"{_baseUrl}/{path}");
        return response.IsSuccessStatusCode;
    }

    // POST - cria um recurso dentro do recurso pai e devolve o recurso criado
    // (parentPath vazio = criar diretamente em /api/solid)
    public async Task<ResourceRequest?> Create(string parentPath, ResourceRequest resource)
    {
        string url = _baseUrl;
        if (parentPath != "")
        {
            url = $"{_baseUrl}/{parentPath}";
        }

        HttpResponseMessage response = await _client.PostAsJsonAsync(url, resource);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Erro HTTP {(int)response.StatusCode}");
        }

        return await response.Content.ReadFromJsonAsync<ResourceRequest>();
    }
}
