# Kubernetes

Kubernetes deployment manifests for AKISA-AI services.

## Files

- `backend-deployment.yaml` — backend deployment and service.
- `frontend-deployment.yaml` — frontend deployment and service.

## Goals

- Service definitions for backend, frontend, and inference layers
- Redis, Postgres, and vector storage deployment
- Ingress/Routing and load balancing

## Next steps

- Add stateful sets for databases and vector storage.
- Add ingress and TLS configuration.
- Add deployment automation scripts.
