import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { webhookApi } from '@/api/WebhooksApi';
import type { Webhook, WebHookEvents } from '@/types';
import { setItem } from '@/utils/localstorage';

export function useCreateWebhook() {
  debugger
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async (name: string) => {
      const result : Webhook | null = await webhookApi.createWebhook(name);
      if (result) {
        setItem('WebhookId', result.id);
      }
      return result;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['webhooks'] });
    },
  });

  return {
    ...mutation,
    createWebhook: mutation.mutateAsync,
  };
}

export function useGetWebhook(id: string) {
  const query = useQuery({
    queryKey: ['webhook', id],
    queryFn: async () => {
      const result = await webhookApi.getWebhook(id);
      debugger;
      return result;
    },
    enabled: !!id,
  });

  return query;
}

export function useGetWebhookEvents(id: string) {
  debugger;
  const query = useQuery({
    queryKey: ['webhook-events', id],
    queryFn: async () => {
      const result = await webhookApi.getWebhookEvents(id);
      return result;
    },
    enabled: !!id,
    refetchInterval: id ? 30000 : false, // Poll every 30 seconds when webhook is selected
    refetchIntervalInBackground: true, // Continue polling even when tab is not active
  });

  return query;
}

export function useWebhooks() {
  const createWebhook = useCreateWebhook();

  return {
    createWebhook: createWebhook.createWebhook,
  };
}
