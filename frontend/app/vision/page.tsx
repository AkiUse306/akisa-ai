"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

type ImageAnalysisResult = {
  fileName: string;
  description: string;
  tags: string[];
  objects: string[];
};

type OcrResult = {
  fileName: string;
  language: string;
  text: string;
};

type FileAnalysisResult = {
  fileName: string;
  fileType: string;
  summary: string;
  containsText: boolean;
};

export default function VisionPage() {
  const [token, setToken] = useState<string | null>(null);
  const [file, setFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [status, setStatus] = useState<string | null>(null);
  const [imageAnalysis, setImageAnalysis] = useState<ImageAnalysisResult | null>(null);
  const [ocrResult, setOcrResult] = useState<OcrResult | null>(null);
  const [docAnalysis, setDocAnalysis] = useState<FileAnalysisResult | null>(null);

  useEffect(() => {
    const storedToken = window.localStorage.getItem("akisa_access_token");
    setToken(storedToken);
  }, []);

  useEffect(() => {
    if (!file) {
      setPreviewUrl(null);
      return;
    }

    if (file.type.startsWith("image/")) {
      const url = URL.createObjectURL(file);
      setPreviewUrl(url);
      return () => URL.revokeObjectURL(url);
    }

    setPreviewUrl(null);
  }, [file]);

  const authenticated = useMemo(() => !!token, [token]);

  const upload = async (endpoint: string) => {
    if (!file || !token) {
      return;
    }

    setStatus("Uploading file...");
    const formData = new FormData();
    formData.append("file", file);

    const response = await fetch(`${apiUrl}${endpoint}`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
      },
      body: formData,
    });

    setStatus(null);
    if (!response.ok) {
      setStatus("File processing failed.");
      return;
    }

    if (endpoint === "/api/vision/analyze-image") {
      const data: ImageAnalysisResult = await response.json();
      setImageAnalysis(data);
      setOcrResult(null);
      setDocAnalysis(null);
    } else if (endpoint === "/api/vision/ocr") {
      const data: OcrResult = await response.json();
      setOcrResult(data);
      setImageAnalysis(null);
      setDocAnalysis(null);
    } else {
      const data: FileAnalysisResult = await response.json();
      setDocAnalysis(data);
      setOcrResult(null);
      setImageAnalysis(null);
    }
  };

  if (!authenticated) {
    return (
      <main className="min-h-screen bg-slate-950 text-slate-100 p-6">
        <div className="mx-auto max-w-3xl rounded-3xl border border-slate-800 bg-slate-900/90 p-10 text-center shadow-2xl shadow-slate-950/40">
          <h1 className="text-4xl font-semibold">Vision assistant access required</h1>
          <p className="mt-4 text-slate-400">Login to access multi-modal image, OCR, and document analysis tools.</p>
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
      <div className="mx-auto grid max-w-6xl gap-6 lg:grid-cols-[1.4fr_0.8fr]">
        <section className="rounded-3xl border border-slate-800 bg-slate-900/90 p-6 shadow-2xl shadow-slate-950/30">
          <div className="mb-6 flex items-center justify-between gap-4">
            <div>
              <p className="text-sm uppercase tracking-[0.35em] text-sky-400">Phase 3</p>
              <h1 className="mt-3 text-3xl font-semibold">Multi-modal vision workspace</h1>
            </div>
            <Link href="/workspace" className="rounded-2xl border border-slate-700 px-4 py-2 text-sm text-slate-200 transition hover:border-sky-400">Back to workspace</Link>
          </div>

          <div className="rounded-3xl border border-slate-800 bg-slate-950/80 p-5">
            <label className="block text-sm text-slate-300">
              Select an image or document file
              <input
                type="file"
                accept="image/*,.pdf,.md,.txt"
                className="mt-4 w-full rounded-2xl border border-slate-700 bg-slate-950 p-4 text-slate-100"
                onChange={(event) => setFile(event.target.files?.[0] ?? null)}
              />
            </label>
          </div>

          {previewUrl ? (
            <div className="rounded-3xl border border-slate-800 bg-slate-950/80 p-5">
              <p className="text-sm uppercase tracking-[0.35em] text-slate-400">Preview</p>
              <img src={previewUrl} alt="Preview" className="mt-4 w-full rounded-3xl object-contain" />
            </div>
          ) : null}

          <div className="grid gap-4 sm:grid-cols-3">
            <button
              onClick={() => upload("/api/vision/analyze-image")}
              disabled={!file}
              className="rounded-3xl bg-slate-800 px-5 py-4 text-sm font-semibold text-slate-100 transition hover:bg-slate-700 disabled:opacity-60"
            >
              Analyze image
            </button>
            <button
              onClick={() => upload("/api/vision/ocr")}
              disabled={!file}
              className="rounded-3xl bg-slate-800 px-5 py-4 text-sm font-semibold text-slate-100 transition hover:bg-slate-700 disabled:opacity-60"
            >
              OCR text
            </button>
            <button
              onClick={() => upload("/api/vision/analyze-document")}
              disabled={!file}
              className="rounded-3xl bg-slate-800 px-5 py-4 text-sm font-semibold text-slate-100 transition hover:bg-slate-700 disabled:opacity-60"
            >
              Analyze document
            </button>
          </div>

          {status ? <p className="mt-6 text-sm text-slate-400">{status}</p> : null}

          {imageAnalysis ? (
            <div className="mt-6 rounded-3xl border border-slate-800 bg-slate-950/90 p-6">
              <h2 className="text-xl font-semibold">Vision analysis</h2>
              <p className="mt-3 text-slate-300">{imageAnalysis.description}</p>
              <div className="mt-4 text-sm text-slate-400">
                <p><strong>Tags:</strong> {imageAnalysis.tags.join(", ")}</p>
                <p className="mt-2"><strong>Detected objects:</strong> {imageAnalysis.objects.join(", ")}</p>
              </div>
            </div>
          ) : null}

          {ocrResult ? (
            <div className="mt-6 rounded-3xl border border-slate-800 bg-slate-950/90 p-6">
              <h2 className="text-xl font-semibold">OCR result</h2>
              <p className="mt-3 text-slate-300 whitespace-pre-wrap">{ocrResult.text}</p>
            </div>
          ) : null}

          {docAnalysis ? (
            <div className="mt-6 rounded-3xl border border-slate-800 bg-slate-950/90 p-6">
              <h2 className="text-xl font-semibold">Document analysis</h2>
              <p className="mt-3 text-slate-300">{docAnalysis.summary}</p>
              <p className="mt-4 text-sm text-slate-400"><strong>File type:</strong> {docAnalysis.fileType}</p>
            </div>
          ) : null}
        </section>

        <aside className="rounded-3xl border border-slate-800 bg-slate-900/90 p-6 shadow-2xl shadow-slate-950/30">
          <div className="space-y-4">
            <div>
              <p className="text-sm uppercase tracking-[0.35em] text-sky-400">Multi-modal insights</p>
              <h2 className="mt-3 text-2xl font-semibold">Phase 3 features</h2>
            </div>
            <ul className="space-y-3 text-slate-400">
              <li>• Image understanding and object recognition</li>
              <li>• OCR text extraction from screenshots and documents</li>
              <li>• Document format detection and semantic summarization</li>
              <li>• Multi-modal context for deeper reasoning</li>
            </ul>
          </div>
        </aside>
      </div>
    </main>
  );
}
