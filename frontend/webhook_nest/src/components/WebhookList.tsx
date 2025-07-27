import type { Webhook } from "@/types";

interface WebhookListProps {
  webhooks: Webhook[];
  selectedWebhook: Webhook | null;
  onSelectWebhook: (webhook: Webhook) => void;
  onDeleteWebhook?: (webhookId: string) => void;
}

export default function WebhookList({
  webhooks,
  selectedWebhook,
  onSelectWebhook,
  onDeleteWebhook,
}: WebhookListProps) {
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
    });
  };

  const copyToClipboard = async (url: string) => {
    try {
      await navigator.clipboard.writeText(url);
      // You could add a toast notification here
    } catch (err) {
      console.error("Failed to copy URL:", err);
    }
  };

  if (webhooks.length === 0) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
        <div className="text-center">
          <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-3">
            <svg
              className="w-6 h-6 text-gray-400"
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
          <h3 className="text-md font-medium text-gray-900 mb-1">
            No webhooks yet
          </h3>
          <p className="text-gray-500 text-sm">
            Create your first webhook endpoint
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold text-gray-900">Webhooks</h2>
        <span className="text-sm text-gray-500">{webhooks.length}</span>
      </div>

      <div className="space-y-2 max-h-96 overflow-y-auto">
        {webhooks.map((webhook) => (
          <div
            key={webhook.id}
            onClick={() => onSelectWebhook(webhook)}
            className={`group relative bg-white rounded-lg border-2 transition-all duration-200 cursor-pointer hover:shadow-md ${
              selectedWebhook?.id === webhook.id
                ? "border-blue-500 bg-blue-50 shadow-sm"
                : "border-gray-200 hover:border-blue-300"
            }`}
          >
            <div className="p-3">
              {/* Header */}
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center space-x-2">
                  <div
                    className={`w-2 h-2 rounded-full ${
                      webhook.isActive ? "bg-green-500" : "bg-gray-400"
                    }`}
                  ></div>
                  <h3 className="font-medium text-gray-900 text-sm truncate">
                    {webhook.name || "Untitled Webhook"}
                  </h3>
                </div>
                {onDeleteWebhook && (
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      if (
                        confirm("Are you sure you want to delete this webhook?")
                      ) {
                        onDeleteWebhook(webhook.id);
                      }
                    }}
                    className="opacity-0 group-hover:opacity-100 transition-opacity p-1 text-red-400 hover:text-red-600"
                    title="Delete webhook"
                  >
                    <svg
                      className="w-3 h-3"
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
                  </button>
                )}
              </div>

              {/* URL - Compact version */}
              <div className="mb-2">
                <div className="flex items-center space-x-1 bg-gray-50 rounded p-2">
                  <code className="flex-1 text-xs font-mono text-gray-700 truncate">
                    {webhook.url}
                  </code>
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      copyToClipboard(webhook.url);
                    }}
                    className="p-1 text-gray-400 hover:text-gray-600 transition-colors"
                    title="Copy URL"
                  >
                    <svg
                      className="w-3 h-3"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"
                      />
                    </svg>
                  </button>
                </div>
              </div>

              {/* Compact Stats */}
              <div className="flex items-center justify-between text-xs text-gray-500">
                <span>{webhook.requestCount} requests</span>
                <span>{formatDate(webhook.createdAt)}</span>
              </div>
            </div>

            {selectedWebhook?.id === webhook.id && (
              <div className="absolute inset-0 rounded-lg ring-2 ring-blue-500 ring-opacity-50 pointer-events-none"></div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
