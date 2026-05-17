# Setup

## Local development

1. Copy the environment example:

```bash
cp .env.example .env
```

2. Fill in your secrets in `.env`:

```bash
OPENAI_API_KEY=
JWT_SECRET=your-super-secret-key
NEXT_PUBLIC_API_URL=http://localhost:5000
ASPNETCORE_URLS=http://localhost:5000
```

3. Start the backend and frontend separately:

```bash
cd backend
dotnet restore
dotnet run
```

```bash
cd frontend
npm install
npm run dev
```

4. Open `http://localhost:3000` in your browser.

## Running with Docker

```bash
docker compose -f infrastructure/docker-compose.yml up --build
```

The backend listens on `http://localhost:5000` and the frontend listens on `http://localhost:3000`.
