# AI Core

This directory contains the AKISA-AI reasoning core prototype.

## Purpose

- High-performance inference orchestration
- Multi-model routing
- Context and memory management
- Recursive planning and reflection
- GPU accelerated workloads via CUDA, TensorRT, and ONNX Runtime

## Prototype

- `src/main.cpp` is a small command-line prototype that accepts a prompt and prints a reasoning summary.

## Getting Started

```bash
mkdir -p build
cd build
cmake ..
cmake --build .
./akisa_core "Build an AI reasoning plan"
```
