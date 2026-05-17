# Inference Engine

The inference-engine directory contains a small runtime prototype for AKISA-AI multi-modal inference.

## Prototype

- `src/main.cpp` is a command-line sample that simulates inference tasks such as analysis and vision routing.

## Phase 3 goals

- Vision model orchestration and image understanding
- OCR and document analysis
- PDF and image file processing
- Multi-modal model routing and hybrid reasoning

## Build

```bash
mkdir -p build
cd build
cmake ..
cmake --build .
./akama_inference analyze "Summarize the reasoning architecture"
```
