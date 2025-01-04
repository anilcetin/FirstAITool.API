using FirstAITool.API.SemanticKernel.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FirstAITool.API.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SemanticKernelController : ControllerBase
{
    private readonly ISemanticKernelService _semanticKernelService;
    private readonly ILogger<SemanticKernelController> _logger;

    public SemanticKernelController(
        ISemanticKernelService semanticKernelService,
        ILogger<SemanticKernelController> logger)
    {
        _semanticKernelService = semanticKernelService;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] string prompt)
    {
        try
        {
            var response = await _semanticKernelService.ChatCompletionAsync(prompt);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chat completion");
            return StatusCode(500, "An error occurred during chat completion");
        }
    }

    [HttpPost("analyze-sentiment")]
    public async Task<IActionResult> AnalyzeSentiment([FromBody] string text)
    {
        try
        {
            var parameters = new Dictionary<string, string> { { "text", text } };
            var response = await _semanticKernelService.ExecutePluginAsync("TextAnalysis", "AnalyzeSentiment", parameters);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in sentiment analysis");
            return StatusCode(500, "An error occurred during sentiment analysis");
        }
    }

    [HttpPost("extract-topics")]
    public async Task<IActionResult> ExtractTopics([FromBody] string text)
    {
        try
        {
            var parameters = new Dictionary<string, string> { { "text", text } };
            var response = await _semanticKernelService.ExecutePluginAsync("TextAnalysis", "ExtractKeyTopics", parameters);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in topic extraction");
            return StatusCode(500, "An error occurred during topic extraction");
        }
    }

    [HttpPost("summarize")]
    public async Task<IActionResult> Summarize([FromBody] SummarizeRequest request)
    {
        try
        {
            var parameters = new Dictionary<string, string>
            {
                { "text", request.Text },
                { "maxWords", request.MaxWords.ToString() }
            };
            var response = await _semanticKernelService.ExecutePluginAsync("TextAnalysis", "Summarize", parameters);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in text summarization");
            return StatusCode(500, "An error occurred during text summarization");
        }
    }

    [HttpPost("embeddings")]
    public async Task<IActionResult> GetEmbeddings([FromBody] string text)
    {
        try
        {
            var embeddings = await _semanticKernelService.GetEmbeddingsAsync(text);
            return Ok(embeddings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings");
            return StatusCode(500, "An error occurred while generating embeddings");
        }
    }
}

public class SummarizeRequest
{
    public string Text { get; set; } = string.Empty;
    public int MaxWords { get; set; } = 100;
} 