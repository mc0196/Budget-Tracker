import type { Metadata } from "next";
import { Geist } from "next/font/google";
import "./globals.css";
import { QueryProvider } from "@/components/QueryProvider";
import { Sidebar } from "@/components/Sidebar";
import { BottomNav } from "@/components/BottomNav";

const geist = Geist({ subsets: ["latin"], variable: "--font-geist-sans" });

export const metadata: Metadata = {
  title: "Budget Tracker",
  description: "Personal finance tracking app",
  manifest: "/manifest.json",
  appleWebApp: {
    capable: true,
    statusBarStyle: "default",
    title: "Budget",
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className={`${geist.variable} h-full antialiased`}>
      <body className="h-full bg-gray-50 text-gray-900">
        <QueryProvider>
          <div className="flex h-full">
            <Sidebar />
            <main className="flex-1 overflow-y-auto pb-20 md:pb-0">
              {children}
            </main>
          </div>
          <BottomNav />
        </QueryProvider>
      </body>
    </html>
  );
}
