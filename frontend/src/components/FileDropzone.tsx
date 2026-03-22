"use client";

import { useRef, useState, type DragEvent, type ChangeEvent } from "react";

interface Props {
  onFile: (file: File) => void;
  disabled?: boolean;
}

const ACCEPTED = [".csv", ".xls", ".xlsx"];

export function FileDropzone({ onFile, disabled }: Props) {
  const [dragging, setDragging] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  function handleDrop(e: DragEvent<HTMLDivElement>) {
    e.preventDefault();
    setDragging(false);
    const file = e.dataTransfer.files[0];
    if (file) onFile(file);
  }

  function handleChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file) onFile(file);
    e.target.value = "";
  }

  return (
    <div
      onClick={() => !disabled && inputRef.current?.click()}
      onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
      onDragLeave={() => setDragging(false)}
      onDrop={handleDrop}
      className={`flex flex-col items-center justify-center gap-3 rounded-2xl border-2 border-dashed p-12 cursor-pointer transition-colors select-none ${
        disabled
          ? "border-gray-200 bg-gray-50 cursor-not-allowed opacity-50"
          : dragging
          ? "border-blue-500 bg-blue-50"
          : "border-gray-300 bg-white hover:border-blue-400 hover:bg-blue-50/40"
      }`}
    >
      <span className="text-4xl">📂</span>
      <p className="text-sm font-medium text-gray-600">
        Drag &amp; drop your bank statement here
      </p>
      <p className="text-xs text-gray-400">CSV, XLS, XLSX — up to 10 MB</p>
      <input
        ref={inputRef}
        type="file"
        accept={ACCEPTED.join(",")}
        className="hidden"
        onChange={handleChange}
        disabled={disabled}
      />
    </div>
  );
}
