export type ChatRequest = {
  prompt: string;
  model?: string;
  sessionId?: string;
};

export type ChatResponse = {
  requestId: string;
  message: string;
  model: string;
  sessionId: string;
};

export type AgentDescriptor = {
  id: string;
  name: string;
  description: string;
};

export type AgentResult = {
  agentId: string;
  output: string;
  success: boolean;
};

export type PluginManifest = {
  id: string;
  displayName: string;
  version: string;
  description: string;
  actions: Array<{ id: string; name: string; description: string }>;
};

export type PluginExecutionResult = {
  pluginId: string;
  output: string;
  success: boolean;
};

export const getApiUrl = () => {
  return typeof window !== 'undefined'
    ? window.location.origin
    : 'http://localhost:5000';
};

async function fetchJson<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`${getApiUrl()}${endpoint}`, options);
  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || 'AKISA request failed.');
  }
  return response.json();
}

export async function sendChat(payload: ChatRequest, token: string): Promise<ChatResponse> {
  return fetchJson<ChatResponse>(`/api/chat`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(payload),
  });
}

export async function login(username: string, password: string) {
  return fetchJson<{ accessToken: string; sessionId: string }>('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  });
}

export async function register(username: string, password: string, displayName?: string) {
  return fetchJson<{ accessToken: string; sessionId: string }>('/api/auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password, displayName }),
  });
}

export async function getAgents(token: string): Promise<AgentDescriptor[]> {
  return fetchJson<AgentDescriptor[]>('/api/agents', {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function runAgent(agentId: string, input: string, token: string): Promise<AgentResult> {
  return fetchJson<AgentResult>(`/api/agents/${agentId}/execute`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ input }),
  });
}

export async function getPlugins(token: string): Promise<PluginManifest[]> {
  return fetchJson<PluginManifest[]>('/api/plugins', {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function executePlugin(pluginId: string, input: string, token: string): Promise<PluginExecutionResult> {
  return fetchJson<PluginExecutionResult>(`/api/plugins/${pluginId}/execute`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ input }),
  });
}

export async function uploadVisionFile(endpoint: string, file: File, token: string) {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(`${getApiUrl()}${endpoint}`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}` },
    body: formData,
  });

  if (!response.ok) {
    throw new Error(await response.text());
  }

  return response.json();
}

export async function searchMemory(query: string, token: string) {
  return fetchJson<any>(`/api/memory/search?query=${encodeURIComponent(query)}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}
