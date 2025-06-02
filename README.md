# ChatbotAI

A full-stack chatbot demo with real-time streaming answers in the style of your chosen character (Rick, Yoda, Sherlock, Socrates) using [shapeshifter-mcp](https://github.com/patrykmurzyn/shapeshifter-mcp) as the backend AI engine.

---

![ChatbotAI Screenshot](assets/Screenshot%20from%202025-06-03%2018-21-24.png)

---

## Features

- **Character-based AI**: Choose a character and get answers in their style (via Perplexity API, streamed chunk by chunk)
- **Modern stack**: Angular 19 frontend, .NET 9 backend, SignalR for real-time streaming
- **Session & history**: Persistent chat sessions, message ratings, and full chat history
- **Easy switching**: Add new characters or swap out the AI backend easily

## Quickstart

### 1. Start the MCP backend

Clone and run [shapeshifter-mcp](https://github.com/patrykmurzyn/shapeshifter-mcp) (Node.js, see its README for setup and Perplexity API key):

```bash
cd shapeshifter-mcp
npm install
npm run dev
```

### 2. Start the .NET backend

```bash
cd backend/src/ChatbotAI.API
# Set up your DB connection string in appsettings.json
# Apply migrations (first time only):
dotnet ef database update --project ../ChatbotAI.Infrastructure/ChatbotAI.Infrastructure.csproj --startup-project ChatbotAI.API.csproj
# Run the API
 dotnet run
```

### 3. Start the Angular frontend

```bash
cd frontend/chatbot-ai
npm install
npm start
```

- Frontend: [http://localhost:4200](http://localhost:4200)
- Backend API: [http://localhost:5225](http://localhost:5225)
- MCP: [http://localhost:3000](http://localhost:3000)

## How it works

- You pick a character and type a question.
- The backend streams your question to shapeshifter-mcp, which calls Perplexity and returns the answer chunk by chunk in the style of your chosen character.
- The frontend displays the answer as it streams in, with full session history and rating support.
