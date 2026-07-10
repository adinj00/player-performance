import { useQuery } from "@tanstack/react-query";

import { getSession } from "@/features/auth/api/auth-api";
import type {
  SessionResponse,
  SessionUser,
} from "@/features/auth/types/session";

export const sessionQueryKey = ["auth", "session"] as const;

export interface UseSessionResult {
  session: SessionResponse | undefined;
  user: SessionUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  isUnauthenticated: boolean;
  isError: boolean;
  error: unknown;
  refetchSession: () => Promise<unknown>;
}

export function useSession(): UseSessionResult {
  const query = useQuery({
    queryKey: sessionQueryKey,
    queryFn: getSession,
    retry: false,
  });

  const session = query.data;
  const isAuthenticated =
    session?.isAuthenticated === true && session.user !== null;

  return {
    session,
    user: session?.user ?? null,
    isLoading: query.isLoading,
    isAuthenticated,
    isUnauthenticated:
      query.isSuccess &&
      (session?.isAuthenticated !== true || session.user === null),
    isError: query.isError,
    error: query.error,
    refetchSession: async () => {
      const result = await query.refetch();
      return result.data;
    },
  };
}
