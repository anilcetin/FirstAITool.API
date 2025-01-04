using FirstAITool.API.SemanticKernel.Configuration;
using FirstAITool.API.SemanticKernel.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Diagnostics.CodeAnalysis;

namespace FirstAITool.API.SemanticKernel.Services;

public class SemanticKernelService : ISemanticKernelService
{
    private readonly Dictionary<string, Kernel> _kernels = new();
    private readonly ILogger<SemanticKernelService> _logger;
    private readonly SemanticKernelSettings _settings;
    private string _currentModel;

    public SemanticKernelService(SemanticKernelSettings settings, ILogger<SemanticKernelService> logger)
    {
        _settings = settings;
        _logger = logger;
        _currentModel = settings.DefaultModel;
        
        foreach (var (modelName, config) in settings.Models)
        {
            var builder = Kernel.CreateBuilder();
            
            switch (config.Type.ToLowerInvariant())
            {
                case "azure":
                    builder.AddAzureOpenAIChatCompletion(
                        deploymentName: config.ModelId,
                        endpoint: config.Endpoint!,
                        apiKey: config.ApiKey);
                    break;
                
                case "deepseek":
                    var httpClient = new HttpClient { BaseAddress = new Uri(config.BaseUrl) };
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
                    
                    builder.AddOpenAIChatCompletion(
                        modelId: config.ModelId,
                        apiKey: config.ApiKey,
                        serviceId: "Deepseek",
                        httpClient: httpClient);
                    break;
                
                case "openai":
                    builder.AddOpenAIChatCompletion(
                        modelId: config.ModelId,
                        apiKey: config.ApiKey);
                    break;
                
                default:
                    throw new ArgumentException($"Unsupported model type: {config.Type}");
            }
            
            _kernels[modelName] = builder.Build();
        }
    }

    private Kernel GetCurrentKernel()
    {
        if (!_kernels.ContainsKey(_currentModel))
        {
            throw new InvalidOperationException($"Model {_currentModel} not found in configuration");
        }
        return _kernels[_currentModel];
    }

    public void SetModel(string modelName)
    {
        if (!_kernels.ContainsKey(modelName))
        {
            throw new ArgumentException($"Model {modelName} not found in configuration");
        }
        _currentModel = modelName;
    }

    public async Task<string> ChatCompletionAsync(string prompt)
    {
        try
        {
            var config = _settings.Models[_currentModel];
            var arguments = new KernelArguments();
            arguments.Add("MaxTokens", config.MaxTokens);
            arguments.Add("Temperature", config.Temperature);
            arguments.Add("TopP", config.TopP);
            
            var result = await GetCurrentKernel().InvokePromptAsync(prompt, arguments);
            return result.GetValue<string>() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during chat completion");
            throw;
        }
    }

    public async Task<string> TextCompletionAsync(string prompt)
    {
        return await ChatCompletionAsync(prompt);
    }

    public async Task<float[]> GetEmbeddingsAsync(string text)
    {
        try
        {
            var config = _settings.Models[_currentModel];
            var embeddingKernel = Kernel.CreateBuilder();
            
            #pragma warning disable SKEXP0011
            switch (config.Type.ToLowerInvariant())
            {
                case "azure":
                    embeddingKernel.AddAzureOpenAITextEmbeddingGeneration(
                        deploymentName: config.EmbeddingModelId,
                        endpoint: config.Endpoint!,
                        apiKey: config.ApiKey);
                    break;
                
                case "deepseek":
                    var httpClient = new HttpClient { BaseAddress = new Uri(config.BaseUrl) };
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
                    
                    embeddingKernel.AddOpenAITextEmbeddingGeneration(
                        modelId: config.EmbeddingModelId,
                        apiKey: config.ApiKey,
                        httpClient: httpClient);
                    break;
                
                case "openai":
                    embeddingKernel.AddOpenAITextEmbeddingGeneration(
                        modelId: config.EmbeddingModelId,
                        apiKey: config.ApiKey);
                    break;
                
                default:
                    throw new ArgumentException($"Unsupported model type: {config.Type}");
            }
            #pragma warning restore SKEXP0011

            var kernel = embeddingKernel.Build();
            var arguments = new KernelArguments();
            arguments.Add("text", text);
            
            var result = await kernel.InvokePromptAsync(text, arguments);
            var embedding = result.GetValue<float[]>();
            return embedding ?? Array.Empty<float>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings");
            throw;
        }
    }

    public async Task<string> ExecutePromptTemplateAsync(string templateName, string input)
    {
        try
        {
            var promptTemplate = GetCurrentKernel().CreateFunctionFromPrompt(templateName);
            var result = await GetCurrentKernel().InvokeAsync(promptTemplate, new KernelArguments { { "input", input } });
            return result.GetValue<string>() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing prompt template");
            throw;
        }
    }

    public Kernel GetKernel()
    {
        return GetCurrentKernel();
    }

    public async Task<string> ExecutePluginAsync(string pluginName, string functionName, Dictionary<string, string> parameters)
    {
        try
        {
            var function = GetCurrentKernel().Plugins[pluginName][functionName];
            var kernelArguments = new KernelArguments();
            
            foreach (var param in parameters)
            {
                kernelArguments.Add(param.Key, param.Value);
            }

            var result = await GetCurrentKernel().InvokeAsync(function, kernelArguments);
            return result.GetValue<string>() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing plugin");
            throw;
        }
    }
} 