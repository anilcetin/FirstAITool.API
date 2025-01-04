using Microsoft.SemanticKernel;

namespace FirstAITool.API.SemanticKernel.Interfaces;

public interface ISemanticKernelService
{
    void SetModel(string modelName);
    Task<string> ChatCompletionAsync(string prompt);
    Task<string> TextCompletionAsync(string prompt);
    Task<float[]> GetEmbeddingsAsync(string text);
    Task<string> ExecutePromptTemplateAsync(string templateName, string input);
    Kernel GetKernel();
    Task<string> ExecutePluginAsync(string pluginName, string functionName, Dictionary<string, string> parameters);
} 