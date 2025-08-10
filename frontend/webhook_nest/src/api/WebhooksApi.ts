import axios from 'axios';
import type { AxiosInstance } from 'axios';
import type { Webhook, WebHookEvents } from '@/types';
import { getEnvOrNull } from '@/utils/env';

type CreateWebhookResponse ={
  id: string;
}
function buildWebhookUrl(id: string): string {
  const baseUrl = getEnvOrNull('VITE_BASE_URL');
  const cleanBaseUrl = baseUrl.replace(/\/$/, '');
  return `${cleanBaseUrl}/api/v1/webhook/updatewebhook/${id}`;
}

class WebhookApi {
  private apiClient: AxiosInstance;

  constructor() {
    this.apiClient = axios.create({
      baseURL: getEnvOrNull('VITE_BASE_URL'),
      timeout: 15000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.apiClient.interceptors.request.use(
      config => {
        return config;
      },
      error => Promise.reject(error)
    );

    this.apiClient.interceptors.response.use(
      response => response,
      error => {
        return Promise.reject(error);
      }
    );
  }

  async createWebhook(name: string = ''): Promise<Webhook | null> {
    try {
      const response = await this.apiClient.post<CreateWebhookResponse>(
        'api/v1/webhook/createwebhook',
        { name }
      );

      const data = response.data;

      return {
        id: data.id,
        url: buildWebhookUrl(data.id)
      };
    } catch (error) {
      return null;
    }
  }

  async getWebhook(id: string): Promise<Webhook | null> {
    try {
      const response = await this.apiClient.get<CreateWebhookResponse>(
        `api/v1/webhook/getwebhook/${id}`
      );
      
      return {
        id: response.data.id,
        url: buildWebhookUrl(response.data.id)
      };
    } catch (error) {
      return null;
    }
  }

  async getWebhookEvents(id: string): Promise<WebHookEvents> {
    try {
      const response = await this.apiClient.get<WebHookEvents>(
        `api/v1/webhook/getwebhook/events/${id}`
      );

      return response.data;
    } catch (error) {
      return [];
    }
  }
}

export const webhookApi = new WebhookApi();
