"use client";

import Link from "next/link";
import { useState } from "react";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

type CompareResult = {
  Akisa: string;
  ChatGpt: string;
  Cursor: string;
  SessionId: string;
};

const features = [
  {
    title: "AI operating platform",
    description: "AKISA-AI is designed as a full OS-style platform with memory, agents, vision, and developer tools.",
  },
  {
    title: "Conversational reasoning",
    description: "ChatGPT is focused on conversation. AKISA-AI extends that with workspace and automation capabilities.",
  },
  {
    title: "Developer assistance",
    description: "Cursor specializes in code navigation and developer workflows; AKISA-AI brings those into an extensible environment.",
  },
  {
    title: "Multi-modal support",
    description: "AKISA-AI supports vision, OCR, and document analysis as part of the platform's Phase 3 rollout.",
  },
  {
    title: "Tool orchestration",
    description: "AKISA-AI plans native tool execution and autonomous agent workflows beyond static text responses.",
  },
];

export default function ComparePage() {
  const [prompt, setPrompt] = useState("");
  const [result, setResult] = useState<CompareResult | null>(null);
  const [status, setStatus] = useState<string | null>(null);

  const comparePrompt = async () => {
    if (!prompt.trim()) {
      return;
    }

    setStatus("Comparing AI responses...");
    setResult(null);

    const response = await fetch(`${apiUrl}/api/compare`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ prompt }),
    });

    setStatus(null);
    if (!response.ok) {
      setStatus("Comparison request failed. Make sure the backend is running and the prompt is valid.");
      return;
    }

    const data = await response.json();
    setResult(data);
  };

  return (
    <main className="min-h-screen bg-slate-950 text-slate-100 p-8">
      <div className="mx-auto max-w-6xl rounded-3xl border border-slate-800 bg-slate-900/90 p-10 shadow-2xl shadow-slate-950/30">
        <div className="mb-8">
          <p className="text-sm uppercase tracking-[0.4em] text-sky-400">AKISA-AI Comparison</p>
          <h1 className="mt-4 text-4xl font-semibold">Live comparison: AKISA-AI vs ChatGPT vs Cursor</h1>
          <p className="mt-4 max-w-3xl text-slate-300">
            Enter a prompt and compare how AKISA-AI responds next to a ChatGPT-style answer and a Cursor-style developer response.
          </p>
        </div>

        <div className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
          <textarea
            value={prompt}
            onChange={(event) => setPrompt(event.target.value)}
            placeholder="Enter the prompt you want to compare across AI assistants..."
            className="min-h-[160px] w-full rounded-3xl border border-slate-700 bg-slate-950 p-5 text-slate-100 outline-none focus:border-sky-500"
          />
          <div className="mt-4 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <button
              onClick={comparePrompt}
              className="rounded-3xl bg-sky-500 px-6 py-4 text-sm font-semibold text-slate-950 transition hover:bg-sky-400"
            >
              Compare now
            </button>
            <p className="text-sm text-slate-400">The compare endpoint returns AKISA, ChatGPT, and Cursor responses side-by-side.</p>
          </div>
          {status ? <p className="mt-4 text-sm text-rose-400">{status}</p> : null}
        </div>

        {result ? (
          <div className="mt-10 space-y-6">
            <div className="rounded-3xl border border-slate-800 bg-slate-950/90 p-6">
              <h2 className="text-2xl font-semibold">AKISA-AI</h2>
              <p className="mt-4 whitespace-pre-line text-slate-200">{result.Akisa}</p>
            </div>
            <div className="rounded-3xl border border-slate-800 bg-slate-950/90 p-6">
              <h2 className="text-2xl font-semibold">ChatGPT</h2>
              <p className="mt-4 whitespace-pre-line text-slate-200">{result.ChatGpt}</p>
            </div>
            <div className="rounded-3xl border border-slate-800 bg-slate-950/90 p-6">
              <h2 className="text-2xl font-semibold">Cursor</h2>
              <p className="mt-4 whitespace-pre-line text-slate-200">{result.Cursor}</p>
            </div>
            <div className="rounded-3xl border border-slate-800 bg-slate-950/90 p-6 text-slate-400">
              <p><strong>Comparison session:</strong> {result.SessionId}</p>
            </div>
          </div>
        ) : null}

        <div className="mt-10 rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
          <h2 className="text-2xl font-semibold">Why this matters</h2>
          <p className="mt-4 text-slate-400">
            AKISA-AI is built to combine conversational reasoning with autonomous agents, memory, and automation. Compare outputs here to see the platform’s value versus general-purpose and developer-focused assistants.
          </p>
          <div className="mt-6 flex flex-wrap gap-3">
            <Link href="/workspace" className="rounded-2xl bg-sky-500 px-5 py-3 text-sm font-semibold text-slate-950 hover:bg-sky-400">Go to workspace</Link>
            <Link href="/vision" className="rounded-2xl border border-slate-700 px-5 py-3 text-sm font-semibold text-slate-100 hover:border-sky-400">Explore vision</Link>
          </div>
        </div>
      </div>
    </main>
  );
}
