import axios from "axios";
import type { AxiosInstance, AxiosRequestConfig } from "axios";
import type {
  Webhook,
  WebHookEvents,
} from "@/types";
import { getEnvOrNull } from "@/utils/env";

class WebhookApi {
  private apiClient: AxiosInstance;

  constructor() {
    this.apiClient = axios.create({
      baseURL: getEnvOrNull('VITE_BASE_URL'),
      timeout: 15000,
      headers: {
        "Content-Type": "application/json",
      },
    });

    this.apiClient.interceptors.request.use(
      (config) => {
        return config;
      },
      (error) => Promise.reject(error)
    );

    this.apiClient.interceptors.response.use(
      (response) => response,
      (error) => {
        return Promise.reject(error);
      }
    );
  }

  async createWebhook(name: string = ""): Promise<Webhook | null> {
    try {
      debugger;
      const response = await this.apiClient.post<Webhook>(
        "api/v1/webhook/createwebhook",
        { name }
      );
      debugger;
      const data = response.data;
      debugger;
      return data;
    } catch (error) {
      return null;
    }
  }

  async getWebhook(id: string): Promise<Webhook | null> {
    try {
      const response = await this.apiClient.get<Webhook>(
        `api/v1/webhook/getwebhook/${id}`
      );
      return response.data;
    } catch (error) {
      return null;
    }
  }

  async getWebhookEvents(id: string): Promise<WebHookEvents> {
    try {
      debugger;
      const response = await this.apiClient.get<WebHookEvents>(`api/v1/webhook/getwebhook/events/${id}`);
      debugger;
      return response.data;
    } catch (error) {
      debugger;
      return [];
    }
  }
}

export const webhookApi = new WebhookApi();

