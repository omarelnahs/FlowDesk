"use client";

import { signIn } from "next-auth/react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";

const schema = z.object({ email: z.string().email(), password: z.string().min(6) });
type FormData = z.infer<typeof schema>;

export default function LoginPage() {
  const router = useRouter();
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = async (data: FormData) => {
    const result = await signIn("credentials", { ...data, redirect: false });
    if (result?.ok) router.push("/dashboard");
    else alert("Invalid email or password");
  };

  return (
    <main className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="bg-white p-8 rounded-xl shadow w-full max-w-sm space-y-4">
        <h1 className="text-2xl font-bold text-gray-900">Sign in to FlowDesk</h1>

        {/* OAuth buttons */}
        <button onClick={() => signIn("github", { callbackUrl: "/dashboard" })}
          className="w-full flex items-center justify-center gap-2 border rounded-lg px-4 py-2 text-sm hover:bg-gray-50">
          <span>□</span> Continue with GitHub
        </button>
        <button onClick={() => signIn("google", { callbackUrl: "/dashboard" })}
          className="w-full flex items-center justify-center gap-2 border rounded-lg px-4 py-2 text-sm hover:bg-gray-50">
          <span>□</span> Continue with Google
        </button>

        <div className="flex items-center gap-2 text-xs text-gray-400">
          <div className="flex-1 border-t"/><span>or</span><div className="flex-1 border-t"/>
        </div>

        {/* Email/password form */}
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-3">
          <div>
            <input {...register("email")} type="email" placeholder="Email"
              className="w-full border rounded-lg px-4 py-2 text-sm focus:ring-2 focus:ring-blue-500 outline-none" />
            {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email.message}</p>}
          </div>
          <div>
            <input {...register("password")} type="password" placeholder="Password"
              className="w-full border rounded-lg px-4 py-2 text-sm focus:ring-2 focus:ring-blue-500 outline-none" />
            {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password.message}</p>}
          </div>
          <button type="submit" disabled={isSubmitting}
            className="w-full bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
            {isSubmitting ? "Signing in..." : "Sign in"}
          </button>
        </form>
      </div>
    </main>
  );
}
