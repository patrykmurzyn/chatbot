import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  Injector,
  OnInit,
  ViewChild,
  computed,
  effect,
  inject,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ChatService } from '../../services/chat.service';
import { ConnectionStatusComponent } from '../connection-status/connection-status.component';
import { MessageComponent } from '../message/message.component';
import { MessageInputComponent } from '../message-input/message-input.component';
import { TypingIndicatorComponent } from '../typing-indicator/typing-indicator.component';
import { ConnectionStatus } from '../../models/connection-status.model';
import { Message } from '../../models/message.model';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    ConnectionStatusComponent,
    MessageComponent,
    MessageInputComponent,
    TypingIndicatorComponent,
  ],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss'],
})
export class ChatComponent implements OnInit {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;

  private chatService = inject(ChatService);
  private injector = inject(Injector);

  messages = this.chatService.messages;
  isGenerating = this.chatService.isGenerating;
  connectionStatus = this.chatService.connectionStatus;
  currentSessionId = this.chatService.currentSessionId;
  inputText = this.chatService.inputText;
  canSend = this.chatService.canSend;

  // New computed signal for hasInput
  hasInputSignal = computed(() => {
    const text = this.inputText();
    const result = text.trim().length > 0;
    console.log(
      `[ChatComponent] hasInputSignal computed. Input from service: "${text}". Result: ${result}`
    );
    return result;
  });

  constructor() {
    effect(() => {
      const messageCount = this.messages().length;
      if (messageCount > 0) {
        this.scrollToBottom();
      }
    });

    // Log when canSend or hasInputSignal changes for debugging UI
    effect(() => {
      console.log(`[ChatComponent Effect] canSend changed: ${this.canSend()}`);
    });
    effect(() => {
      console.log(
        `[ChatComponent Effect] hasInputSignal changed: ${this.hasInputSignal()}`
      );
    });
  }

  ngOnInit(): void {
    // Nothing needed here
  }

  /**
   * Start a new chat session
   */
  startNewChat(): void {
    this.chatService.createNewSession();
  }

  /**
   * Get connection status text
   */
  getConnectionStatusText(): string {
    const status = this.connectionStatus();
    switch (status) {
      case ConnectionStatus.Connected:
        return 'Connected';
      case ConnectionStatus.Connecting:
        return 'Connecting';
      case ConnectionStatus.Disconnected:
        return 'Disconnected';
      default:
        return 'Unknown';
    }
  }

  /**
   * Scroll to the bottom of the chat
   */
  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.messagesContainer?.nativeElement) {
        const element = this.messagesContainer.nativeElement;
        element.scrollTo({ top: element.scrollHeight, behavior: 'smooth' });
      }
    }, 0);
  }

  /**
   * trackBy function to preserve message components by their ID, preventing re-creation.
   */
  trackByMessageId(index: number, message: Message): string {
    return message.id || index.toString();
  }
}
