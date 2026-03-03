import NextAuth from "next-auth";
import Credentials from "next-auth/providers/credentials";
import GitHub from "next-auth/providers/github";
import Google from "next-auth/providers/google";

const API = process.env.NEXT_PUBLIC_API_URL;

export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [
    GitHub,  // reads AUTH_GITHUB_ID + AUTH_GITHUB_SECRET automatically
    Google,  // reads AUTH_GOOGLE_ID + AUTH_GOOGLE_SECRET automatically

    Credentials({
      credentials: {
        email: { label: "Email", type: "email" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials) {
        const res = await fetch(`${API}/api/auth/login`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ email: credentials.email, password: credentials.password }),
        });
        if (!res.ok) return null;
        const data = await res.json();
        return { id: data.userId, email: data.email, name: data.name, accessToken: data.accessToken };
      },
    }),
  ],

  callbacks: {
    // Called on OAuth sign-in - upsert user in .NET backend
    async signIn({ user, account }) {
      if (account?.provider === "github" || account?.provider === "google") {
        const res = await fetch(`${API}/api/auth/oauth`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            provider: account.provider,
            providerKey: account.providerAccountId,
            email: user.email,
            name: user.name,
            avatarUrl: user.image,
          }),
        });
        if (!res.ok) return false;
        const data = await res.json();
        user.id = data.userId;
        (user as { accessToken?: string }).accessToken = data.accessToken;
      }
      return true;
    },
    jwt({ token, user }) {
      if (user) {
        token.userId = user.id;
        token.accessToken = (user as { accessToken?: string }).accessToken;
      }
      return token;
    },
    session({ session, token }) {
      session.user.id = token.userId as string;
      (session as { accessToken?: unknown }).accessToken = token.accessToken;
      return session;
    },
  },
  pages: { signIn: "/login" },
});
