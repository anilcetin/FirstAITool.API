namespace FirstAITool.API.SemanticKernel.Configuration;

public class SemanticKernelSettings
{
    public Dictionary<string, ModelConfig> Models { get; set; } = new();
    public string DefaultModel { get; set; } = "deepseek";
}

public class ModelConfig
{
    public string Type { get; set; } = "deepseek";
    public string ApiKey { get; set; } = string.Empty;
    public string ModelId { get; set; } = "deepseek-chat";
    public string EmbeddingModelId { get; set; } = "text-embedding-ada-002";
    public int MaxTokens { get; set; } = 2000;
    public float Temperature { get; set; } = 0.7f;
    public float TopP { get; set; } = 0.95f;
    public string? Endpoint { get; set; } // For Azure OpenAI
    public string BaseUrl { get; set; } = "https://api.deepseek.com/v1"; // For Deepseek
} 