import type { Events } from '@/types';

interface RequestDetailsProps {
  request: Events | null;
}

export default function RequestDetails({ request }: RequestDetailsProps) {
  if (!request) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 h-full">
        <div className="flex flex-col items-center justify-center h-full text-center">
          <div className="w-12 h-12 bg-gradient-to-br from-blue-100 to-purple-100 rounded-full flex items-center justify-center mb-3">
            <svg
              className="w-6 h-6 text-blue-500"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
              />
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
              />
            </svg>
          </div>
          <h3 className="text-base font-medium text-gray-900 mb-1">
            Select a request
          </h3>
          <p className="text-gray-500 text-sm">
            Choose a request from the list to view its details
          </p>
        </div>
      </div>
    );
  }

  const formatDate = (timestamp: string) => {
    const date = new Date(timestamp);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false,
    });
  };

  const copyToClipboard = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text);
    } catch (err) {
      console.error('Failed to copy text:', err);
    }
  };

  const formatJson = (obj: any) => {
    try {
      return JSON.stringify(obj, null, 2);
    } catch {
      return String(obj);
    }
  };

  const getMethodColor = (method: string) => {
    switch (method.toUpperCase()) {
      case 'GET':
        return 'bg-green-500 text-white';
      case 'POST':
        return 'bg-blue-500 text-white';
      case 'PUT':
        return 'bg-yellow-500 text-white';
      case 'DELETE':
        return 'bg-red-500 text-white';
      case 'PATCH':
        return 'bg-purple-500 text-white';
      default:
        return 'bg-gray-500 text-white';
    }
  };

  const getStatusColor = (statusCode: number) => {
    if (statusCode >= 200 && statusCode < 300) {
      return 'bg-green-500 text-white';
    } else if (statusCode >= 400) {
      return 'bg-red-500 text-white';
    } else {
      return 'bg-gray-500 text-white';
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold text-gray-900">Request Details</h2>
        <span className="text-sm text-gray-500 font-mono">
          ID: {request.id || 'N/A'}
        </span>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4">
        <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
          <div>
            <span className="text-xs font-medium text-gray-500 uppercase tracking-wide">
              Method
            </span>
            <div className="mt-3">
              <div
                className={`inline-flex items-center px-2.5 py-1 rounded-full text-sm font-medium ${getMethodColor(
                  request.method
                )}`}
              >
                {request.method}
              </div>
            </div>
          </div>

          <div>
            <span className="text-xs font-medium text-gray-500 uppercase tracking-wide">
              Status
            </span>
            <div className="mt-3">
              <div
                className={`inline-flex items-center px-2.5 py-1 rounded-full text-sm font-medium ${getStatusColor(
                  request.statusCode || 200
                )}`}
              >
                {request.statusCode || 200}
              </div>
            </div>
          </div>

          <div>
            <span className="text-xs font-medium text-gray-500 uppercase tracking-wide">
              Date
            </span>
            <p className="text-sm text-gray-900 font-mono mt-2">
              {request.createdAt ? formatDate(request.createdAt) : 'N/A'}
            </p>
          </div>
        </div>
      </div>

      {request.userAgent && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-base font-medium text-gray-900">User Agent</h3>
            <button
              onClick={() => copyToClipboard(request.userAgent!)}
              className="p-2 text-gray-400 hover:text-gray-600 transition-colors rounded-md hover:bg-gray-100"
              title="Copy User Agent"
            >
              <svg
                className="w-4 h-4"
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
          <div className="bg-gray-50 rounded-lg p-3">
            <code className="text-sm text-gray-700 break-all">
              {request.userAgent}
            </code>
          </div>
        </div>
      )}

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4">
        <div className="flex items-center justify-between mb-3">
          <h3 className="text-base font-medium text-gray-900">
            Headers{' '}
            <span className="text-sm text-gray-500">
              ({Object.keys(request.headers || {}).length})
            </span>
          </h3>
          <button
            onClick={() => copyToClipboard(formatJson(request.headers))}
            className="p-2 text-gray-400 hover:text-gray-600 transition-colors rounded-md hover:bg-gray-100"
            title="Copy Headers"
          >
            <svg
              className="w-4 h-4"
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
        <div className="bg-gray-50 rounded-lg p-3 max-h-48 overflow-y-auto">
          <div className="space-y-2">
            {Object.entries(request.headers || {}).map(([key, value]) => (
              <div key={key} className="flex flex-col space-y-1">
                <span className="text-xs font-medium text-blue-600">
                  {key}:
                </span>
                <span className="text-sm text-gray-700 font-mono pl-4 break-all">
                  {String(value)}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4">
        <div className="flex items-center justify-between mb-3">
          <h3 className="text-base font-medium text-gray-900">
            Raw Request Data
          </h3>
          <button
            onClick={() => copyToClipboard(formatJson(request))}
            className="p-2 text-gray-400 hover:text-gray-600 transition-colors rounded-md hover:bg-gray-100"
            title="Copy Raw Data"
          >
            <svg
              className="w-4 h-4"
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
        <div className="bg-gray-50 rounded-lg p-3 max-h-64 overflow-y-auto">
          <pre className="text-xs text-gray-600 whitespace-pre-wrap break-words">
            {formatJson(request)}
          </pre>
        </div>
      </div>
    </div>
  );
}
