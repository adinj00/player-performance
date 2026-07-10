import { useMutation, useQueryClient } from "@tanstack/react-query";

import { logout } from "@/features/auth/api/auth-api";
import { sessionQueryKey } from "@/features/auth/hooks/use-session";

export function useLogout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: logout,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: sessionQueryKey });
    },
  });
}
