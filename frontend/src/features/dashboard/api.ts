import type { QueryClient } from "@tanstack/react-query";
import { apiRequest } from "@/lib/api/api-client";
import type { DashboardContextOptions, DashboardOverview } from "./types";

export const dashboardKeys = {
  contextOptions: ["dashboard", "context-options"] as const,
  overview: (teamId: string, seasonId: string) =>
    ["dashboard", "overview", teamId, seasonId] as const,
};
export const dashboardApi = {
  contextOptions: () =>
    apiRequest<DashboardContextOptions>("/api/dashboard/context-options"),
  overview: (teamId: string, seasonId: string) =>
    apiRequest<DashboardOverview>(
      `/api/dashboard/overview?teamId=${encodeURIComponent(teamId)}&seasonId=${encodeURIComponent(seasonId)}`,
    ),
};
export function invalidateDashboardOverview(
  queryClient: QueryClient,
  teamId?: string,
) {
  return queryClient.invalidateQueries({
    queryKey: teamId
      ? ["dashboard", "overview", teamId]
      : ["dashboard", "overview"],
  });
}
export function invalidateDashboardContextOptions(queryClient: QueryClient) {
  return queryClient.invalidateQueries({
    queryKey: dashboardKeys.contextOptions,
  });
}
