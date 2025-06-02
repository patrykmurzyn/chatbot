export interface MessageRating {
  id?: string;
  messageId?: string;
  isPositive: boolean;
  createdAt?: Date;
  updatedAt?: Date | null;
}

export interface Message {
  id?: string;
  sessionId?: string;
  content: string;
  isFromUser: boolean;
  isPartial: boolean;
  createdAt?: Date;
  rating?: MessageRating;
}
