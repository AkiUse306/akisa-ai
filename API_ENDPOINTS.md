# AKISA-AI New API Endpoints Reference

## Authentication

### Refresh Token
```
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "jwt_token",
  "userId": "user_id"
}

Response 200:
{
  "accessToken": "new_jwt_token",
  "expiresIn": 3600
}
```

### Logout
```
POST /api/auth/logout
Authorization: Bearer <accessToken>

Response 200:
{
  "message": "Logged out successfully."
}
```

---

## Agent Orchestration

### Execute Agent
```
POST /api/orchestration/agents
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "input": "Analyze market trends for Q2",
  "agentType": "analyst"
}

Response 200:
{
  "agentId": "550e8400-e29b-41d4-a716-446655440000",
  "agentType": "analyst",
  "status": "completed",
  "response": "Market analysis results...",
  "executionTimeMs": 2847,
  "context": {...},
  "timestamp": "2026-05-23T10:30:00Z"
}
```

**Agent Types:**
- `analyst` - Data analysis
- `planner` - Workflow planning
- `researcher` - Research synthesis
- `creative` - Ideation
- `developer` - Code/architecture
- `general` - Default

### Execute Tool
```
POST /api/orchestration/tools
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "toolId": "plugin-id",
  "parameters": {
    "param1": "value1",
    "param2": "value2"
  }
}

Response 200:
{
  "agentId": "uuid",
  "agentType": "tool",
  "status": "completed",
  "response": "Tool execution result",
  "executionTimeMs": 150,
  "context": {...},
  "timestamp": "2026-05-23T10:30:00Z"
}
```

---

## Streaming Chat

### Stream Chat Response
```
POST /api/chat/stream
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "prompt": "What is AKISA-AI?",
  "model": "gpt-4",
  "sessionId": "optional_session_id"
}

Response: text/event-stream
data: {"content":"What","delta":"What","finishReason":null}
data: {"content":" is","delta":" is","finishReason":null}
data: {"content":" AKISA","delta":" AKISA","finishReason":null}
...
data: {"content":"?","delta":"?","finishReason":"stop"}
```

**JavaScript Client Example:**
```javascript
const response = await fetch('/api/chat/stream', {
  method: 'POST',
  headers: { 'Authorization': `Bearer ${token}` },
  body: JSON.stringify({ prompt: 'Your question' })
});

const reader = response.body.getReader();
const decoder = new TextDecoder();

while (true) {
  const { done, value } = await reader.read();
  if (done) break;
  
  const chunk = decoder.decode(value);
  const lines = chunk.split('\n');
  
  lines.forEach(line => {
    if (line.startsWith('data: ')) {
      const token = JSON.parse(line.slice(6));
      console.log(token.content);
    }
  });
}
```

---

## Voice

### Text-to-Speech
```
POST /api/voice/text-to-speech
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "text": "Hello world",
  "voice": "nova"
}

Response 200: audio/mp3
[Binary MP3 data]

Available voices:
- nova (default)
- alloy
- echo
- fable
- onyx
- shimmer
```

### Speech-to-Text
```
POST /api/voice/speech-to-text
Authorization: Bearer <accessToken>
Content-Type: multipart/form-data

[Audio file as multipart upload]

Response 200:
{
  "success": true,
  "text": "Transcribed audio content",
  "language": "en"
}
```

---

## Memory

### Search Memory (Enhanced)
```
GET /api/memory/search?query=machine%20learning&limit=5
Authorization: Bearer <accessToken>

Response 200:
[
  {
    "id": "memory_id",
    "content": "Related memory entry",
    "type": "note",
    "timestamp": "2026-05-23T..."
  },
  ...
]
```

---

## Error Responses

### 400 Bad Request
```json
{
  "message": "Description of what was invalid"
}
```

### 401 Unauthorized
```json
{
  "title": "Unauthorized"
}
```

### 500 Internal Server Error
```json
{
  "message": "Error description"
}
```

---

## Rate Limiting & Quotas

Currently none enforced. In production, implement:
- 100 requests/min per user for agent execution
- 10 requests/min for voice endpoints
- 1000 tokens/hour for streaming chat

---

## WebSocket Support (Future)

The `/realtime/ai` SignalR hub is available for real-time bidirectional communication:
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/realtime/ai')
  .withAutomaticReconnect()
  .build();

connection.on('ReceiveMessage', (message) => {
  console.log(message);
});

await connection.start();
await connection.invoke('SendIfReady', 'message');
```

---

## Configuration for Production

Add to `.env`:
```
OpenAI__ApiKey=sk-proj-xxx
Jwt__Secret=your-very-long-secret-key-minimum-32-chars
Jwt__AccessTokenExpiryMinutes=60
Jwt__RefreshTokenExpiryDays=7
Redis__Url=redis-instance.example.com:6379
```

---

## Integration Checklist

- [ ] Update frontend auth module to use `/api/auth/refresh`
- [ ] Implement logout that calls `/api/auth/logout`
- [ ] Add streaming chat UI component
- [ ] Create agent selection UI
- [ ] Add voice upload/playback UI
- [ ] Test all endpoints with real OpenAI API key
- [ ] Configure Redis for your environment
- [ ] Set up error logging and monitoring
