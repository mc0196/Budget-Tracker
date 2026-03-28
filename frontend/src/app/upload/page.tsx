"use client";

import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { importApi } from "@/lib/api";
import { FileDropzone } from "@/components/FileDropzone";
import type { ImportResult } from "@/types";

type UploadState =
  | { status: "idle" }
  | { status: "uploading"; fileName: string }
  | { status: "success"; result: ImportResult }
  | { status: "error"; message: string };

export default function UploadPage() {
  const [state, setState] = useState<UploadState>({ status: "idle" });
  const queryClient = useQueryClient();

  async function handleFile(file: File) {
    setState({ status: "uploading", fileName: file.name });
    try {
      const result = await importApi.upload(file);
      setState({ status: "success", result });
      // Invalidate dashboard + transactions so they refresh with new data
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
    } catch (err) {
      const message = err instanceof Error ? err.message : "Upload failed";
      setState({ status: "error", message });
    }
  }

  return (
    <div className="p-6 max-w-xl mx-auto space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Upload Statement</h1>

      <FileDropzone
        onFile={handleFile}
        disabled={state.status === "uploading"}
      />

      {state.status === "uploading" && (
        <div className="flex items-center gap-3 rounded-xl border border-blue-200 bg-blue-50 p-4 text-sm text-blue-700">
          <span className="animate-spin text-lg">⏳</span>
          Uploading <strong>{state.fileName}</strong>…
        </div>
      )}

      {state.status === "success" && (
        <div className="rounded-xl border border-green-200 bg-green-50 p-4 text-sm text-green-700 space-y-1">
          <p className="font-semibold text-base">✅ Import complete</p>
          <p>
            <strong>{state.result.rowsImported}</strong> transactions imported
            from <strong>{state.result.fileName}</strong>.
          </p>
          <button
            onClick={() => setState({ status: "idle" })}
            className="mt-2 text-xs underline text-green-600 hover:text-green-800"
          >
            Upload another file
          </button>
        </div>
      )}

      {state.status === "error" && (
        <div className="rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700 space-y-1">
          <p className="font-semibold text-base">❌ Upload failed</p>
          <p>{state.message}</p>
          <button
            onClick={() => setState({ status: "idle" })}
            className="mt-2 text-xs underline text-red-600 hover:text-red-800"
          >
            Try again
          </button>
        </div>
      )}
    </div>
  );
}
