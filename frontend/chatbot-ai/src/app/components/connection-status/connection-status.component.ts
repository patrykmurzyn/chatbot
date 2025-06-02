import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { connectionStatusAnimation } from '../../animations/chat.animations';
import { ConnectionStatus } from '../../models/connection-status.model';
import { ChatService } from '../../services/chat.service';

@Component({
  selector: 'app-connection-status',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatTooltipModule],
  animations: [connectionStatusAnimation],
  templateUrl: './connection-status.component.html',
  styleUrls: ['./connection-status.component.scss'],
})
export class ConnectionStatusComponent {
  private chatService = inject(ChatService);

  status = this.chatService.connectionStatus;

  getStatusIcon(): string {
    switch (this.status()) {
      case ConnectionStatus.Connected:
        return 'wifi';
      case ConnectionStatus.Connecting:
        return 'sync';
      case ConnectionStatus.Disconnected:
        return 'wifi_off';
      default:
        return 'error';
    }
  }

  getStatusText(): string {
    switch (this.status()) {
      case ConnectionStatus.Connected:
        return 'Connected';
      case ConnectionStatus.Connecting:
        return 'Connecting...';
      case ConnectionStatus.Disconnected:
        return 'Disconnected';
      default:
        return 'Unknown status';
    }
  }
}
