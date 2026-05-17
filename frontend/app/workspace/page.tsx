"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

type Message = {
  role: "user" | "assistant";
  text: string;
  timestamp: string;
};

type AgentDescriptor = {
  id: string;
  name: string;
  description: string;
};

type AgentResult = {
  agentId: string;
  output: string;
  success: boolean;
};

export default function WorkspacePage() {
  const [token, setToken] = useState<string | null>(null);
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [prompt, setPrompt] = useState("");
  const [messages, setMessages] = useState<Message[]>([]);
  const [agents, setAgents] = useState<AgentDescriptor[]>([]);
  const [agentOutput, setAgentOutput] = useState<string | null>(null);
  const [selectedAgent, setSelectedAgent] = useState<string>("");
  const [status, setStatus] = useState<string | null>(null);

  useEffect(() => {
    const storedToken = window.localStorage.getItem("akisa_access_token");
    const storedSession = window.localStorage.getItem("akisa_session_id");
    setToken(storedToken);
    setSessionId(storedSession);
  }, []);

  useEffect(() => {
    if (!token) {
      return;
    }

    fetch(`${apiUrl}/api/agents`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then((res) => res.json())
      .then((data) => setAgents(data))
      .catch(() => setAgents([]));
  }, [token]);

  const authenticated = useMemo(() => !!token, [token]);

  const sendMessage = async () => {
    if (!prompt.trim() || !token) {
      return;
    }

    setStatus("Sending message...");
    const response = await fetch(`${apiUrl}/api/chat`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ prompt, sessionId }),
    });

    setStatus(null);
    if (!response.ok) {
      setStatus("Unable to send message. Please login again.");
      return;
    }

    const data = await response.json();
    setSessionId(data.sessionId);
    window.localStorage.setItem("akisa_session_id", data.sessionId);
    setMessages((current) => [
      ...current,
      { role: "user", text: prompt, timestamp: new Date().toISOString() },
      { role: "assistant", text: data.message, timestamp: new Date().toISOString() },
    ]);
    setPrompt("");
  };

  const executeAgent = async () => {
    if (!selectedAgent || !prompt.trim() || !token) {
      return;
    }

    setStatus("Running agent...");
    const response = await fetch(`${apiUrl}/api/agents/${selectedAgent}/execute`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ input: prompt }),
    });

    setStatus(null);
    if (!response.ok) {
      setAgentOutput("Agent execution failed.");
      return;
    }

    const data: AgentResult = await response.json();
    setAgentOutput(data.output);
  };

  if (!authenticated) {
    return (
      <main className="min-h-screen bg-slate-950 text-slate-100 p-6">
        <div className="mx-auto max-w-3xl rounded-3xl border border-slate-800 bg-slate-900/90 p-10 text-center shadow-2xl shadow-slate-950/40">
          <h1 className="text-4xl font-semibold">Workspace access required</h1>
          <p className="mt-4 text-slate-400">Sign in or register to launch the AKISA workspace experience.</p>
          <div className="mt-8 flex justify-center gap-4">
            <Link href="/login" className="rounded-2xl bg-sky-500 px-6 py-3 text-sm font-semibold text-slate-950 hover:bg-sky-400">Login</Link>
            <Link href="/register" className="rounded-2xl border border-slate-700 px-6 py-3 text-sm font-semibold text-slate-100 hover:border-sky-400">Register</Link>
          </div>
        </div>
      </main>
    );
  }

  return (
    <main className="min-h-screen bg-slate-950 text-slate-100 p-6">
      <div className="mx-auto grid max-w-6xl gap-6 lg:grid-cols-[1.7fr_1fr]">
        <section className="rounded-3xl border border-slate-800 bg-slate-900/90 p-6 shadow-2xl shadow-slate-950/30">
          <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-sm uppercase tracking-[0.35em] text-sky-400">AKISA Workspace</p>
              <h1 className="mt-3 text-3xl font-semibold">Real-time reasoning hub</h1>
            </div>
            <div className="flex flex-wrap items-center gap-3">
              <span className="rounded-full bg-slate-800 px-4 py-2 text-xs uppercase tracking-[0.3em] text-slate-300">Live</span>
              <Link href="/vision" className="rounded-2xl border border-slate-700 bg-slate-950 px-4 py-2 text-sm text-slate-100 transition hover:border-sky-400">
                Open Vision Hub
              </Link>
            </div>
          </div>

          <div className="mb-5 rounded-3xl border border-slate-800 bg-slate-950/80 p-5">
            <div className="mb-4 flex items-center justify-between gap-4">
              <div>
                <p className="text-sm text-slate-400">Message thread</p>
                <p className="text-xs text-slate-500">AI responses include memory, planning, and agent context.</p>
              </div>
              <span className="text-xs uppercase tracking-[0.3em] text-slate-500">Session {sessionId ? sessionId.slice(0, 8) : "new"}</span>
            </div>
            <div className="space-y-4">
              {messages.length === 0 ? (
                <p className="text-slate-500">Start the conversation with your first prompt.</p>
              ) : (
                messages.map((message, index) => (
                  <div key={index} className={message.role === "user" ? "rounded-3xl bg-slate-900 p-4 text-slate-100" : "rounded-3xl bg-slate-800 p-4 text-slate-100"}>
                    <div className="text-xs text-slate-500">{message.role}</div>
                    <p className="mt-2 whitespace-pre-line">{message.text}</p>
                  </div>
                ))
              )}
            </div>
          </div>

          <div className="space-y-4">
            <textarea
              value={prompt}
              onChange={(event) => setPrompt(event.target.value)}
              placeholder="Ask AKISA to plan, analyze, automate, or code..."
              className="min-h-[140px] w-full rounded-3xl border border-slate-800 bg-slate-950 p-4 text-slate-100 outline-none focus:border-sky-500"
            />
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <button
                onClick={sendMessage}
                className="rounded-3xl bg-sky-500 px-6 py-4 text-sm font-semibold text-slate-950 transition hover:bg-sky-400"
              >
                Send to AI
              </button>
              <p className="text-sm text-slate-500">{status ?? "Use the chat to interact with the reasoning engine."}</p>
            </div>
          </div>
        </section>

        <aside className="rounded-3xl border border-slate-800 bg-slate-900/90 p-6 shadow-2xl shadow-slate-950/30">
          <div className="mb-6">
            <p className="text-sm uppercase tracking-[0.35em] text-sky-400">Agents</p>
            <h2 className="mt-3 text-2xl font-semibold">Autonomous helpers</h2>
            <p className="mt-2 text-slate-400">Launch a specialized AI agent for coding, research, automation, business, or creativity.</p>
          </div>

          <div className="space-y-4">
            <select
              value={selectedAgent}
              onChange={(event) => setSelectedAgent(event.target.value)}
              className="w-full rounded-3xl border border-slate-700 bg-slate-950 p-4 text-slate-100 outline-none focus:border-sky-500"
            >
              <option value="">Select an agent</option>
              {agents.map((agent) => (
                <option key={agent.id} value={agent.id}>{agent.name}</option>
              ))}
            </select>

            <button
              onClick={executeAgent}
              className="w-full rounded-3xl bg-slate-800 px-6 py-4 text-sm font-semibold text-slate-100 transition hover:bg-slate-700 disabled:opacity-70"
            >
              Run agent with current prompt
            </button>

            {agentOutput ? (
              <div className="rounded-3xl border border-slate-800 bg-slate-950 p-4 text-slate-200">
                <p className="text-sm uppercase tracking-[0.35em] text-slate-500">Agent output</p>
                <p className="mt-3 whitespace-pre-line">{agentOutput}</p>
              </div>
            ) : null}
          </div>

          <div className="mt-8 rounded-3xl border border-slate-800 bg-slate-950/80 p-5">
            <p className="text-sm uppercase tracking-[0.35em] text-slate-400">Live context</p>
            <p className="mt-3 text-slate-400">Agents and chat share the same workspace session and memory. This is your autonomous AI command center.</p>
          </div>
        </aside>
      </div>
    </main>
  );
}
