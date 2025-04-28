# PromptSpark.Chat

![PromptSpark](https://img.shields.io/badge/PromptSpark-AI%20Chat%20Platform-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-purple)
![Semantic Kernel](https://img.shields.io/badge/Semantic%20Kernel-1.41.0-green)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-orange)

PromptSpark.Chat is an advanced real-time AI chat platform built with ASP.NET Core 9.0 that leverages Microsoft's Semantic Kernel to provide intelligent, context-aware conversations. The application implements a workflow-based architecture for managing flexible conversation flows with Large Language Models (LLMs).

## 📋 Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Workflows](#workflows)
- [API Documentation](#api-documentation)
- [Extending the Platform](#extending-the-platform)
- [Deployment](#deployment)

## 🔍 Overview

PromptSpark.Chat combines modern web technologies and AI capabilities to create an intelligent chat interface that can be tailored for various use cases. Its workflow-based architecture allows administrators to define conversational flows as a series of connected nodes, creating structured interactions for different scenarios.

The platform supports:

- Real-time streaming of AI responses
- Multiple workflow definitions for different conversation scenarios
- Visual workflow management and editing
- Integration with OpenAI's advanced language models

## ✨ Key Features

- **Real-Time Communication**: Powered by SignalR for instantaneous message delivery and streaming AI responses
- **AI-Driven Conversations**: Integration with Microsoft Semantic Kernel and OpenAI to provide contextual, high-quality responses
- **Workflow-Based Architecture**: Define and manage conversation flows through a configurable workflow system with nodes and connections
- **Visual Workflow Management**: Admin interface with flowchart visualization using Mermaid.js
- **Extensible Design**: Support for custom node types and conversation patterns
- **Multiple Workflow Templates**: Pre-defined templates for various conversation scenarios (medical, troubleshooting, general assistance)

## 🛠️ Technology Stack

### Backend

- **Framework**: ASP.NET Core 9.0
- **AI Integration**: Microsoft Semantic Kernel 1.41.0
- **Real-Time Communication**: SignalR
- **API Documentation**: Scalar for API reference

### Frontend

- **UI Framework**: Bootstrap
- **JavaScript Libraries**: jQuery
- **Flow Visualization**: Mermaid.js for workflow diagrams
- **Data Tables**: DataTables for tabular data presentation

### External Services

- **AI Provider**: OpenAI API (with support for gpt-4o by default)

## 🏗️ Architecture

PromptSpark.Chat follows a service-oriented architecture with the following key components:

### Controllers

- `HomeController`: Manages the main user interface views
- `WorkflowAdminController`: Handles workflow administration functionality
- API Controllers:
  - `ConversationController`: Manages conversation state and history
  - `WorkflowController`: Handles workflow operations and node navigation

### Services

- `ChatService`: Integrates with Semantic Kernel to generate AI responses
- `ConversationService`: Manages conversation state, history, and workflow progression
- `WorkflowService`: Loads, saves, and manages workflow definitions
- `PromptSparkHub`: SignalR hub for real-time communication

### Workflow System

The core of the application is the workflow system, which consists of:

- **Workflow**: A collection of nodes and a designated start node
- **Nodes**: Individual steps in the conversation, containing:
  - Questions or prompts to present to users
  - Possible answers/responses
  - Links to next nodes in the flow
- **QuestionTypes**:
  - `Options`: Multiple choice selection
  - `OptionsWithText`: Multiple choice with free text input
  - `Text`: Free-form text input
  - `Message`: Simple message display

### Data Flow

1. Client connects to the SignalR hub
2. Workflow is loaded based on configuration
3. Conversation starts at the designated start node
4. User interacts with the current node
5. System navigates to the next node based on user input
6. AI responses are generated using Semantic Kernel and streamed to the client
7. Conversation history is maintained throughout the session

## 🚀 Getting Started

### Prerequisites

- **.NET 9.0 SDK**
- **OpenAI API Key**
- **Development Environment**: Visual Studio 2022 or later/VS Code

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/MarkHazleton/WebSpark/PromptSpark.Chat.git
   cd PromptSpark.Chat
   ```

2. **Configure your OpenAI API key**

   Using User Secrets (Development):

   ```bash
   dotnet user-secrets set "OPENAI_API_KEY" "your-api-key-here"
   ```

   Or in appsettings.json (Production):

   ```json
   {
     "OPENAI_API_KEY": "your-api-key-here"
   }
   ```

3. **Build the application**

   ```bash
   dotnet build
   ```

4. **Run the application**

   ```bash
   dotnet run
   ```

5. **Access the application**
   - Chat Interface: `https://localhost:7105/Home/Chat`
   - Workflow Admin: `https://localhost:7105/WorkflowAdmin`

## ⚙️ Configuration

### Application Settings

The following settings can be configured in `appsettings.json` or through environment variables:

```json
{
  "OPENAI_API_KEY": "your-openai-api-key",
  "MODEL_ID": "gpt-4o",
  "Workflow": {
    "FilePath": "wwwroot/workflow.json",
    "DirectoryPath": "wwwroot/workflows"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Launch Profiles

The application includes the following launch profiles:

- **HTTP**: Runs on <http://localhost:5058>
- **HTTPS**: Runs on <https://localhost:7105> and <http://localhost:5058>
- **IIS Express**: Runs in IIS Express for development

## 🔄 Workflows

Workflows are the heart of PromptSpark.Chat, defining the conversation structure and flow.

### Workflow Structure

Workflows are defined as JSON files with the following structure:

```json
{
  "workflowId": "1",
  "workflowName": "Sample Workflow",
  "startNode": "1",
  "nodes": [
    {
      "id": "1",
      "question": "How can I help you today?",
      "questionType": 0,
      "answers": [
        {
          "response": "I need technical support",
          "nextNode": "2",
          "system": "You are a technical support specialist."
        },
        {
          "response": "I have a general question",
          "nextNode": "3",
          "system": "You are a helpful assistant."
        }
      ]
    },
    // Additional nodes...
  ]
}
```

### Question Types

- **Options (0)**: Multiple choice options
- **OptionsWithText (1)**: Multiple choice with text input
- **Text (2)**: Free-form text input
- **Message (3)**: Display message to user

### Pre-defined Workflows

The application includes several pre-defined workflows in the `wwwroot/workflows` directory:

- `chatassistant.json`: General-purpose chat assistant
- `troubleshoot.json`: Technical troubleshooting workflow
- `medicalmessage.json`: Healthcare-related assistance
- `hospitalmessage.json`: Hospital information workflow
- `coffee.json`: Coffee ordering demonstration
- `promptspark.json`: PromptSpark platform assistance

### Workflow Administration

The workflow administration interface (`/WorkflowAdmin`) provides the following features:

- View and select available workflows
- Add, edit, and delete workflow nodes
- Visualize workflow connections using flowcharts
- Tree view of workflow structure

## 📝 API Documentation

PromptSpark.Chat uses Scalar for API documentation and includes the following API endpoints:

### Conversation API

- `GET /api/Conversation/init/{workflowName?}`: Initialize a new conversation
- `POST /api/Conversation/chat`: Send a chat message
- `GET /api/Conversation/{conversationId}`: Get conversation state

### Workflow API

- `GET /api/Workflow/init`: Start a workflow
- `GET /api/Workflow/{nodeId}`: Get node details
- `POST /api/Workflow/next`: Navigate to next node

To access the API documentation, run the application and navigate to `/api/reference`.

## 🧩 Extending the Platform

### Adding Custom Workflows

1. Create a new JSON file in the `wwwroot/workflows` directory
2. Define your workflow structure following the schema
3. Access your workflow through the administration interface or API

### Creating Custom Node Types

To extend the available question types:

1. Add new enum values to the `QuestionType` enum in `Workflow.cs`
2. Update the frontend to handle the new question type in `site.js`
3. Implement the corresponding UI components in the chat interface

### Customizing AI Behavior

You can customize the AI behavior by modifying:

- The system prompt in each workflow answer
- The default model in `appsettings.json` or environment variables
- The chat completion implementation in `ChatService.cs`

## 🌐 Deployment

### Requirements

- .NET 9.0 Runtime
- HTTPS certificate for production
- Sufficient resources for handling concurrent SignalR connections

### Deployment Options

#### Azure App Service

1. Create an Azure App Service with .NET 9.0
2. Set application settings for OpenAI API key and other configurations
3. Deploy using Azure DevOps, GitHub Actions, or direct publish

#### Docker Deployment

The application can be containerized using the following Dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PromptSpark.Chat.csproj", "./"]
RUN dotnet restore "PromptSpark.Chat.csproj"
COPY . .
RUN dotnet build "PromptSpark.Chat.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PromptSpark.Chat.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PromptSpark.Chat.dll"]
```

#### On-Premises IIS

1. Install .NET 9.0 Hosting Bundle
2. Create an IIS website pointing to the published application
3. Configure application pool settings
4. Set up HTTPS binding

---

## 📊 Performance Considerations

- **SignalR Scale-Out**: For high-traffic scenarios, consider using Azure SignalR Service or Redis backplane
- **Model Selection**: Balance between model capabilities and response speed by configuring appropriate models
- **Caching**: Implement caching for frequently accessed workflows or responses
- **Monitoring**: Use Application Insights or other monitoring solutions to track performance

## 🔐 Security Best Practices

- **API Key Management**: Store API keys securely using Azure Key Vault or similar solutions
- **Input Validation**: Implement thorough validation for all user inputs
- **HTTPS**: Always use HTTPS in production environments
- **Authentication**: Consider adding authentication for the admin interface

## 📄 License

[Specify your license information here]

## 👥 Contributing

We welcome contributions to PromptSpark.Chat! This section outlines the process and best practices for contributing to this project.

### GitHub Workflow

1. **Fork the Repository**
   - Click the "Fork" button at the top right of the [repository page](https://github.com/MarkHazleton/WebSpark)
   - This creates your own copy of the repository under your GitHub account

2. **Clone Your Fork**

   ```bash
   git clone https://github.com/YOUR-USERNAME/WebSpark.git
   cd WebSpark/PromptSpark.Chat
   ```

3. **Add the Upstream Remote**

   ```bash
   git remote add upstream https://github.com/MarkHazleton/WebSpark.git
   ```

4. **Create a Feature Branch**

   ```bash
   git checkout -b feature/your-feature-name
   ```

   - Use descriptive branch names that reflect the changes you're making
   - Prefix with `feature/`, `bugfix/`, `docs/`, or `enhancement/` as appropriate

5. **Make Your Changes**
   - Follow the coding standards described below
   - Keep changes focused and related to a single issue

6. **Commit Your Changes**
   - Write clear, concise commit messages
   - Use present tense ("Add feature" not "Added feature")
   - Reference issue numbers in the commit message if applicable

   ```bash
   git commit -m "Add workflow visualization feature (#123)"
   ```

7. **Pull Latest Changes**

   ```bash
   git pull upstream main
   ```

   - Resolve any merge conflicts

8. **Push to Your Fork**

   ```bash
   git push origin feature/your-feature-name
   ```

9. **Create a Pull Request**
   - Go to your fork on GitHub and click "New Pull Request"
   - Provide a clear title and description of your changes
   - Link related issues

### Pull Request Guidelines

- **Keep PRs Focused**: Each PR should address a single concern
- **Include Tests**: Add/update tests for any changed functionality
- **Update Documentation**: Ensure README.md and other docs reflect your changes
- **Verify CI Checks**: Ensure all automated tests pass
- **Review Feedback**: Be responsive to feedback and make requested changes

### Code Standards

1. **Coding Style**
   - Follow C# coding conventions from [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
   - Use meaningful variable, method, and class names
   - Include XML documentation comments for public APIs

2. **Project Structure**
   - Follow the existing architecture and folder organization
   - Place new controllers in the Controllers folder
   - Place new services in the Services folder

3. **Testing**
   - Write unit tests for new functionality
   - Ensure all tests pass before submitting PRs

### Issue Reporting

- **Use Issue Templates**: Choose the appropriate template when reporting issues
- **Be Specific**: Clearly describe the issue, including steps to reproduce
- **Include Environment Details**: Specify your operating system, .NET version, etc.
- **Check Existing Issues**: Avoid creating duplicates

### Security Vulnerability Reporting

If you discover a security vulnerability, please do **NOT** create a public issue. Instead:

- Email [security@example.com](mailto:security@example.com) with details
- Follow responsible disclosure practices

### Code of Conduct

By participating in this project, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md). Please report unacceptable behavior to [conduct@example.com](mailto:conduct@example.com).

---
Built with ❤️ using ASP.NET Core and Microsoft Semantic Kernel
