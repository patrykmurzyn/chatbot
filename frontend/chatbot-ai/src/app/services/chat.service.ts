import {
  Injectable,
  PLATFORM_ID,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { SignalrService } from './signalr.service';
import { SessionService } from './session.service';
import { NotificationService } from './notification.service';
import { Message, MessageRating } from '../models/message.model';
import { ConnectionStatus } from '../models/connection-status.model';
import {
  EMPTY,
  Observable,
  Subject,
  catchError,
  finalize,
  firstValueFrom,
  from,
  of,
  switchMap,
  take,
  tap,
} from 'rxjs';

export interface ChatState {
  messages: Message[];
  isGenerating: boolean;
  connectionStatus: ConnectionStatus;
  currentSessionId: string;
  inputText: string;
}

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private signalrService = inject(SignalrService);
  private sessionService = inject(SessionService);
  private notificationService = inject(NotificationService);
  private platformId = inject(PLATFORM_ID);

  // State signals
  private state = signal<ChatState>({
    messages: [],
    isGenerating: false,
    connectionStatus: ConnectionStatus.Disconnected,
    currentSessionId: '',
    inputText: '',
  });

  // Exposed state slices
  messages = computed(() => this.state().messages);
  isGenerating = computed(() => this.state().isGenerating);
  connectionStatus = computed(() => this.state().connectionStatus);
  currentSessionId = computed(() => this.state().currentSessionId);
  inputText = computed(() => {
    console.log(
      '[ChatService] inputText computed, value:',
      this.state().inputText
    );
    return this.state().inputText;
  });

  // Derived states
  canSend = computed(() => {
    const hasText = this.inputText().trim().length > 0;
    const notGenerating = !this.isGenerating();
    const connected = this.connectionStatus() === ConnectionStatus.Connected;
    console.log(
      `[ChatService] canSend evaluated: inputText="${this.inputText()}", hasText=${hasText}, notGenerating=${notGenerating}, connected=${connected}, RESULT=${
        hasText && notGenerating && connected
      }`
    );
    return hasText && notGenerating && connected;
  });

  showCancel = computed(() => this.isGenerating());

  // Message subjects
  private partialMessageContent = signal<string>('');
  private currentPartialMessageId = signal<string | null>(null);

  constructor() {
    // Only set up subscriptions in the browser
    if (isPlatformBrowser(this.platformId)) {
      this.setupSubscriptions();

      // Wait a short time before initializing to avoid race conditions
      setTimeout(() => {
        this.initializeSession();
      }, 100);
    }
  }

  private setupSubscriptions(): void {
    // Handle connection status changes
    this.signalrService.connectionStatus$.subscribe((status) => {
      this.updateState({ connectionStatus: status });

      // Only join the session if we're connected and have a sessionId
      if (status === ConnectionStatus.Connected && this.currentSessionId()) {
        console.log(
          '[ChatService] Connection status changed to Connected, joining session:',
          this.currentSessionId()
        );
        this.joinSession(this.currentSessionId());
      }
    });

    // Handle new messages
    this.signalrService.messageReceived$.subscribe((message) => {
      this.addMessage(message);
    });

    // Handle generation started
    this.signalrService.generationStarted$.subscribe(() => {
      this.updateState({ isGenerating: true });
      this.partialMessageContent.set('');
      this.currentPartialMessageId.set(null);
    });

    // Handle partial messages - updated for incremental updates
    this.signalrService.partialMessageReceived$.subscribe((update) => {
      const { content: chunk, messageId } = update;

      // Update our internal content tracker
      const currentContent = this.partialMessageContent();
      const updatedContent = currentContent + chunk;
      this.partialMessageContent.set(updatedContent);

      // If this is the first chunk with a message ID, create a new partial message
      if (messageId) {
        console.log(
          '[ChatService] First chunk received, creating partial message'
        );

        // Create a new partial message
        const partialMessage: Message = {
          id: messageId,
          content: chunk,
          isFromUser: false,
          isPartial: true,
        };
        this.addMessage(partialMessage);

        // Store the ID for future updates
        this.currentPartialMessageId.set(messageId);
      }
      // Otherwise update existing message
      else if (this.currentPartialMessageId()) {
        console.log('[ChatService] Updating partial message with new chunk');
        this.updatePartialMessageWithNewChunk(chunk);
      } else {
        console.warn(
          '[ChatService] Received chunk but no message ID is set:',
          chunk
        );
      }
    });

    // Handle generation completed
    this.signalrService.generationCompleted$.subscribe((messageId) => {
      this.updateState({ isGenerating: false });
      this.partialMessageContent.set('');
      this.currentPartialMessageId.set(null);

      // Mark the message as no longer partial
      if (messageId) {
        this.markMessageAsComplete(messageId);
      }
    });

    // Handle generation cancelled
    this.signalrService.generationCancelled$.subscribe(() => {
      this.updateState({ isGenerating: false });
      this.notificationService.showInfo('Response generation cancelled');
      this.currentPartialMessageId.set(null);
    });
  }

  private updateState(partialState: Partial<ChatState>): void {
    console.log('[ChatService] Updating state with:', partialState);
    this.state.update((currentState) => ({ ...currentState, ...partialState }));
    console.log('[ChatService] New state:', this.state());
  }

  /**
   * Initialize the chat session
   */
  private async initializeSession(): Promise<void> {
    // Only run in browser environment
    if (!isPlatformBrowser(this.platformId)) return;
    console.log('[ChatService] Initializing session...');

    // Try to load existing session ID from localStorage
    const savedSessionId = this.sessionService.getSavedSessionId();
    console.log(
      '[ChatService] Saved session ID from localStorage:',
      savedSessionId
    );

    if (savedSessionId) {
      try {
        const session = await firstValueFrom(
          this.sessionService.getSession(savedSessionId)
        );
        console.log('[ChatService] Fetched session from service:', session);

        if (session) {
          this.updateState({ currentSessionId: savedSessionId });
          await this.loadMessages(savedSessionId);
          await this.joinSession(savedSessionId);
          return;
        } else {
          // Session not found, clear the invalid session ID
          console.log(
            '[ChatService] Session not found for ID, clearing saved ID.'
          );
          this.sessionService.clearSavedSessionId();
        }
      } catch (error) {
        console.error('[ChatService] Error loading saved session:', error);
        // Clear the invalid session ID on error
        this.sessionService.clearSavedSessionId();
      }
    }

    // If no session or invalid session, create a new one
    console.log('[ChatService] No valid saved session, creating new one.');
    this.createNewSession();
  }

  /**
   * Create a new chat session
   */
  async createNewSession(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    console.log('[ChatService] Creating new session...');

    try {
      const sessionId = await firstValueFrom(
        this.sessionService.createSession()
      );
      console.log('[ChatService] New session ID from service:', sessionId);

      if (sessionId) {
        this.updateState({
          currentSessionId: sessionId,
          messages: [],
        });

        await this.joinSession(sessionId);
        this.notificationService.showSuccess('New chat session created');
      } else {
        this.notificationService.showError('Failed to create a new session');
      }
    } catch (error) {
      console.error('[ChatService] Error creating new session:', error);
      this.notificationService.showError('Error creating new session');
    }
  }

  /**
   * Join a SignalR session
   */
  private async joinSession(sessionId: string): Promise<void> {
    if (
      !sessionId ||
      sessionId === 'undefined' ||
      !isPlatformBrowser(this.platformId)
    )
      return;

    console.log('[ChatService] Attempting to join session:', sessionId);
    console.log(
      '[ChatService] Current connection status for joinSession:',
      this.connectionStatus()
    );

    try {
      // First ensure we have a connection
      const connectionEnsured = await firstValueFrom(
        this.signalrService.ensureConnection()
      );
      console.log(
        '[ChatService] Connection ensured for joinSession:',
        connectionEnsured
      );

      if (!connectionEnsured) {
        console.error(
          '[ChatService] Cannot join session, SignalR connection not ensured.'
        );
        this.notificationService.showError(
          'Failed to establish SignalR connection for session.'
        );
        return;
      }

      // Then try to join the session
      const success = await firstValueFrom(
        this.signalrService.joinSession(sessionId)
      );

      if (success) {
        console.log('[ChatService] Successfully joined session:', sessionId);
      } else {
        this.notificationService.showError('Failed to connect to chat session');
      }
    } catch (error) {
      console.error('[ChatService] Error joining session:', error);
      this.notificationService.showError('Error connecting to chat session');
    }
  }

  /**
   * Load messages for a session
   */
  private async loadMessages(sessionId: string): Promise<void> {
    if (!sessionId || sessionId === 'undefined') return;
    console.log('[ChatService] Loading messages for session:', sessionId);

    try {
      const messagesResponse = await firstValueFrom(
        this.sessionService.getMessages(sessionId)
      );

      // Handle the new reference-preserving JSON format
      let messagesArray: Message[] = [];

      // If it's an array, use it directly
      if (Array.isArray(messagesResponse)) {
        messagesArray = messagesResponse;
      } else if (messagesResponse && typeof messagesResponse === 'object') {
        const seen = new Set<any>();
        // Recursive function to extract message-like objects
        const extract = (obj: any) => {
          if (obj && typeof obj === 'object') {
            // If this object looks like a Message (has id and content)
            if (
              'id' in obj &&
              'content' in obj &&
              typeof obj.content === 'string'
            ) {
              if (!seen.has(obj)) {
                seen.add(obj);
                messagesArray.push(obj as Message);
              }
            }
            // Recurse into arrays and objects
            for (const key in obj) {
              const val = obj[key];
              if (Array.isArray(val)) {
                val.forEach(extract);
              } else if (val && typeof val === 'object') {
                extract(val);
              }
            }
          }
        };
        extract(messagesResponse);
      }

      console.log('[ChatService] Processed messages array:', messagesArray);
      // Exclude any in-progress (partial) messages so they will be replayed as a single generating bubble
      messagesArray = messagesArray.filter((msg) => !msg.isPartial);
      this.updateState({ messages: messagesArray });
    } catch (error) {
      console.error('[ChatService] Error loading messages:', error);
      this.notificationService.showError('Error loading chat history');
    }
  }

  /**
   * Update input text
   */
  setInputText(text: string): void {
    console.log(`[ChatService] setInputText called with: "${text}"`);
    this.updateState({ inputText: text });
  }

  /**
   * Send a message
   */
  async sendMessage(characterId: number): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    console.log('[ChatService] Attempting to send message...');

    const content = this.inputText().trim();
    const sessionId = this.currentSessionId();

    console.log(
      `[ChatService] sendMessage details: content="${content}", sessionId="${sessionId}", isGenerating=${this.isGenerating()}`
    );

    if (!content || !sessionId || this.isGenerating()) {
      console.log('[ChatService] sendMessage preconditions not met. Aborting.');
      return;
    }

    // Clear input immediately for better UX - this is handled by MessageInputComponent now
    // this.updateState({ inputText: '' });

    try {
      // Add user message to UI immediately
      const userMessage: Message = {
        content,
        isFromUser: true,
        isPartial: false,
        sessionId,
        createdAt: new Date(),
      };

      this.addMessage(userMessage);

      // Ensure connection before sending
      const connectionEnsured = await firstValueFrom(
        this.signalrService.ensureConnection()
      );
      console.log(
        '[ChatService] Connection ensured for sendMessage:',
        connectionEnsured
      );

      if (!connectionEnsured) {
        console.error(
          '[ChatService] Cannot send message, SignalR connection not ensured.'
        );
        this.notificationService.showError(
          'Failed to establish SignalR connection for sending message.'
        );
        this.removeLastMessage(); // Remove optimistic user message
        return;
      }

      // Send message to API including character
      const messageSent = await firstValueFrom(
        this.sessionService.sendMessage(sessionId, content, characterId)
      );
      console.log('[ChatService] Message sent to API status:', messageSent);

      if (!messageSent) {
        this.notificationService.showError('Failed to send message');
        this.removeLastMessage(); // Remove optimistic user message
        return; // Don't proceed to SignalR if API call failed
      }

      // Initiate response generation through SignalR with character context
      const success = await firstValueFrom(
        this.signalrService.sendMessage(sessionId, content, characterId)
      );
      console.log('[ChatService] SignalR sendMessage success:', success);

      if (!success) {
        this.notificationService.showError('Failed to generate response');
        // Note: message is already in DB, but SignalR part failed.
        // Depending on desired behavior, might not remove from UI.
      }
    } catch (error) {
      console.error('[ChatService] Error sending message:', error);
      this.notificationService.showError('Error sending message');
      this.removeLastMessage(); // Remove optimistic user message on general error
    }
  }

  private removeLastMessage(): void {
    const currentMessages = this.messages();
    if (currentMessages.length > 0) {
      this.updateState({ messages: currentMessages.slice(0, -1) });
    }
  }

  /**
   * Cancel response generation
   */
  async cancelGeneration(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    console.log('[ChatService] Attempting to cancel generation...');

    if (!this.isGenerating()) {
      console.log('[ChatService] Not generating, no need to cancel.');
      return;
    }

    try {
      // Ensure connection first
      const connectionEnsured = await firstValueFrom(
        this.signalrService.ensureConnection()
      );
      console.log(
        '[ChatService] Connection ensured for cancelGeneration:',
        connectionEnsured
      );

      if (!connectionEnsured) {
        console.error(
          '[ChatService] Cannot cancel generation, SignalR connection not ensured.'
        );
        this.notificationService.showError(
          'Failed to establish SignalR connection for cancelling.'
        );
        return;
      }

      const success = await firstValueFrom(
        this.signalrService.cancelGeneration(this.currentSessionId())
      );
      console.log('[ChatService] SignalR cancelGeneration success:', success);

      if (!success) {
        this.notificationService.showError(
          'Failed to cancel response generation'
        );
      }
    } catch (error) {
      console.error('[ChatService] Error cancelling generation:', error);
      this.notificationService.showError('Error cancelling response');
    }
  }

  /**
   * Rate a message
   */
  async rateMessage(messageId: string, isPositive: boolean): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    console.log(
      `[ChatService] Rating message ${messageId} as ${
        isPositive ? 'positive' : 'negative'
      }`
    );

    try {
      const success = await firstValueFrom(
        this.sessionService.rateMessage(messageId, isPositive)
      );

      if (success) {
        // Update message in local state
        this.updateState({
          messages: this.messages().map((msg) =>
            msg.id === messageId ? { ...msg, rating: isPositive } : msg
          ),
        });
      } else {
        this.notificationService.showError('Failed to rate message');
      }
    } catch (error) {
      console.error('[ChatService] Error rating message:', error);
      this.notificationService.showError('Error rating message');
    }
  }

  /**
   * Add a message to the chat
   */
  private addMessage(message: Message): void {
    this.updateState({
      messages: [...this.messages(), message],
    });
  }

  /**
   * Clear chat history
   */
  clearChatHistory(): void {
    this.updateState({ messages: [] });
    this.notificationService.showInfo('Chat history cleared');
  }

  /**
   * Update a partial message with a new chunk of text
   */
  private updatePartialMessageWithNewChunk(chunk: string): void {
    const partialMessageId = this.currentPartialMessageId();
    if (!partialMessageId) return;

    // Instead of recreating the entire state, we'll just find and update the specific message
    const messages = this.messages();
    const messageIndex = messages.findIndex(
      (msg) => msg.id === partialMessageId
    );

    if (messageIndex !== -1) {
      // Create a new message instance with updated content by appending the chunk
      const updatedMessage = {
        ...messages[messageIndex],
        content: messages[messageIndex].content + chunk,
      };

      // Create a new array with the same message references except for the updated one
      const updatedMessages = [...messages];
      updatedMessages[messageIndex] = updatedMessage;

      // Update the state with the new array
      this.updateState({ messages: updatedMessages });
    }
  }

  /**
   * Mark a message as complete (no longer partial)
   */
  private markMessageAsComplete(messageId: string): void {
    this.updateState({
      messages: this.messages().map((msg) => {
        if (msg.id === messageId) {
          return { ...msg, isPartial: false };
        }
        return msg;
      }),
    });
  }
}
