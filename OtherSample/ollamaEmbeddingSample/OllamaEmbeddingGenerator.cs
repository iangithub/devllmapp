using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using System.Text;
using System.Text.Json;

public class OllamaEmbeddingGenerator : ITextEmbeddingGenerator
{
    /// <inheritdoc />
    public int MaxTokens { get; }

    private HttpClient _httpClient = new HttpClient();
    private readonly OllamaEmbeddingGeneratorConfig _config;


    public OllamaEmbeddingGenerator(OllamaEmbeddingGeneratorConfig config)
    {
        _config = config;
        MaxTokens = config.MaxToken;
    }

    /// <inheritdoc />
    public int CountTokens(string text)
    {
        // ... calculate and return the number of tokens ...
        return 0;
    }

    /// <inheritdoc />
    public async Task<Embedding> GenerateEmbeddingAsync(
        string text, CancellationToken cancellationToken = default)
    {
        var requestBody = new { Model = _config.EmbeddingModel, Prompt = text };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_config.Endpoint, content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<Dictionary<string, List<float>>>(responseContent);
            var embeddingArray = responseJson["embedding"].ToArray();
            return new Embedding(embeddingArray);
        }
        else
        {
            throw new KernelMemoryException("Failed to generate embedding for the given text");
        }
    }
}

public class OllamaEmbeddingGeneratorConfig
{
    public int MaxToken { get; set; } = 4096;
    public string Endpoint { get; set; }
    public string EmbeddingModel { get; set; }
}

