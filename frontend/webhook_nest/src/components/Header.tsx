import { useState } from 'react';

interface HeaderProps {
  onCreateWebhook: () => void;
  isCreating: boolean;
}

export default function Header({ onCreateWebhook, isCreating }: HeaderProps) {
  const [webhookName, setWebhookName] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);

  const handleCreate = () => {
    onCreateWebhook();
    setWebhookName('');
    setShowCreateForm(false);
  };

  return (
    <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
      <div className="max-w-7xl mx-auto px-6 py-4">
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
                Create and monitor webhook endpoints in real-time
              </p>
            </div>
          </div>

          <div className="flex items-center space-x-4">
            {showCreateForm && (
              <div className="flex items-center space-x-2 bg-gray-50 rounded-lg p-2">
                <input
                  type="text"
                  placeholder="Webhook name (optional)"
                  value={webhookName}
                  onChange={e => setWebhookName(e.target.value)}
                  className="px-3 py-1 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <button
                  onClick={handleCreate}
                  disabled={isCreating}
                  className="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white px-4 py-1 rounded-md text-sm font-medium transition-colors"
                >
                  {isCreating ? 'Creating...' : 'Create'}
                </button>
                <button
                  onClick={() => setShowCreateForm(false)}
                  className="text-gray-500 hover:text-gray-700 px-2 py-1 text-sm"
                >
                  Cancel
                </button>
              </div>
            )}

            {!showCreateForm && (
              <button
                onClick={() => setShowCreateForm(true)}
                className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white font-semibold px-6 py-2 rounded-lg shadow-lg transition-all duration-300 transform hover:scale-105 flex items-center space-x-2"
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
                <span>Create Webhook</span>
              </button>
            )}

            <div className="flex items-center space-x-2 text-sm text-gray-500">
              <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
              <span>Live monitoring</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
