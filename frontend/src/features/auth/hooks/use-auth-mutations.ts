import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  changePassword,
  getSession,
  login,
} from "@/features/auth/api/auth-api";
import { sessionQueryKey } from "@/features/auth/hooks/use-session";
import type {
  ChangePasswordRequest,
  LoginRequest,
  SessionResponse,
} from "@/features/auth/types/session";

async function refreshSession(
  queryClient: ReturnType<typeof useQueryClient>,
  session: SessionResponse,
): Promise<SessionResponse> {
  queryClient.setQueryData(sessionQueryKey, session);
  await queryClient.invalidateQueries({ queryKey: sessionQueryKey });

  return queryClient.fetchQuery({
    queryKey: sessionQueryKey,
    queryFn: getSession,
    staleTime: 0,
  });
}

export function useLogin() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (request: LoginRequest) =>
      refreshSession(queryClient, await login(request)),
    retry: false,
  });
}

export function useChangePassword() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (request: ChangePasswordRequest) =>
      refreshSession(queryClient, await changePassword(request)),
    retry: false,
  });
}
