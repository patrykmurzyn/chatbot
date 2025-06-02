import { HttpClient } from '@angular/common/http';
import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { Observable, catchError, map, of, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { Message } from '../models/message.model';
import { Session } from '../models/session.model';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class SessionService {
  private readonly API_URL = `${environment.apiUrl}/api/v1`;
  private readonly STORAGE_KEY = 'chatbot_session_id';
  private readonly HTTP_OPTIONS = { withCredentials: true };
  private platformId = inject(PLATFORM_ID);

  constructor(private http: HttpClient) {}

  /**
   * Create a new chat session
   */
  createSession(): Observable<string> {
    return this.http
      .post<{ sessionId: string }>(
        `${this.API_URL}/sessions`,
        {},
        this.HTTP_OPTIONS
      )
      .pipe(
        map((response) => response.sessionId),
        tap((sessionId) => this.saveSessionId(sessionId)),
        catchError((error) => {
          console.error('Error creating session:', error);
          return of('');
        })
      );
  }

  /**
   * Get a session by ID
   */
  getSession(sessionId: string): Observable<Session | null> {
    if (!sessionId || sessionId === 'undefined') {
      console.warn('Invalid session ID provided:', sessionId);
      return of(null);
    }

    return this.http
      .get<Session>(`${this.API_URL}/sessions/${sessionId}`, this.HTTP_OPTIONS)
      .pipe(
        catchError((error) => {
          console.error(`Error fetching session ${sessionId}:`, error);
          return of(null);
        })
      );
  }

  /**
   * Get messages for a session
   */
  getMessages(sessionId: string): Observable<Message[]> {
    if (!sessionId || sessionId === 'undefined') {
      console.warn('Invalid session ID provided for getMessages:', sessionId);
      return of([]);
    }

    return this.http
      .get<Message[]>(
        `${this.API_URL}/sessions/${sessionId}/messages`,
        this.HTTP_OPTIONS
      )
      .pipe(
        catchError((error) => {
          console.error(
            `Error fetching messages for session ${sessionId}:`,
            error
          );
          return of([]);
        })
      );
  }

  /**
   * Send a message in a session
   */
  sendMessage(
    sessionId: string,
    content: string,
    characterId: number
  ): Observable<Message | null> {
    return this.http
      .post<Message>(
        `${this.API_URL}/sessions/${sessionId}/messages`,
        {
          content,
          characterId,
        },
        this.HTTP_OPTIONS
      )
      .pipe(
        catchError((error) => {
          console.error(
            `Error sending message in session ${sessionId}:`,
            error
          );
          return of(null);
        })
      );
  }

  /**
   * Rate a message
   */
  rateMessage(messageId: string, isPositive: boolean): Observable<boolean> {
    return this.http
      .put<any>(
        `${this.API_URL}/messages/${messageId}/rating`,
        { isPositive },
        this.HTTP_OPTIONS
      )
      .pipe(
        map(() => true),
        catchError((error) => {
          console.error(`Error rating message ${messageId}:`, error);
          return of(false);
        })
      );
  }

  /**
   * Save session ID to localStorage
   */
  saveSessionId(sessionId: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.STORAGE_KEY, sessionId);
    }
  }

  /**
   * Get saved session ID from localStorage
   */
  getSavedSessionId(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem(this.STORAGE_KEY);
    }
    return null;
  }

  /**
   * Clear saved session ID
   */
  clearSavedSessionId(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.STORAGE_KEY);
    }
  }
}
