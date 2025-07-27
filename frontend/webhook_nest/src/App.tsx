import { useState, useEffect } from "react";
import { WebhookList, RequestTable, RequestDetails } from "@/components";
import { 
  useCreateWebhook, 
  useGetWebhook,
  useGetWebhookEvents
} from "@/hooks/useWebhooks";
import { getItem, clear } from "@/utils/localstorage";
import { webhookApi } from "@/api/WebhooksApi";
import type { Webhook, WebHookEvents } from "@/types";

function App() {
  const [selectedWebhook, setSelectedWebhook] = useState<Webhook | null>(null);
  const [selectedRequest, setSelectedRequest] = useState<any | null>(null);
  const [createdWebhook, setCreatedWebhook] = useState<Webhook | null>(null);
  const [showError, setShowError] = useState(false);
  const [isLoadingStoredWebhook, setIsLoadingStoredWebhook] = useState(true);

  const createWebhookMutation = useCreateWebhook();
  const { data: webhook, isLoading: isLoadingWebhook } = useGetWebhook(selectedWebhook?.id || '');
  const { data: webhookEvents, isLoading: isLoadingEvents } = useGetWebhookEvents(selectedWebhook?.id || '');

  useEffect(() => {
    const loadStoredWebhook = async () => {
      try {
        debugger;
        const storedWebhookId = getItem<string>('WebhookId');
        if (storedWebhookId) {
          const storedWebhook = await webhookApi.getWebhook(storedWebhookId);
          if (storedWebhook) {
            setSelectedWebhook(storedWebhook);
            setCreatedWebhook(storedWebhook);
          }
        }
      } catch (error) {
        console.error("Error loading stored webhook:", error);
      } finally {
        setIsLoadingStoredWebhook(false);
      }
    };

    loadStoredWebhook();
  }, []);

  const handleCreateWebhook = async () => {
    debugger;
    try {
      const result = await createWebhookMutation.mutateAsync('New Webhook');
      if (result) {
        setCreatedWebhook(result);
        setSelectedWebhook(result);
        setSelectedRequest(null);
        setShowError(false);
      } else {
        setShowError(true);
      }
    } catch (error) {
      console.error("Error creating webhook:", error);
      setShowError(true);
    }
  };

  const handleClearWebhook = () => {
    clear();
    setSelectedWebhook(null);
    setSelectedRequest(null);
    setCreatedWebhook(null);
    setShowError(false);
  };

  const handleSelectWebhook = (webhook: Webhook) => {
    setSelectedWebhook(webhook);
    setSelectedRequest(null);
  };

  const handleSelectRequest = (request: any) => {
    setSelectedRequest(request);
  };

  const handleDeleteWebhook = async (webhookId: string) => {
    try {
      if (selectedWebhook?.id === webhookId) {
        setSelectedWebhook(null);
        setSelectedRequest(null);
      }
    } catch (error) {
      console.error('Error deleting webhook:', error);
    }
  };

  const isCreating = createWebhookMutation.isPending;

  if (isLoadingStoredWebhook) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin w-8 h-8 border-2 border-blue-500 border-t-transparent rounded-full mx-auto mb-4"></div>
          <p className="text-gray-500">Loading webhook...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-full mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="bg-gradient-to-r from-blue-600 to-purple-600 p-2 rounded-lg">
                <svg
                  className="w-8 h-8 text-white"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M13 10V3L4 14h7v7l9-11h-7z"
                  />
                </svg>
              </div>
              <div>
                <h1 className="text-3xl font-bold bg-gradient-to-r from-gray-900 via-blue-800 to-purple-800 bg-clip-text text-transparent">
                  Webhook Nest
                </h1>
                <p className="text-gray-600 text-sm">
                  Monitor webhook endpoints in real-time
                </p>
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-2 text-sm text-gray-500">
                <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
                <span>Live monitoring</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="flex h-[calc(100vh-80px)]">
        <div className="w-80 bg-white border-r border-gray-200 flex flex-col">
          <div className="p-6 border-b border-gray-200 flex-shrink-0">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">
              Create Webhook
            </h2>
            <div className="space-y-3">
              <button
                onClick={handleCreateWebhook}
                disabled={isCreating}
                className="w-full bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 disabled:from-blue-300 disabled:to-purple-300 text-white font-semibold py-3 px-4 rounded-lg shadow-lg transition-all duration-300 transform hover:scale-105 flex items-center justify-center space-x-2"
              >
                <svg
                  className="w-5 h-5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                  />
                </svg>
                <span>{isCreating ? "Creating..." : "Create New Webhook"}</span>
              </button>
              
              <button
                onClick={handleClearWebhook}
                className="w-full bg-gray-500 hover:bg-gray-600 text-white font-semibold py-3 px-4 rounded-lg shadow-lg transition-all duration-300 transform hover:scale-105 flex items-center justify-center space-x-2"
              >
                <svg
                  className="w-5 h-5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                  />
                </svg>
                <span>Clear Webhook & Data</span>
              </button>
            </div>
            
            {createWebhookMutation.isError && (
              <div className="mt-2 text-sm text-red-600 bg-red-50 p-2 rounded">
                Failed to create webhook. Please try again.
              </div>
            )}

            {createdWebhook && (
              <div className="mt-4 p-4 bg-green-50 border border-green-200 rounded-lg">
                <h3 className="text-sm font-semibold text-green-800 mb-2">
                  Webhook Created Successfully!
                </h3>
                <div className="space-y-2">
                  <div className="text-xs text-green-700">
                    <strong>ID:</strong> {createdWebhook.id}
                  </div>
                  <div className="text-xs text-green-700">
                    <strong>URL:</strong> 
                    <div className="mt-1 p-2 bg-white border border-green-300 rounded text-xs font-mono break-all">
                      {createdWebhook.url}
                    </div>
                  </div>
                  <button
                    onClick={() => navigator.clipboard.writeText(createdWebhook.url)}
                    className="mt-2 w-full bg-green-600 hover:bg-green-700 text-white text-xs py-1 px-2 rounded"
                  >
                    Copy URL
                  </button>
                </div>
              </div>
            )}
          </div>

          <div className="flex-1 p-6 overflow-y-auto">
            {isLoadingWebhook ? (
              <div className="flex items-center justify-center py-8">
                <div className="animate-spin w-6 h-6 border-2 border-blue-500 border-t-transparent rounded-full"></div>
                <span className="ml-2 text-gray-500">Loading webhooks...</span>
              </div>
            ) : (
              <WebhookList
                webhooks={selectedWebhook ? [selectedWebhook] : []}
                selectedWebhook={selectedWebhook}
                onSelectWebhook={handleSelectWebhook}
                onDeleteWebhook={handleDeleteWebhook}
              />
            )}
          </div>
        </div>

        <div className="flex-1 flex">
          <div className="w-1/3 p-6">
            {selectedWebhook ? (
              <RequestTable
                requests={webhookEvents || []}
                selectedRequest={selectedRequest}
                onSelectRequest={handleSelectRequest}
                isLoading={isLoadingEvents}
              />
            ) : (
              <div className="bg-white rounded-xl shadow-sm border border-gray-200 h-full flex items-center justify-center">
                <div className="text-center">
                  <div className="w-20 h-20 bg-gradient-to-br from-blue-100 to-purple-100 rounded-full flex items-center justify-center mx-auto mb-6">
                    <svg
                      className="w-10 h-10 text-blue-500"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M13 10V3L4 14h7v7l9-11h-7z"
                      />
                    </svg>
                  </div>
                  <h3 className="text-xl font-semibold text-gray-900 mb-3">
                    Select a webhook
                  </h3>
                  <p className="text-gray-500 text-lg max-w-md mx-auto">
                    Choose a webhook from the sidebar to view its incoming
                    requests and monitor activity in real-time.
                  </p>
                </div>
              </div>
            )}
          </div>

          <div className="w-2/3 border-l border-gray-200 bg-white">
            <div className="h-full overflow-y-auto p-6">
              <RequestDetails request={selectedRequest} />
            </div>
          </div>
        </div>
      </div>

      {!selectedWebhook && !isCreating && (
        <div className="fixed inset-0 bg-gray-50 flex items-center justify-center z-20">
          <div className="text-center max-w-2xl mx-auto p-8">
            <div className="w-24 h-24 bg-gradient-to-br from-blue-100 to-purple-100 rounded-full flex items-center justify-center mx-auto mb-6">
              <svg
                className="w-12 h-12 text-blue-500"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M13 10V3L4 14h7v7l9-11h-7z"
                />
              </svg>
            </div>
            <h3 className="text-3xl font-bold text-gray-900 mb-4">
              Welcome to Webhook Nest
            </h3>
            <p className="text-gray-600 text-lg mb-8">
              Create your first webhook endpoint to start monitoring and
              debugging HTTP requests in real-time. Perfect for testing
              integrations, webhooks, and API calls.
            </p>
            <button
              onClick={handleCreateWebhook}
              disabled={isCreating}
              className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 disabled:from-blue-300 disabled:to-purple-300 text-white font-semibold px-8 py-4 rounded-lg shadow-lg transition-all duration-300 transform hover:scale-105 text-lg"
            >
              {isCreating ? "Creating..." : "Create Your First Webhook"}
            </button>
          </div>
        </div>
      )}

      {showError && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md mx-4">
            <div className="flex items-center mb-4">
              <div className="w-10 h-10 bg-red-100 rounded-full flex items-center justify-center mr-3">
                <svg
                  className="w-6 h-6 text-red-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"
                  />
                </svg>
              </div>
              <h3 className="text-lg font-semibold text-gray-900">
                Unable to Create Webhook
              </h3>
            </div>
            <p className="text-gray-600 mb-6">
              There was an error creating your webhook. Please try again or check your connection.
            </p>
            <div className="flex space-x-3">
              <button
                onClick={() => setShowError(false)}
                className="flex-1 bg-gray-200 hover:bg-gray-300 text-gray-800 font-semibold py-2 px-4 rounded-lg transition-colors"
              >
                Close
              </button>
              <button
                onClick={() => {
                  setShowError(false);
                  handleCreateWebhook();
                }}
                className="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-lg transition-colors"
              >
                Try Again
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
