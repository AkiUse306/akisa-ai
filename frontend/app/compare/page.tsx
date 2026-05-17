import Link from "next/link";

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
  return (
    <main className="min-h-screen bg-slate-950 text-slate-100 p-8">
      <div className="mx-auto max-w-6xl rounded-3xl border border-slate-800 bg-slate-900/90 p-10 shadow-2xl shadow-slate-950/30">
        <div className="mb-8">
          <p className="text-sm uppercase tracking-[0.4em] text-sky-400">AKISA-AI Comparison</p>
          <h1 className="mt-4 text-4xl font-semibold">How AKISA-AI compares with ChatGPT and Cursor</h1>
          <p className="mt-4 max-w-3xl text-slate-300">
            AKISA-AI aims to be a full intelligent ecosystem rather than a single assistant, combining chat, agents, memory, vision, and developer workflows.
          </p>
        </div>

        <div className="grid gap-6 lg:grid-cols-2">
          {features.map((feature) => (
            <article key={feature.title} className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
              <h2 className="text-xl font-semibold text-white">{feature.title}</h2>
              <p className="mt-3 text-slate-400">{feature.description}</p>
            </article>
          ))}
        </div>

        <div className="mt-10 rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
          <h2 className="text-2xl font-semibold">Next step</h2>
          <p className="mt-4 text-slate-400">
            Continue building the AKISA-AI platform by connecting the AI core to real model providers, adding real-time voice and vision pipelines, and enabling autonomous tool execution.
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
