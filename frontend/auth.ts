import NextAuth from "next-auth";
import Credentials from "next-auth/providers/credentials";

export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [
    Credentials({
      credentials: {
        email: { label: "Email", type: "email" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials) {
        const res = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL}/api/auth/login`,
          {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              email: credentials.email,
              password: credentials.password,
            }),
          }
        );

        if (!res.ok) return null;

        const data = await res.json();
        // Return user object - stored in the session token
        return {
          id: data.email,
          email: data.email,
          name: data.name,
          accessToken: data.accessToken,
        };
      },
    }),
  ],
  callbacks: {
    jwt({ token, user }) {
      if (user) token.accessToken = (user as { accessToken?: string }).accessToken;
      return token;
    },
    session({ session, token }) {
      if (session) {
        Object.assign(session, { accessToken: token.accessToken });
      }
      return session;
    },
  },
  pages: { signIn: "/login" },
});
