import { Message } from './message.model';

export interface Session {
  id: string;
  createdAt?: Date;
  lastActivity?: Date;
  metadata?: Record<string, string>;
}
