import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSearchParams } from "react-router-dom";

import { useSession } from "@/features/auth/hooks/use-session";
import {
  invalidateDashboardContextOptions,
  invalidateDashboardOverview,
} from "@/features/dashboard";

import { settingsApi } from "./api";

const key = (resource: string, includeArchived: boolean) =>
  ["settings", resource, includeArchived] as const;
const canLoad = (role: string | null | undefined) => role === "ADMIN";

export function useArchivedFilter() {
  const [params] = useSearchParams();
  return params.get("archived") === "include";
}

function useSettingsQuery<T>(
  resource: string,
  includeArchived: boolean,
  queryFn: () => Promise<T>,
) {
  const { user } = useSession();
  return useQuery({
    queryKey: key(resource, includeArchived),
    queryFn,
    enabled: canLoad(user?.primaryRole),
    retry: false,
  });
}

function useSettingsMutation<TVariables, TResponse>(
  resource: string,
  mutationFn: (variables: TVariables) => Promise<TResponse>,
) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn,
    retry: false,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["settings", resource] });
      if (resource === "seasons" || resource === "teams") {
        await invalidateDashboardContextOptions(queryClient);
        await invalidateDashboardOverview(queryClient);
      }
    },
  });
}

export function useSeasons(includeArchived: boolean) {
  return {
    query: useSettingsQuery("seasons", includeArchived, () =>
      settingsApi.listSeasons(includeArchived),
    ),
    create: useSettingsMutation("seasons", settingsApi.createSeason),
    update: useSettingsMutation(
      "seasons",
      ({
        id,
        ...request
      }: {
        id: string;
        name: string;
        startDate: string;
        endDate: string;
      }) => settingsApi.updateSeason(id, request),
    ),
    lifecycle: useSettingsMutation(
      "seasons",
      ({ id, action }: { id: string; action: "archive" | "restore" }) =>
        settingsApi.lifecycle("seasons", id, action),
    ),
  };
}

export function useNamedSettings(
  resource: "competitions" | "venues" | "opponents",
  includeArchived: boolean,
) {
  return {
    query: useSettingsQuery(resource, includeArchived, () =>
      settingsApi.listNamed(resource, includeArchived),
    ),
    create: useSettingsMutation(resource, ({ name }: { name: string }) =>
      settingsApi.createNamed(resource, name),
    ),
    update: useSettingsMutation(
      resource,
      ({ id, name }: { id: string; name: string }) =>
        settingsApi.updateNamed(resource, id, name),
    ),
    lifecycle: useSettingsMutation(
      resource,
      ({ id, action }: { id: string; action: "archive" | "restore" }) =>
        settingsApi.lifecycle(resource, id, action),
    ),
  };
}

export function useTeams(includeArchived: boolean) {
  return {
    query: useSettingsQuery("teams", includeArchived, () =>
      settingsApi.listTeams(includeArchived),
    ),
    create: useSettingsMutation("teams", settingsApi.createTeam),
    update: useSettingsMutation(
      "teams",
      ({
        id,
        ...request
      }: {
        id: string;
        name: string;
        trackingLevel: import("./types").TeamTrackingLevel;
      }) => settingsApi.updateTeam(id, request),
    ),
    lifecycle: useSettingsMutation(
      "teams",
      ({
        id,
        action,
      }: {
        id: string;
        action: "archive" | "restore" | "activate" | "deactivate";
      }) => settingsApi.lifecycle("teams", id, action),
    ),
    reorder: useSettingsMutation(
      "teams",
      ({ orderedTeamIds }: { orderedTeamIds: string[] }) =>
        settingsApi.reorderTeams(orderedTeamIds),
    ),
  };
}
