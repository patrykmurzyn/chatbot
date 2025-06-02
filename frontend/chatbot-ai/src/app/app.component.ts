import { Component } from '@angular/core';
import { ChatComponent } from './components/chat/chat.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ChatComponent],
  template: `<app-chat></app-chat>`,
  styleUrl: './app.component.scss',
})
export class AppComponent {
  title = 'ChatbotAI';
}
