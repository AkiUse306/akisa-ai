# AKISA-AI local helper commands

.PHONY: all backend frontend build run

all: build

backend:
	cd backend && dotnet build

frontend:
	cd frontend && npm install && npm run build

build: backend frontend

run-backend:
	cd backend && dotnet run

run-frontend:
	cd frontend && npm install && npm run dev

run:
	@echo "Start backend and frontend separately:"
	@echo "  make run-backend"
	@echo "  make run-frontend"
