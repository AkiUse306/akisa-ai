"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

export default function RegisterPage() {
  const router = useRouter();
  const [username, setUsername] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    setLoading(true);
    setError(null);

    const res = await fetch(`${apiUrl}/api/auth/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username, password, displayName }),
    });

    setLoading(false);

    if (!res.ok) {
      const data = await res.json();
      setError(data?.message ?? "Registration failed.");
      return;
    }

    const data = await res.json();
    window.localStorage.setItem("akisa_access_token", data.accessToken);
    window.localStorage.setItem("akisa_session_id", data.sessionId);
    router.push("/workspace");
  };

  return (
    <main className="min-h-screen bg-slate-950 text-slate-100 p-6">
      <div className="mx-auto max-w-2xl rounded-3xl border border-slate-800 bg-slate-900/90 p-10 shadow-2xl shadow-slate-950/40">
        <p className="text-sm uppercase tracking-[0.4em] text-sky-400">AKISA-AI</p>
        <h1 className="mt-4 text-4xl font-semibold">Create your AI account</h1>
        <p className="mt-3 text-slate-400">Start building workflows, agents, and long-term memory today.</p>

        <form className="mt-10 space-y-6" onSubmit={submit}>
          <label className="block">
            <span className="text-sm text-slate-300">Username</span>
            <input
              className="mt-2 w-full rounded-2xl border border-slate-700 bg-slate-950 p-4 text-white outline-none focus:border-sky-500"
              value={username}
              onChange={(event) => setUsername(event.target.value)}
              required
            />
          </label>

          <label className="block">
            <span className="text-sm text-slate-300">Display name</span>
            <input
              className="mt-2 w-full rounded-2xl border border-slate-700 bg-slate-950 p-4 text-white outline-none focus:border-sky-500"
              value={displayName}
              onChange={(event) => setDisplayName(event.target.value)}
            />
          </label>

          <label className="block">
            <span className="text-sm text-slate-300">Password</span>
            <input
              type="password"
              className="mt-2 w-full rounded-2xl border border-slate-700 bg-slate-950 p-4 text-white outline-none focus:border-sky-500"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />
          </label>

          {error ? <p className="text-sm text-rose-400">{error}</p> : null}

          <button
            type="submit"
            className="w-full rounded-2xl bg-sky-500 px-6 py-4 text-base font-semibold text-slate-950 transition hover:bg-sky-400 disabled:cursor-not-allowed disabled:opacity-70"
            disabled={loading}
          >
            {loading ? "Creating account..." : "Create account"}
          </button>
        </form>

        <p className="mt-6 text-sm text-slate-400">
          Already registered? <Link href="/login" className="text-sky-400 hover:text-sky-300">Sign in</Link>.
        </p>
      </div>
    </main>
  );
}
