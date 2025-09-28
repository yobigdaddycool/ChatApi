# ChatApi

A C# Web API application for Windows that saves chat conversations, remembers key points, and provides intelligent summarization capabilities.

## Features

- **Chat Conversation Management**: Create, read, update, and delete chat conversations
- **Message Storage**: Store messages with roles (user, assistant, system) and importance flags
- **Key Point Extraction**: Automatically extract important points from conversations using keyword analysis
- **Conversation Summarization**: Generate both short and detailed summaries of conversations
- **SQLite Database**: Local storage with Entity Framework Core
- **RESTful API**: Clean API endpoints for all operations

## Technology Stack

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core Web API** - Web API framework
- **Entity Framework Core** - ORM with SQLite provider
- **SQLite** - Local database storage
- **Newtonsoft.Json** - JSON processing for advanced features

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Windows OS (designed for Windows but cross-platform compatible)

### Installation

1. Clone the repository
2. Navigate to the project directory
3. Restore packages and build:
   ```bash
   dotnet restore
   dotnet build
   ```

### Running the Application

```bash
dotnet run
```

The API will be available at `http://localhost:5191`

## API Endpoints

### Root Endpoints
- `GET /` - API information
- `GET /health` - Health check

### Conversations
- `GET /api/conversations` - List all conversations
- `GET /api/conversations/{id}` - Get conversation with messages and key points
- `POST /api/conversations` - Create new conversation
- `PUT /api/conversations/{id}` - Update conversation
- `DELETE /api/conversations/{id}` - Delete conversation

### Messages
- `GET /api/conversations/{id}/messages` - Get messages for conversation
- `POST /api/conversations/{id}/messages` - Add message to conversation

### Key Points
- `GET /api/conversations/{id}/keypoints` - Get key points for conversation
- `POST /api/conversations/{id}/keypoints/extract` - Extract key points from conversation
- `POST /api/keypoints` - Add manual key point
- `PUT /api/keypoints/{id}` - Update key point
- `DELETE /api/keypoints/{id}` - Delete key point

### Summaries
- `GET /api/conversations/{id}/summary` - Get or generate conversation summary
- `POST /api/conversations/{id}/summary` - Generate/update conversation summary

## Usage Examples

### Create a Conversation
```bash
curl -X POST http://localhost:5191/api/conversations \
  -H "Content-Type: application/json" \
  -d '{"title": "Project Planning", "description": "Planning our new software project"}'
```

### Add a Message
```bash
curl -X POST http://localhost:5191/api/conversations/1/messages \
  -H "Content-Type: application/json" \
  -d '{"role": "user", "content": "We need to implement user authentication. This is critical for security.", "isImportant": true}'
```

### Extract Key Points
```bash
curl -X POST http://localhost:5191/api/conversations/1/keypoints/extract
```

### Generate Summary
```bash
curl -X POST http://localhost:5191/api/conversations/1/summary
```

## Key Features Explained

### Automatic Key Point Extraction
The system automatically analyzes messages for important keywords and phrases:
- **High Priority**: critical, essential, must, urgent
- **Medium Priority**: important, should, key
- **Categories**: Action Items, Decisions, Questions, Ideas, General

### Intelligent Summarization
Generates two types of summaries:
- **Short Summary**: Quick overview with participant count, message count, and key metrics
- **Detailed Summary**: Comprehensive summary with key points by category and important messages

### Message Importance
Messages can be marked as important, triggering automatic key point extraction.

## Database Schema

The application uses SQLite with the following main entities:
- **Conversations**: Chat conversation metadata
- **Messages**: Individual messages with role and content
- **KeyPoints**: Extracted or manually added key points
- **ConversationSummaries**: Generated summaries

## Development

### Project Structure
```
ChatApi/
├── Controllers/          # API controllers
├── Data/                # Database context
├── DTOs/                # Data transfer objects
├── Models/              # Entity models
├── Services/            # Business logic services
└── Program.cs           # Application startup
```

### Services
- **ChatService**: Manages conversations and messages
- **KeyPointService**: Handles key point extraction and management
- **SummarizationService**: Generates conversation summaries

## Future Enhancements

- Integration with AI services for advanced summarization
- Support for file attachments in messages
- Real-time chat capabilities with SignalR
- Advanced search and filtering
- Export functionality for conversations
- User authentication and authorization
- Multi-tenant support

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is open source and available under the MIT License.