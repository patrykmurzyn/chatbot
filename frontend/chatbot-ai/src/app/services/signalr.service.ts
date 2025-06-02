import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject, from, interval } from 'rxjs';
import { catchError, map, takeWhile, tap } from 'rxjs/operators';
import { ConnectionStatus } from '../models/connection-status.model';
import { Message } from '../models/message.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class SignalrService {
  private hubConnection?: signalR.HubConnection;
  private connectionStatusSubject = new BehaviorSubject<ConnectionStatus>(
    ConnectionStatus.Disconnected
  );
  private messageReceived = new Subject<Message>();
  private partialMessageReceived = new Subject<{
    content: string;
    messageId: string | null;
  }>();
  private generationStarted = new Subject<void>();
  private generationCompleted = new Subject<string>();
  private generationCancelled = new Subject<void>();

  public connectionStatus$ = this.connectionStatusSubject.asObservable();
  public messageReceived$ = this.messageReceived.asObservable();
  public partialMessageReceived$ = this.partialMessageReceived.asObservable();
  public generationStarted$ = this.generationStarted.asObservable();
  public generationCompleted$ = this.generationCompleted.asObservable();
  public generationCancelled$ = this.generationCancelled.asObservable();

  private reconnectAttempts = 0;
  private maxReconnectAttempts = 10;
  private baseReconnectDelay = 3000; // 3 seconds
  private platformId = inject(PLATFORM_ID);

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      this.initializeConnection();
    }
  }

  public initializeConnection(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    console.log('[SignalrService] initializeConnection called.');

    // Prevent re-initialization if already initialized or connecting
    if (
      this.hubConnection &&
      this.hubConnection.state !== signalR.HubConnectionState.Disconnected
    ) {
      console.log(
        '[SignalrService] initializeConnection: Hub connection already exists and is not disconnected. State:',
        this.hubConnection.state
      );
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/chatHub`, {
        withCredentials: true,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.previousRetryCount > this.maxReconnectAttempts) {
            console.log('[SignalrService] Max reconnect attempts reached.');
            return null; // Stop reconnecting
          }
          const delay = Math.min(
            this.baseReconnectDelay *
              Math.pow(2, retryContext.previousRetryCount),
            30000
          );
          console.log(`[SignalrService] Attempting reconnect in ${delay}ms`);
          return delay;
        },
      })
      .build();

    this.registerSignalREventHandlers();

    this.connectionStatusSubject.next(ConnectionStatus.Disconnected);
    this.startConnection();
  }

  private registerSignalREventHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveMessage', (message: Message) => {
      console.log('[SignalrService] Received message:', message);
      this.messageReceived.next(message);
    });

    this.hubConnection.on('ResponseGenerationStarted', () => {
      console.log('[SignalrService] Response generation started');
      this.generationStarted.next();
    });

    this.hubConnection.on(
      'ResponseGenerationProgress',
      (chunk: string, messageId: string | null) => {
        console.log(
          `[SignalrService] Response progress chunk: "${chunk}", messageId: ${
            messageId || 'null'
          }`
        );
        this.partialMessageReceived.next({
          content: chunk,
          messageId: messageId,
        });
      }
    );

    this.hubConnection.on(
      'ResponseGenerationCompleted',
      (messageId: string) => {
        console.log(
          '[SignalrService] Response generation completed for message:',
          messageId
        );
        this.generationCompleted.next(messageId);
      }
    );

    this.hubConnection.on('ResponseGenerationCancelled', () => {
      console.log('[SignalrService] Response generation cancelled');
      this.generationCancelled.next();
    });

    this.hubConnection.onreconnecting((error) => {
      console.warn('[SignalrService] SignalR reconnecting...', error);
      this.connectionStatusSubject.next(ConnectionStatus.Connecting);
      this.reconnectAttempts++;
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log(
        '[SignalrService] SignalR reconnected with ID:',
        connectionId
      );
      this.connectionStatusSubject.next(ConnectionStatus.Connected);
      this.reconnectAttempts = 0;
    });

    this.hubConnection.onclose((error) => {
      console.warn('[SignalrService] SignalR connection closed.', error);
      this.connectionStatusSubject.next(ConnectionStatus.Disconnected);
      // Automatic reconnect handles this, but ensure attemptReconnection is not called if max attempts reached.
      // if (this.reconnectAttempts < this.maxReconnectAttempts) {
      //   this.attemptReconnection(); // This might be redundant with withAutomaticReconnect
      // }
    });
  }

  private startConnection(): void {
    if (!this.hubConnection) {
      console.error(
        '[SignalrService] startConnection: No hub connection available.'
      );
      return;
    }

    // Stricter guard for startConnection
    if (this.hubConnection.state !== signalR.HubConnectionState.Disconnected) {
      console.warn(
        `[SignalrService] startConnection called but connection state is ${this.hubConnection.state}. Aborting start.`
      );
      return;
    }

    console.log('[SignalrService] Starting SignalR connection...');
    this.connectionStatusSubject.next(ConnectionStatus.Connecting);

    this.hubConnection
      .start()
      .then(() => {
        console.log(
          '[SignalrService] SignalR connection established. Connection ID:',
          this.hubConnection?.connectionId
        );
        this.connectionStatusSubject.next(ConnectionStatus.Connected);
        this.reconnectAttempts = 0;
      })
      .catch((err) => {
        console.error(
          '[SignalrService] Error while establishing connection: ',
          err
        );
        this.connectionStatusSubject.next(ConnectionStatus.Disconnected);
        // Let withAutomaticReconnect handle reconnections.
        // this.attemptReconnection(); // This might be redundant
      });
  }

  private attemptReconnection(): void {
    // This method might be redundant if withAutomaticReconnect is fully handling it.
    // Kept for now but consider removing if withAutomaticReconnect is sufficient.
    if (!this.hubConnection) return;
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.log(
        '[SignalrService] attemptReconnection: Max reconnect attempts reached.'
      );
      return;
    }
    if (this.hubConnection.state !== signalR.HubConnectionState.Disconnected) {
      console.warn(
        `[SignalrService] attemptReconnection called but state is ${this.hubConnection.state}. Aborting.`
      );
      return;
    }

    const delay = Math.min(
      this.baseReconnectDelay * Math.pow(2, this.reconnectAttempts),
      30000
    );
    console.log(
      `[SignalrService] attemptReconnection: Attempting reconnect in ${delay}ms`
    );
    setTimeout(() => this.startConnection(), delay);
  }

  public ensureConnection(): Observable<boolean> {
    console.log('[SignalrService] ensureConnection called.');
    if (!this.hubConnection) {
      console.error(
        '[SignalrService] ensureConnection: No hub connection available - initializing.'
      );
      this.initializeConnection(); // This will call startConnection internally
    }

    // If already connected, return true immediately
    if (
      this.hubConnection &&
      this.hubConnection.state === signalR.HubConnectionState.Connected
    ) {
      console.log(
        '[SignalrService] ensureConnection: Connection already established.'
      );
      return from([true]);
    }

    // If not connected, start connection if it's disconnected and return an observable that waits for connection
    if (
      this.hubConnection &&
      this.hubConnection.state === signalR.HubConnectionState.Disconnected
    ) {
      console.log(
        '[SignalrService] ensureConnection: Connection is disconnected, attempting to start.'
      );
      this.startConnection();
    } else if (this.hubConnection) {
      console.log(
        '[SignalrService] ensureConnection: Connection is in transition state, waiting. State:',
        this.hubConnection.state
      );
    }

    return this.connectionStatus$.pipe(
      map((status) => {
        // console.log('[SignalrService] ensureConnection status map:', status);
        return status === ConnectionStatus.Connected;
      }),
      tap((connected) =>
        console.log(
          '[SignalrService] ensureConnection: Waiting for connection... Current connected state:',
          connected
        )
      ),
      takeWhile((connected) => !connected, true),
      map((connected) => {
        // This map is to ensure the observable completes with the final connected state (true)
        // If takeWhile completes because connected is true, this will map it to true.
        console.log(
          '[SignalrService] ensureConnection: Observable completed with connected state:',
          connected
        );
        return connected;
      })
    );
  }

  public joinSession(sessionId: string): Observable<boolean> {
    if (!this.hubConnection) {
      console.error(
        '[SignalrService] joinSession: No hub connection available'
      );
      return from([false]);
    }
    if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.error(
        '[SignalrService] joinSession: Cannot join session - connection not in Connected state. Current state:',
        this.hubConnection.state
      );
      return from([false]);
    }
    console.log(
      '[SignalrService] Invoking JoinSession with sessionId:',
      sessionId
    );
    return from(this.hubConnection.invoke('JoinSession', sessionId)).pipe(
      map(() => {
        console.log(
          '[SignalrService] Successfully joined session via SignalR for sessionId:',
          sessionId
        );
        return true;
      }),
      catchError((err) => {
        console.error(
          `[SignalrService] Error joining session ${sessionId}:`,
          err
        );
        return from([false]);
      })
    );
  }

  public sendMessage(
    sessionId: string,
    message: string,
    characterId: number = 0
  ): Observable<boolean> {
    if (!this.hubConnection) {
      console.error(
        '[SignalrService] sendMessage: No hub connection available'
      );
      return from([false]);
    }
    if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.error(
        '[SignalrService] sendMessage: Cannot send message - connection not in Connected state. State:',
        this.hubConnection.state
      );
      return from([false]);
    }
    return from(
      this.hubConnection.invoke('SendMessage', sessionId, message, characterId)
    ).pipe(
      map(() => true),
      catchError((err) => {
        console.error('[SignalrService] Error sending message:', err);
        return from([false]);
      })
    );
  }

  public cancelGeneration(sessionId: string): Observable<boolean> {
    if (!this.hubConnection) {
      console.error(
        '[SignalrService] cancelGeneration: No hub connection available'
      );
      return from([false]);
    }
    if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.error(
        '[SignalrService] cancelGeneration: Cannot cancel generation - connection not in Connected state. State:',
        this.hubConnection.state
      );
      return from([false]);
    }
    return from(this.hubConnection.invoke('CancelGeneration', sessionId)).pipe(
      map(() => true),
      catchError((err) => {
        console.error('[SignalrService] Error cancelling generation:', err);
        return from([false]);
      })
    );
  }

  public disconnect(): void {
    if (this.hubConnection) {
      console.log('[SignalrService] Disconnecting SignalR hub connection.');
      this.hubConnection.stop();
    }
  }
}
