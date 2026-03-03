import { auth } from "@/auth";
import { redirect } from "next/navigation";

export default async function DashboardPage() {
  const session = await auth();
  if (!session) redirect("/login");

  return (
    <main className="min-h-screen bg-gray-50 p-8">
      <h1 className="text-3xl font-bold text-gray-900">
        Welcome, {session.user?.name}
      </h1>
      <p className="text-gray-500 mt-2">Sprint 1 complete. Dashboard coming in Sprint 3.</p>
    </main>
  );
}
