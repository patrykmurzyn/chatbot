import { CommonModule } from '@angular/common';
import {
  Component,
  Input,
  OnInit,
  inject,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  SimpleChanges,
  OnChanges,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  fadeInAnimation,
  ratingAnimation,
  slideInAnimation,
  typewriterAnimation,
} from '../../animations/chat.animations';
import { Message } from '../../models/message.model';
import { ChatService } from '../../services/chat.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-message',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
  ],
  animations: [
    slideInAnimation,
    fadeInAnimation,
    ratingAnimation,
    typewriterAnimation,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './message.component.html',
  styleUrls: ['./message.component.scss'],
})
export class MessageComponent implements OnInit, OnChanges {
  @Input() message!: Message;

  stableContent = '';
  newChunk: string | null = null;

  private previousContent = '';
  private cdRef = inject(ChangeDetectorRef);
  private chatService = inject(ChatService);
  private notificationService = inject(NotificationService);

  ngOnInit(): void {
    if (!this.message) {
      console.error('Message component initialized without message');
      return;
    }

    this.updateContent(this.message.content || '');
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['message'] && this.message) {
      const currentContent = this.message.content || '';

      if (currentContent !== this.previousContent) {
        this.updateContent(currentContent);
      }
    }
  }

  private updateContent(content: string): void {
    if (content === this.previousContent) return;

    // If first update or total refresh needed
    if (
      this.previousContent === '' ||
      content.length <= this.previousContent.length
    ) {
      this.stableContent = this.formatContent(content);
      this.newChunk = null;
    } else {
      // Handle incremental update - split into stable content and new chunk
      this.stableContent = this.formatContent(this.previousContent);
      this.newChunk = this.formatContent(
        content.substring(this.previousContent.length)
      );

      // After a short delay, merge the new chunk into stable content
      setTimeout(() => {
        this.stableContent = this.formatContent(content);
        this.newChunk = null;
        this.cdRef.markForCheck();
      }, 500);
    }

    this.previousContent = content;
    this.cdRef.markForCheck();
  }

  formatContent(content: string): string {
    if (!content) return '';
    // Convert line breaks to <br> tags
    return content.replace(/\n/g, '<br>');
  }

  copyToClipboard(content: string): void {
    navigator.clipboard
      .writeText(content)
      .then(() => this.notificationService.showSuccess('Copied to clipboard'))
      .catch(() =>
        this.notificationService.showError('Failed to copy to clipboard')
      );
  }

  rateMessage(isPositive: boolean): void {
    if (!this.message.id) {
      this.notificationService.showError('Cannot rate this message');
      return;
    }

    this.chatService.rateMessage(this.message.id, isPositive);
  }

  getAnimationState(isPositive: boolean): string {
    if (this.message.rating === undefined || this.message.rating === null)
      return 'inactive';
    return this.message.rating === isPositive ? 'active' : 'inactive';
  }
}
