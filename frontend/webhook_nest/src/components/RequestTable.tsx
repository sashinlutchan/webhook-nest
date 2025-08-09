import type { Events } from '@/types';

interface RequestTableProps {
  requests: Events[];
  selectedRequest: Events | null;
  onSelectRequest: (request: Events) => void;
  isLoading?: boolean;
}

export default function RequestTable({
  requests,
  selectedRequest,
  onSelectRequest,
  isLoading = false,
}: RequestTableProps) {
  const formatCreatedDate = (createdAt: string) => {
    if (!createdAt) return 'N/A';
    try {
      const date = new Date(createdAt);
      return date.toLocaleString('en-US', {
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false,
      });
    } catch {
      return createdAt;
    }
  };

  const getMethodColor = (method: string) => {
    switch (method.toUpperCase()) {
      case 'GET':
        return 'bg-green-100 text-green-800 border-green-200';
      case 'POST':
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'PUT':
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'DELETE':
        return 'bg-red-100 text-red-800 border-red-200';
      case 'PATCH':
        return 'bg-purple-100 text-purple-800 border-purple-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  const getStatusColor = (statusCode: number) => {
    if (statusCode >= 200 && statusCode < 300) {
      return 'bg-green-100 text-green-800';
    } else if (statusCode >= 400) {
      return 'bg-red-100 text-red-800';
    } else {
      return 'bg-gray-100 text-gray-800';
    }
  };

  const truncateText = (text: string, maxLength: number = 25) => {
    return text.length > maxLength
      ? text.substring(0, maxLength) + '...'
      : text;
  };

  const formatBodyPreview = (data: any) => {
    if (!data) return null;
    try {
      const dataStr = typeof data === 'string' ? data : JSON.stringify(data);
      return truncateText(dataStr, 40);
    } catch {
      return truncateText(String(data), 40);
    }
  };

  const sortedRequests = [...requests].sort((a, b) => {
    if (!a.createdAt && !b.createdAt) return 0;
    if (!a.createdAt) return 1;
    if (!b.createdAt) return -1;
    return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
  });

  if (isLoading) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 h-full">
        <div className="p-3 border-b border-gray-200">
          <div className="h-4 bg-gray-200 rounded w-1/3 animate-pulse"></div>
        </div>
        <div className="p-3">
          <div className="animate-pulse">
            <div className="space-y-2">
              {[...Array(6)].map((_, i) => (
                <div key={i} className="grid grid-cols-4 gap-2">
                  <div className="h-3 bg-gray-200 rounded"></div>
                  <div className="h-3 bg-gray-200 rounded"></div>
                  <div className="h-3 bg-gray-200 rounded"></div>
                  <div className="h-3 bg-gray-200 rounded"></div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 h-full flex flex-col">
      <div className="p-3 border-b border-gray-200 flex-shrink-0">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-semibold text-gray-900">
            Webhook Requests
          </h2>
          <div className="flex items-center space-x-2">
            <div className="flex items-center space-x-1">
              <div className="w-1.5 h-1.5 bg-green-500 rounded-full animate-pulse"></div>
              <span className="text-xs text-gray-500">Live</span>
            </div>
            {sortedRequests.length > 0 && (
              <span className="bg-blue-100 text-blue-800 text-xs font-medium px-2 py-0.5 rounded-full">
                {sortedRequests.length}
              </span>
            )}
          </div>
        </div>
      </div>

      <div className="flex-1 overflow-auto">
        {sortedRequests.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-full p-4">
            <div className="w-8 h-8 bg-gray-100 rounded-full flex items-center justify-center mb-2">
              <svg
                className="w-4 h-4 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                />
              </svg>
            </div>
            <h3 className="text-sm font-medium text-gray-900 mb-1">
              No requests yet
            </h3>
            <p className="text-gray-500 text-xs text-center">
              Send requests to your webhook endpoints
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50 sticky top-0">
                <tr>
                  <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Method
                  </th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Date
                  </th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Body
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {sortedRequests.map((request, index) => (
                  <tr
                    key={request.id || index}
                    onClick={() => onSelectRequest(request)}
                    className={`cursor-pointer transition-colors hover:bg-gray-50 ${
                      selectedRequest?.id === request.id
                        ? 'bg-blue-50 border-l-4 border-blue-500'
                        : ''
                    }`}
                  >
                    <td className="px-3 py-2 whitespace-nowrap">
                      <span
                        className={`inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium border ${getMethodColor(
                          request.method
                        )}`}
                      >
                        {request.method}
                      </span>
                    </td>
                    <td className="px-3 py-2 whitespace-nowrap">
                      <span
                        className={`inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium ${getStatusColor(
                          request.statusCode || 200
                        )}`}
                      >
                        {request.statusCode || 200}
                      </span>
                    </td>
                    <td className="px-3 py-2 whitespace-nowrap text-xs text-gray-900 font-mono">
                      {formatCreatedDate(request.createdAt || '')}
                    </td>
                    <td className="px-3 py-2 text-xs text-gray-600 max-w-xs">
                      {request.data ? (
                        <div
                          className="truncate bg-gray-100 px-1.5 py-0.5 rounded text-xs font-mono"
                          title={JSON.stringify(request.data)}
                        >
                          {formatBodyPreview(request.data)}
                        </div>
                      ) : (
                        <span className="text-gray-400 text-xs">No data</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
