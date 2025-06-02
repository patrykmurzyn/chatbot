# Chatbot AI Backend - Product Requirements Document

## Overview

This document outlines the backend requirements for a chatbot AI prototype. The system will simulate AI responses with lorem ipsum text while establishing a foundation for future integration with real AI models.

## Technical Stack

- ASP.NET Core 9
- Entity Framework Core 9
- SQL Server
- SignalR for real-time communication
- MediatR for CQRS pattern implementation
- FluentValidation for request validation
- Mapster for object mapping (preferred over AutoMapper for better performance and less overhead)

## Project Structure

```
ChatbotAI.Backend/
├── src/
│   ├── ChatbotAI.API/              # Web API + SignalR Hub
│   ├── ChatbotAI.Application/      # Business logic
│   ├── ChatbotAI.Domain/           # Domain models
│   └── ChatbotAI.Infrastructure/   # Data access
└── tests/
    ├── ChatbotAI.UnitTests/
    └── ChatbotAI.IntegrationTests/
```

## Data Models

### Session

```csharp
public class Session
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public Dictionary<string, string> Metadata { get; set; } // For user-agent, IP, etc.
    public ICollection<Message> Messages { get; set; }
}
```

### Message

```csharp
public class Message
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Content { get; set; }
    public bool IsFromUser { get; set; }
    public bool IsPartial { get; set; }
    public DateTime CreatedAt { get; set; }
    public MessageRating Rating { get; set; }

    public Session Session { get; set; }
}
```

### MessageRating

```csharp
public class MessageRating
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public bool IsPositive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Message Message { get; set; }
}
```

## Core Features

### Session Management

- Create unique session IDs for each browser session
- Store session data in database
- Track session activity

### Chat Functionality

- Process and store user messages
- Generate and stream bot responses through SignalR
- Support cancellation of response generation
- Maintain chat history per session

### Response Rating System

- Allow rating of bot responses (👍/👎)
- Support changing ratings
- Store ratings in database

### Response Generation

- Generate lorem ipsum text with varying lengths:
  - Short: 1-2 sentences
  - Medium: 3-5 sentences
  - Long: Multiple paragraphs
- Stream responses word by word or sentence by sentence
- Handle cancellation with partial response storage

## API Endpoints

### Sessions

- `POST /api/v1/sessions` - Create a new session
  - Response: `{ sessionId: string }`

### Messages

- `GET /api/v1/sessions/{sessionId}/messages` - Get chat history
  - Response: Array of messages with ratings
- `POST /api/v1/sessions/{sessionId}/messages` - Send a user message
  - Request: `{ content: string }`
  - Response: Message created confirmation

### Ratings

- `PUT /api/v1/messages/{messageId}/rating` - Rate a message
  - Request: `{ isPositive: boolean }`
  - Response: Rating update confirmation

## SignalR Hub (ChatHub)

### Methods

- `JoinSession(sessionId)` - Connect client to specific session
- `SendMessage(sessionId, message)` - Send message to initiate response
- `CancelGeneration(sessionId)` - Cancel ongoing response generation

### Events

- `ReceiveMessage` - Client receives a new message
- `ResponseGenerationStarted` - Bot starts generating a response
- `ResponseGenerationProgress` - Stream partial response during generation
- `ResponseGenerationCompleted` - Response generation completed
- `ResponseGenerationCancelled` - Response generation was cancelled

## Response Generation Service

- Interface for easy replacement with real AI model
- Lorem ipsum generator with configurable response lengths
- Simulated typing delay between words/sentences
- Cancellation token support

## Database Schema

- Sessions table
- Messages table
- MessageRatings table

## Performance Requirements

- Support concurrent sessions
- Efficient streaming of responses
- Quick response to cancellation requests

## Error Handling

- Global exception handling middleware
- Proper exception handling
- Informative error messages
- Logging system with request/response logging (with data anonymization)
- Structured logging for better analysis

## SignalR Scaling

- Redis backplane for multi-instance deployments
- Proper connection management
- Graceful handling of connection drops

## Future Considerations

- Authentication system
- AI model integration points
- Analytics and reporting
- Performance optimizations
