"use client";
import { useParams, useRouter } from "next/navigation";
import { useSession } from "next-auth/react";
import { useEffect, useState } from "react";

export default function AcceptInvitePage() {
    const { token } = useParams<{ token: string }>();
    const router = useRouter();
    const { data: session, status } = useSession();
    const [state, setState] = useState<"loading"|"success"|"error">("loading");
    const [errMsg, setErrMsg] = useState("");

    useEffect(() => {
        if (status === "unauthenticated") {
            router.push(`/login?callbackUrl=/invite/${token}`);
            return;
        }

        if (status !== "authenticated") return;

        fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/invites/${token}/accept`, {
            method: "POST",
            headers: { Authorization: `Bearer ${(session as { accessToken?: string }).accessToken}` },
        })
        .then(r => r.json())
        .then(d => {
            if (d.workspaceSlug) {
                setState("success");
                setTimeout(() => router.push(`/${d.workspaceSlug}`), 1200);
            } else {
                setState("error");
                setErrMsg(d.error ?? "Something went wrong");
            }
        })
        .catch(() => setState("error"));
    }, [session, status, token, router]);

    return (
        <main className="min-h-screen flex items-center justify-center bg-gray-50">
            <div className="bg-white p-8 rounded-xl shadow text-center max-w-sm w-full">
                {state === "loading" && <p className="text-gray-500">Accepting invitation...</p>}
                {state === "success" && (
                    <p className="text-green-600 font-medium">Joined! Redirecting...</p>
                )}
                {state === "error" && (
                    <p className="text-red-500">{errMsg || "Invalid or expired invite link."}</p>
                )}
            </div>
        </main>
    );
}
