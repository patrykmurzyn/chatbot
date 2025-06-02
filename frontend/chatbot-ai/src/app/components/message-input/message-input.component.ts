import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  HostListener,
  ViewChild,
  OnInit,
  inject,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import {
  buttonTransformAnimation,
  pulseAnimation,
} from '../../animations/chat.animations';
import { ChatService } from '../../services/chat.service';
import { ConnectionStatus } from '../../models/connection-status.model';
import { CharactersService } from '../../services/characters.service';
import { CharacterDto } from '../../models/character.model';

@Component({
  selector: 'app-message-input',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
  ],
  animations: [buttonTransformAnimation, pulseAnimation],
  templateUrl: './message-input.component.html',
  styleUrls: ['./message-input.component.scss'],
})
export class MessageInputComponent implements OnInit {
  @ViewChild('messageInput') messageInputRef!: ElementRef;

  chatService = inject(ChatService);
  pulseState = 'inactive';
  ConnectionStatus = ConnectionStatus;

  canSend = this.chatService.canSend;
  isGenerating = this.chatService.isGenerating;
  connectionStatus = this.chatService.connectionStatus;

  // Character selection state
  characters: CharacterDto[] = [];
  selectedCharacter: number | null = null;
  private charactersService = inject(CharactersService);

  ngOnInit(): void {
    console.log('[MessageInputComponent] Fetching characters from API...');
    // Fetch available characters for selection
    this.charactersService.getCharacters().subscribe((chars) => {
      console.log('[MessageInputComponent] Received characters:', chars);
      this.characters = chars;
      if (chars.length > 0) {
        this.selectedCharacter = chars[0].id;
      }
    });
    // Focus input after character dropdown loads
    setTimeout(() => {
      if (this.messageInputRef?.nativeElement) {
        this.messageInputRef.nativeElement.focus();
      }
    }, 0);
  }

  onDomInputChange(event: Event): void {
    const newText = (event.target as HTMLTextAreaElement).value;
    console.log(
      `[MessageInputComponent] onDomInputChange called with: "${newText}"`
    );
    this.chatService.setInputText(newText);
  }

  @HostListener('document:keydown.escape')
  handleEscapeKey(): void {
    if (this.isGenerating()) {
      this.cancelGeneration();
    }
  }

  handleEnterKeyEvent(event: Event): void {
    const keyboardEvent = event as KeyboardEvent;
    this.handleEnterKey(keyboardEvent);
  }

  handleEnterKey(event: KeyboardEvent): void {
    if (!event.shiftKey && !event.ctrlKey && !event.metaKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  handleButtonClick(): void {
    if (this.isGenerating()) {
      this.cancelGeneration();
    } else {
      this.sendMessage();
    }
  }

  sendMessage(): void {
    if (!this.canSend()) {
      console.warn(
        '[MessageInputComponent] sendMessage called but canSend is false.'
      );
      return;
    }
    // Send message with selected character ID
    if (this.selectedCharacter != null) {
      this.chatService.sendMessage(this.selectedCharacter);
    } else {
      this.chatService.sendMessage(0);
    }
    this.chatService.setInputText('');

    if (this.messageInputRef?.nativeElement) {
      this.messageInputRef.nativeElement.focus();
    }
  }

  cancelGeneration(): void {
    this.pulseState = 'active';
    setTimeout(() => (this.pulseState = 'inactive'), 500);
    this.chatService.cancelGeneration();
  }
}
