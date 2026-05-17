# Vector Memory

This module provides a lightweight semantic memory store for AKISA-AI.

## Files

- `VectorMemory.cs` — local embedding store with cosine similarity search.

## Features

- Add and normalize embeddings
- Retrieve stored metadata
- Perform nearest-neighbor search by similarity score

## Goals

- Long-term memory storage
- Semantic retrieval for context and personalization
- Integration with Qdrant or Milvus

## Next steps

- Add embedding generation using OpenAI or local models.
- Add persistent storage adapters.
- Wire vectors into the backend chat and agent pipelines.
