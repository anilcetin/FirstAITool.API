using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace FirstAITool.API.SemanticKernel.Plugins;

public class TextAnalysisPlugin
{
    [KernelFunction, Description("Analyze the sentiment of the given text")]
    public async Task<string> AnalyzeSentiment(string text)
    {
        var prompt = @$"Analyze the sentiment of the following text and respond with either 'Positive', 'Negative', or 'Neutral', followed by a brief explanation:

Text: {text}

Provide your analysis in the following format:
Sentiment: [sentiment]
Explanation: [your explanation]";

        return prompt;
    }

    [KernelFunction, Description("Extract key topics from the given text")]
    public async Task<string> ExtractKeyTopics(string text)
    {
        var prompt = @$"Extract and list the main topics discussed in the following text:

Text: {text}

Please provide the topics in a bullet point format, with a brief explanation for each topic.";

        return prompt;
    }

    [KernelFunction, Description("Summarize the given text")]
    public async Task<string> Summarize(
        [Description("The text to summarize")] string text,
        [Description("The desired length of the summary in words")] int maxWords = 100)
    {
        var prompt = @$"Provide a concise summary of the following text in no more than {maxWords} words:

Text: {text}

Summary:";

        return prompt;
    }
} 