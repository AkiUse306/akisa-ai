# AKISA-AI

AKISA-AI is a next-generation artificial intelligence ecosystem designed to move beyond chatbot experiences into a complete AI operating platform for humans, developers, businesses, creators, and automation workflows.

## Vision

AKISA-AI combines:
- Multi-model AI operating system
- Real-time reasoning engine
- Autonomous agent framework
- Productivity ecosystem
- Knowledge engine
- Developer platform
- Voice + vision assistant
- Cloud AI infrastructure
- Personalized memory system
- AI app marketplace
- Cross-device intelligent companion

## Architecture

Root folders:
- `backend/` — ASP.NET Core 9 API and real-time platform services
- `frontend/` — Next.js, React, TypeScript, Tailwind UI
- `ai-core/` — C++ reasoning and inference core
- `agents/` — autonomous agent orchestrations and toolchains
- `inference-engine/` — GPU inference and model routing
- `vector-memory/` — embedding storage and semantic search
- `desktop-app/` — future desktop companion app
- `mobile-app/` — future mobile AI assistant app
- `plugins/` — plugin system and SDK integration points
- `sdk/` — developer SDKs, API clients, helpers
- `docs/` — architecture, roadmap, integration guides
- `infrastructure/` — cloud deploy assets and infrastructure notes
- `kubernetes/` — Kubernetes manifests and deployment patterns
- `datasets/` — dataset indexing and training pipelines
- `training/` — model fine-tuning pipeline patterns
- `research/` — experiments, design exploration

## Phase 1 Foundation

Initial scope:
- Authentication architecture
- Database architecture and data model
- AI chat and streaming API
- Basic memory and conversation context
- Modern responsive workspace UI

## Phase 2 Support

Implemented foundations for:
- Autonomous agent orchestration
- Memory-aware chat responses
- JWT authentication and session management
- Realtime workspace interaction patterns
- Agent execution endpoints

## Phase 3 Support

Implemented foundations for:
- Multi-modal vision and OCR endpoints
- Document analysis and file upload workflows
- Frontend vision workspace UI
- Inference engine placeholder for GPU model execution

## Getting Started

### Backend

```bash
cd backend
dotnet restore
dotnet run
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

## Next steps

- Build authentication, JWT, refresh tokens
- Add Redis cache and vector database integration
- Create AI chat service with streaming tokens
- Add agent orchestration endpoints
- Implement memory subsystem and semantic search
- Expand multi-modal vision, voice, and tool execution

## OpenAI integration

AKISA-AI can route chat requests to a live OpenAI provider when `OpenAI:ApiKey` is configured in the backend environment.

Example environment variables:

```bash
export OpenAI__ApiKey="your-openai-api-key"
export Jwt__Secret="your-jwt-secret"
```
