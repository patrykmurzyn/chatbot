# ChatbotAI Frontend

A modern Angular 19 application for real-time communication with the ChatbotAI backend.

## Features

- Real-time messaging with SignalR
- Message streaming for a natural typing experience
- Response rating system (👍/👎)
- Session management
- Responsive design with Angular Material
- Dark mode support

## Technology Stack

- Angular 19
- Angular Material
- SignalR for real-time communication
- Angular Signals for state management
- RxJS for reactive programming
- TypeScript 5.x

## Project Structure

The project follows a feature-based organization:

```
src/
├── app/
│   ├── animations/        # Shared animations
│   ├── components/        # UI components
│   │   ├── chat/
│   │   ├── message/
│   │   ├── message-input/
│   │   ├── connection-status/
│   │   └── typing-indicator/
│   ├── models/            # TypeScript interfaces
│   └── services/          # Core services
├── environments/          # Environment configurations
└── assets/               # Static assets
```

## Getting Started

### Prerequisites

- Node.js 20.x
- npm 11.x

### Installation

1. Clone the repository
2. Install dependencies:

```bash
cd frontend/chatbot-ai
npm install
```

3. Start the development server:

```bash
npm start
```

The application will be available at http://localhost:4200

## Building for Production

```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.

## Connecting to the Backend

By default, the application connects to the backend at `http://localhost:5225`. You can change this in the environment files if needed.

## Features Implemented

- [x] Session management
- [x] Real-time messaging
- [x] Response streaming
- [x] Message rating system
- [x] Message cancellation
- [x] Connection status monitoring
- [x] Responsive design
- [x] Dark mode support
