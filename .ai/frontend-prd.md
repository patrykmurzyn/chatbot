# Chatbot AI Frontend - Product Requirements Document

## Overview

This document outlines the frontend requirements for a chatbot AI prototype. The interface will provide a seamless chat experience with message streaming, response rating, and session management capabilities.

## Technical Stack

- Angular 19 (latest version with standalone components)
- Angular Material 19 for UI components and styling
- @microsoft/signalr for real-time communication
- Angular Animations for UI transitions
- Angular Signals for state management
- RxJS for reactive programming
- TypeScript 5.6+

## Project Structure

```
chatbot-ai-frontend/
├── src/
│   ├── app/
│   │   ├── components/
│   │   │   ├── chat/
│   │   │   ├── message/
│   │   │   ├── message-input/
│   │   │   ├── connection-status/
│   │   │   └── typing-indicator/
│   │   ├── services/
│   │   │   ├── chat.service.ts
│   │   │   ├── signalr.service.ts
│   │   │   ├── session.service.ts
│   │   │   └── notification.service.ts
│   │   ├── models/
│   │   │   ├── message.model.ts
│   │   │   ├── session.model.ts
│   │   │   └── connection-status.model.ts
│   │   ├── animations/
│   │   │   ├── chat.animations.ts
│   │   │   └── message.animations.ts
│   │   └── app.component.ts
│   ├── assets/
│   └── styles/
└── package.json
```

## Core Features

### Session Management

- Store session ID in localStorage for persistence
- "New Chat" button to start a fresh session
- Automatically restore previous session on application start
- Clear history option for data cleanup

### SignalR Connection Management

- Auto-connect on application startup
- Reconnection attempts every 3-5 seconds when connection is lost
- Visual connection status indicator:
  - 🟢 Connected
  - 🟡 Connecting/Reconnecting
  - 🔴 Disconnected
- Display connection status in the application header/toolbar

### Chat Interface

- Load message history from localStorage on startup
- Auto-scroll to newest messages
- Smooth scrolling animations
- Visual distinction between user and bot messages
- Clean, intuitive layout with consistent spacing

### Message Input System

- Always-active input field (user can type during response generation)
- Dynamic send button states:
  - Send mode: Enabled when input has content and no generation in progress
  - Cancel mode: Active during response generation with visual indication
- Send button transforms to Cancel button during generation with animation
- Support for Enter key to send messages
- Button disabled states when appropriate

### Bot Response Streaming

- "Bot is typing..." indicator with animated dots
- Character-by-character text streaming to simulate real-time typing
- Ability to cancel generation at any point
- Handling and display of partial messages when cancelled

### Rating System

- 👍/👎 buttons for each bot response
- Visual feedback when rating is selected
- Ability to change rating at any time
- Persist ratings to backend via API calls

### State Management (Angular Signals)

- Core ChatState signal:

```typescript
interface ChatState {
  messages: Message[];
  isGenerating: boolean;
  connectionStatus: ConnectionStatus;
  currentSessionId: string;
  inputText: string;
}
```

- Computed signals for derived states (canSend, showCancel, etc.)
- Effect hooks for side effects (localStorage sync, auto-scroll)
- Centralized state management for predictable data flow

### Animations & UX

- Message animations:
  - Slide-in for new messages
  - Fade-in for streamed characters
  - Subtle effects for rating interactions
- Button animations:
  - Smooth transformation between Send/Cancel states
  - Pulsing animation during generation
  - Loading spinners where appropriate
- Scroll animations:
  - Smooth scrolling to new messages
  - Natural easing functions
- Connection status transitions:
  - Color changes between states
  - Pulse effect for reconnecting state

### Error Handling & Notifications

- Toast notifications using Angular Material Snackbar:
  - Connection errors
  - API failures
  - Success confirmations
- Automatic retry for failed requests with backoff strategy:
  - Maximum retry attempts (configurable)
  - Visual feedback during retry attempts
  - Clear user messaging when retries are exhausted
- Manual retry options for persistent errors
- Graceful degradation with localStorage when offline
- Fallback UI when backend is unavailable:
  - Read-only mode for viewing existing messages
  - Clear visual indication of limited functionality
- Integration with Sentry for error monitoring and diagnostics

### Performance Optimizations

- OnPush Change Detection for all components:
  - Proper integration with Angular Signals
  - Performance testing with large message volumes
- Virtual scrolling for long chat histories
- Lazy loading for larger components
- Lazy loading of assets (images, emoji sets, etc.)
- Proper subscription cleanup to prevent memory leaks
- CSS transitions preferred over framework animations where possible
- Animation performance testing on longer conversations

## UI Components (Angular Material)

- mat-toolbar: Header with connection status
- mat-card: Message containers
- mat-form-field + mat-input: Message input
- mat-button / mat-fab: Send/Cancel button
- mat-icon: Icons (send, stop, thumb_up, thumb_down)
- mat-snack-bar: Toast notifications
- mat-progress-spinner: Loading indicators
- mat-chip: Status indicators

## Responsive Design

- Desktop-first approach optimized for desktop browsers
- Compatible with Chrome, Firefox, and Safari
- Minimum supported resolution: 1024x768
- Basic responsive adaptations for different screen sizes:
  - Fluid scaling of fonts and UI elements
  - Proper keyboard handling for all devices
  - Touch-friendly interaction targets

## Accessibility (a11y)

- ARIA roles and attributes for all interactive elements
- Proper focus management:
  - Auto-focus on input field after sending messages
  - Focus returns to input after rating
  - Focus trapping in modals
- Keyboard navigation support
- Screen reader compatibility
- Color contrast ratios following WCAG guidelines
- Respect user motion preferences for animations

## Theme Support

- Dark mode and light mode themes using Angular Material theming
- Theme toggle button in UI
- Remember user preference in localStorage
- Respect prefers-color-scheme media query

## Interaction with Backend

- Integration with SignalR hub for real-time communication
- RESTful API calls for session management and message persistence
- Handling of streaming responses
- Cancellation token support

## Additional UX Features

- Feature flags system for easy enabling/disabling of features
- Keyboard shortcuts:
  - Enter: Send message
  - Ctrl+Enter/Cmd+Enter: Send message with line break
  - Escape: Cancel generation or dismiss notifications
- Copy functionality:
  - Option to copy message text to clipboard
  - Visual feedback on copy action
- Auto-focus on input field after sending messages
- Input field focus restoration after UI interactions
