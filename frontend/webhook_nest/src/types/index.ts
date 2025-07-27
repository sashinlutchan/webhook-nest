export interface Webhook {
  id: string;
  url: string;
}

export interface WebhookRequest {
  id: string;
  method: string;
  statusCode: number;
  timestamp: string;
  userAgent?: string;
  ip?: string;
  headers: Record<string, string>;
  data?: any;
  createdAt?: string;
}

export interface ApiError {
  message: string;
  code: string;
  details?: any;
}

export type WebHookEvents = Events[]

export interface Events {
  id?: string;
  headers: any;
  method: string;
  statusCode?: number;
  timestamp?: string;
  userAgent?: string;
  ip?: string;
  data?: any;
  createdAt?: string;
}


