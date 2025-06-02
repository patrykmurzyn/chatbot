import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { typingIndicatorAnimation } from '../../animations/chat.animations';

@Component({
  selector: 'app-typing-indicator',
  standalone: true,
  imports: [CommonModule],
  animations: [typingIndicatorAnimation],
  templateUrl: './typing-indicator.component.html',
  styleUrls: ['./typing-indicator.component.scss'],
})
export class TypingIndicatorComponent {}
