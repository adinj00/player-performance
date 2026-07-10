import { useMutation, useQueryClient } from "@tanstack/react-query";

import { logout } from "@/features/auth/api/auth-api";
import { sessionQueryKey } from "@/features/auth/hooks/use-session";
import type { SessionResponse } from "@/features/auth/types/session";

const unauthenticatedSession: SessionResponse = {
  isAuthenticated: false,
  user: null,
};

export function useLogout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: logout,
    onSuccess: async () => {
      queryClient.setQueryData(sessionQueryKey, unauthenticatedSession);
      await queryClient.invalidateQueries({ queryKey: sessionQueryKey });
    },
  });
}
