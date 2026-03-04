"use client";
import { useRouter } from "next/navigation";
import { useSession } from "next-auth/react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

const schema = z.object({
  name: z.string().min(2, "At least 2 characters").max(50),
});
type Form = z.infer<typeof schema>;

export default function CreateWorkspacePage() {
  const router = useRouter();
  const { data: session } = useSession();
  const { register, handleSubmit, formState: { errors, isSubmitting } }
    = useForm<Form>({ resolver: zodResolver(schema) });

  const onSubmit = async (data: Form) => {
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/api/workspaces`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${(session as { accessToken?: string })?.accessToken}`,
        },
        body: JSON.stringify(data),
      }
    );
    if (res.ok) {
      const ws = await res.json();
      router.push(`/${ws.slug}`);
    }
  };

  return (
    <main className="min-h-screen flex items-center justify-center bg-gray-50">
      <form onSubmit={handleSubmit(onSubmit)}
        className="bg-white p-8 rounded-xl shadow w-full max-w-sm space-y-4">
        <h1 className="text-2xl font-bold">Create your workspace</h1>
        <p className="text-sm text-gray-500">
          A workspace is your team&apos;s shared space for projects and tasks.
        </p>
        <div>
          <input {...register("name")} placeholder="e.g. Acme Corp"
            className="w-full border rounded-lg px-4 py-2 text-sm focus:ring-2 focus:ring-blue-500 outline-none" />
          {errors.name && (
            <p className="text-red-500 text-xs mt-1">{errors.name.message}</p>
          )}
        </div>
        <button type="submit" disabled={isSubmitting}
          className="w-full bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
          {isSubmitting ? "Creating..." : "Create workspace"}
        </button>
      </form>
    </main>
  );
}
