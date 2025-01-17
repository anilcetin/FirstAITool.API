# FirstAITool.API

A .NET 9.0 Web API project that demonstrates integration with multiple AI services using Semantic Kernel.

## Features

- Multi-model AI integration (Deepseek, OpenAI, Azure OpenAI)
- Clean Architecture implementation
- JWT Authentication
- Entity Framework Core with SQL Server
- Semantic Kernel integration with plugins
- RESTful API endpoints

## Project Structure

```
FirstAITool.API/
├── FirstAITool.API.Application/     # Application layer (services, DTOs, interfaces)
├── FirstAITool.API.Domain/          # Domain layer (entities, value objects)
├── FirstAITool.API.Infrastructure/  # Infrastructure layer (repositories, DB context)
├── FirstAITool.API.SemanticKernel/  # Semantic Kernel integration
└── FirstAITool.API.WebAPI/          # API layer (controllers, configuration)
```

## Prerequisites

- .NET 9.0 SDK
- SQL Server
- API keys for the AI services you plan to use:
  - Deepseek API key
  - OpenAI API key (optional)
  - Azure OpenAI credentials (optional)

## Configuration

1. Update the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=FirstAITool;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

2. Configure your AI service credentials in `appsettings.json`:
```json
"SemanticKernel": {
  "DefaultModel": "deepseek",
  "Models": {
    "deepseek": {
      "Type": "deepseek",
      "ApiKey": "your-deepseek-api-key-here",
      "ModelId": "deepseek-chat",
      ...
    },
    "gpt4": {
      "Type": "openai",
      "ApiKey": "your-openai-api-key-here",
      ...
    },
    "azure-gpt4": {
      "Type": "azure",
      "ApiKey": "your-azure-api-key-here",
      "Endpoint": "https://your-azure-endpoint.openai.azure.com/",
      ...
    }
  }
}
```

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/FirstAITool.API.git
```

2. Navigate to the project directory:
```bash
cd FirstAITool.API
```

3. Restore dependencies:
```bash
dotnet restore
```

4. Apply database migrations:
```bash
cd FirstAITool.API.WebAPI
dotnet ef database update
```

5. Run the application:
```bash
dotnet run
```

## API Endpoints

### Authentication
- POST `/api/auth/register` - Register a new user
- POST `/api/auth/login` - Login and get JWT token

### Semantic Kernel
- POST `/api/semantickernel/chat` - Chat completion
- POST `/api/semantickernel/analyze-sentiment` - Analyze text sentiment
- POST `/api/semantickernel/extract-topics` - Extract key topics from text
- POST `/api/semantickernel/summarize` - Summarize text
- POST `/api/semantickernel/embeddings` - Generate text embeddings

## Example Usage

### Chat Completion
```http
POST /api/semantickernel/chat
Content-Type: application/json
Authorization: Bearer your-jwt-token

"What is the capital of France?"
```

### Text Analysis
```http
POST /api/semantickernel/analyze-sentiment
Content-Type: application/json
Authorization: Bearer your-jwt-token

"This product is amazing! I love it."
```

## Switching Between AI Models

You can switch between different AI models in your code:

```csharp
// Use default model (deepseek)
await _semanticKernelService.ChatCompletionAsync("Hello!");

// Switch to GPT-4
_semanticKernelService.SetModel("gpt4");
await _semanticKernelService.ChatCompletionAsync("Hello!");

// Switch to Azure OpenAI
_semanticKernelService.SetModel("azure-gpt4");
await _semanticKernelService.ChatCompletionAsync("Hello!");
```

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- [Semantic Kernel](https://github.com/microsoft/semantic-kernel)
- [.NET](https://dotnet.microsoft.com/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) 
