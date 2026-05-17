import Link from "next/link";

export default function Home() {
  return (
    <main className="min-h-screen bg-slate-950 text-slate-100 p-8">
      <section className="mx-auto max-w-5xl rounded-3xl border border-slate-800 bg-slate-900/80 p-10 shadow-2xl shadow-slate-950/30 backdrop-blur-xl">
        <div className="mb-10 space-y-4">
          <p className="text-sm uppercase tracking-[0.4em] text-sky-400">AKISA-AI</p>
          <h1 className="text-4xl font-semibold sm:text-5xl">A next-generation AI operating platform</h1>
          <p className="max-w-3xl text-slate-300">
            AKISA-AI blends reasoning, memory, agents, voice, vision, and automation into a futuristic workspace for developers, creators, and businesses.
          </p>
        </div>

        <div className="grid gap-6 sm:grid-cols-2">
          <div className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
            <h2 className="text-xl font-semibold text-white">Workspace</h2>
            <p className="mt-3 text-slate-400">Start a live conversation, execute agents, and manage your AI memory.</p>
          </div>
          <div className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
            <h2 className="text-xl font-semibold text-white">Platform</h2>
            <p className="mt-3 text-slate-400">Secure APIs, extensible SDKs, and intelligent automation flows for developers.</p>
          </div>
          <div className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
            <h2 className="text-xl font-semibold text-white">Memory</h2>
            <p className="mt-3 text-slate-400">Semantic context and long-term memory keep the platform personalized and aware.</p>
          </div>
          <div className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
            <h2 className="text-xl font-semibold text-white">Agents</h2>
            <p className="mt-3 text-slate-400">Launch intelligent assistants for code, research, automation, and creative tasks.</p>
          </div>
        </div>

        <div className="mt-10 flex flex-col gap-4 sm:flex-row">
          <Link href="/login" className="rounded-3xl bg-sky-500 px-6 py-4 text-center text-base font-semibold text-slate-950 transition hover:bg-sky-400">
            Launch workspace
          </Link>
          <Link href="/register" className="rounded-3xl border border-slate-700 px-6 py-4 text-center text-base font-semibold text-slate-100 transition hover:border-sky-400">
            Get started
          </Link>
          <Link href="/compare" className="rounded-3xl border border-slate-700 px-6 py-4 text-center text-base font-semibold text-slate-100 transition hover:border-sky-400">
            Compare with ChatGPT & Cursor
          </Link>
        </div>
      </section>
    </main>
  );
}
