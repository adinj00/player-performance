import { Plus } from "lucide-react";
import { useState, type ReactNode } from "react";

import { EmptyState } from "@/components/common/empty-state";
import { ErrorState } from "@/components/common/error-state";
import { LoadingState } from "@/components/common/loading-state";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { isApiError } from "@/lib/api/api-client";
import { formatDate } from "@/lib/date-format";

import {
  ArchivedFilter,
  ConfirmDialog,
  LifecycleButtons,
  SettingFormDialog,
} from "./components";
import {
  useArchivedFilter,
  useNamedSettings,
  useSeasons,
  useTeams,
} from "./hooks";
import {
  teamStatusLabels,
  trackingLevelLabels,
  type NamedSetting,
  type Season,
  type Team,
} from "./types";

function ResourceFrame({
  children,
  action,
}: {
  children: ReactNode;
  action: ReactNode;
}) {
  return (
    <section className="flex flex-col gap-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <ArchivedFilter />
        {action}
      </div>
      {children}
    </section>
  );
}

function MutationMessage({ error }: { error: unknown }) {
  if (!error) return null;
  return (
    <ErrorState
      title={
        isApiError(error) && error.status === 404
          ? "Zapis nije pronađen"
          : "Radnja nije uspjela"
      }
      description="Pokušajte ponovo nakon osvježavanja liste."
    />
  );
}

export function SeasonsSettingsPage() {
  const includeArchived = useArchivedFilter();
  const { query, create, update, lifecycle } = useSeasons(includeArchived);
  const [editing, setEditing] = useState<Season | null | "new">(null);
  const [confirmation, setConfirmation] = useState<{
    item: Season;
    action: "archive" | "restore";
  } | null>(null);
  const pending = create.isPending || update.isPending;
  const save = (values: {
    name: string;
    startDate: string;
    endDate: string;
  }) => {
    if (editing === "new")
      create.mutate(values, { onSuccess: () => setEditing(null) });
    else if (editing)
      update.mutate(
        { id: editing.id, ...values },
        { onSuccess: () => setEditing(null) },
      );
  };
  return (
    <ResourceFrame
      action={
        <Button onClick={() => setEditing("new")}>
          <Plus data-icon="inline-start" />
          Nova sezona
        </Button>
      }
    >
      {query.isLoading ? (
        <LoadingState />
      ) : query.isError ? (
        <ErrorState
          description="Sezone nije moguće učitati."
          action={
            <Button onClick={() => void query.refetch()}>Pokušaj ponovo</Button>
          }
        />
      ) : query.data?.length === 0 ? (
        <EmptyState
          title="Nema sezona"
          description="Kreirajte prvu sezonu za korištenje kroz sistem."
        />
      ) : (
        <div className="border-border overflow-x-auto rounded-xl border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Naziv</TableHead>
                <TableHead>Datum početka</TableHead>
                <TableHead>Datum završetka</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>
                  <span className="sr-only">Radnje</span>
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {query.data?.map((item) => (
                <TableRow key={item.id}>
                  <TableCell className="font-medium">{item.name}</TableCell>
                  <TableCell>{formatDate(item.startDate)}</TableCell>
                  <TableCell>{formatDate(item.endDate)}</TableCell>
                  <TableCell>
                    <Badge variant={item.isArchived ? "secondary" : "outline"}>
                      {item.isArchived ? "Arhivirana" : "Aktivna"}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <LifecycleButtons
                      archived={item.isArchived}
                      onEdit={() => setEditing(item)}
                      onAction={(action) =>
                        setConfirmation({
                          item,
                          action: action as "archive" | "restore",
                        })
                      }
                    />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
      <MutationMessage error={lifecycle.error} />
      {editing ? (
        <SettingFormDialog
          open
          onClose={() => setEditing(null)}
          kind="season"
          title={editing === "new" ? "Nova sezona" : "Uredi sezonu"}
          initial={editing === "new" ? undefined : editing}
          pending={pending}
          error={create.error ?? update.error}
          onSubmit={(values) =>
            save(values as { name: string; startDate: string; endDate: string })
          }
        />
      ) : null}
      {confirmation ? (
        <ConfirmDialog
          target={confirmation.item.name}
          action={confirmation.action}
          pending={lifecycle.isPending}
          error={lifecycle.error}
          onClose={() => setConfirmation(null)}
          onConfirm={() =>
            lifecycle.mutate(
              { id: confirmation.item.id, action: confirmation.action },
              { onSuccess: () => setConfirmation(null) },
            )
          }
        />
      ) : null}
    </ResourceFrame>
  );
}

const namedConfig = {
  competitions: {
    empty: "Nema takmičenja",
    description: "Kreirajte prvo takmičenje za korištenje kroz sistem.",
    add: "Novo takmičenje",
  },
  venues: {
    empty: "Nema lokacija",
    description: "Kreirajte prvu lokaciju za korištenje kroz sistem.",
    add: "Nova lokacija",
  },
  opponents: {
    empty: "Nema protivnika",
    description: "Kreirajte prvog protivnika za korištenje kroz sistem.",
    add: "Novi protivnik",
  },
} as const;

export function NamedSettingsPage({
  resource,
}: {
  resource: keyof typeof namedConfig;
}) {
  const includeArchived = useArchivedFilter();
  const { query, create, update, lifecycle } = useNamedSettings(
    resource,
    includeArchived,
  );
  const config = namedConfig[resource];
  const [editing, setEditing] = useState<NamedSetting | null | "new">(null);
  const [confirmation, setConfirmation] = useState<{
    item: NamedSetting;
    action: "archive" | "restore";
  } | null>(null);
  const save = (values: { name: string }) => {
    if (editing === "new")
      create.mutate(values, { onSuccess: () => setEditing(null) });
    else if (editing)
      update.mutate(
        { id: editing.id, ...values },
        { onSuccess: () => setEditing(null) },
      );
  };
  return (
    <ResourceFrame
      action={
        <Button onClick={() => setEditing("new")}>
          <Plus data-icon="inline-start" />
          {config.add}
        </Button>
      }
    >
      {query.isLoading ? (
        <LoadingState />
      ) : query.isError ? (
        <ErrorState
          description="Podatke nije moguće učitati."
          action={
            <Button onClick={() => void query.refetch()}>Pokušaj ponovo</Button>
          }
        />
      ) : query.data?.length === 0 ? (
        <EmptyState title={config.empty} description={config.description} />
      ) : (
        <div className="border-border overflow-x-auto rounded-xl border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Naziv</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>
                  <span className="sr-only">Radnje</span>
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {query.data?.map((item) => (
                <TableRow key={item.id}>
                  <TableCell className="font-medium">{item.name}</TableCell>
                  <TableCell>
                    <Badge variant={item.isArchived ? "secondary" : "outline"}>
                      {item.isArchived ? "Arhiviran" : "Aktivan"}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <LifecycleButtons
                      archived={item.isArchived}
                      onEdit={() => setEditing(item)}
                      onAction={(action) =>
                        setConfirmation({
                          item,
                          action: action as "archive" | "restore",
                        })
                      }
                    />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
      <MutationMessage error={lifecycle.error} />
      {editing ? (
        <SettingFormDialog
          open
          onClose={() => setEditing(null)}
          kind="name"
          title={editing === "new" ? config.add : "Uredi zapis"}
          initial={editing === "new" ? undefined : editing}
          pending={create.isPending || update.isPending}
          error={create.error ?? update.error}
          onSubmit={(values) => save(values as { name: string })}
        />
      ) : null}
      {confirmation ? (
        <ConfirmDialog
          target={confirmation.item.name}
          action={confirmation.action}
          pending={lifecycle.isPending}
          error={lifecycle.error}
          onClose={() => setConfirmation(null)}
          onConfirm={() =>
            lifecycle.mutate(
              { id: confirmation.item.id, action: confirmation.action },
              { onSuccess: () => setConfirmation(null) },
            )
          }
        />
      ) : null}
    </ResourceFrame>
  );
}

export function TeamsSettingsPage() {
  const includeArchived = useArchivedFilter();
  const { query, create, update, lifecycle, reorder } =
    useTeams(includeArchived);
  const [editing, setEditing] = useState<Team | null | "new">(null);
  const [confirmation, setConfirmation] = useState<{
    item: Team;
    action: "archive" | "restore" | "activate" | "deactivate";
  } | null>(null);
  const move = (team: Team, delta: number) => {
    const active = (query.data ?? []).filter(
      (item) => item.status !== "ARCHIVED",
    );
    const from = active.findIndex((item) => item.id === team.id);
    const to = from + delta;
    if (from < 0 || to < 0 || to >= active.length || reorder.isPending) return;
    [active[from], active[to]] = [active[to], active[from]];
    reorder.mutate({ orderedTeamIds: active.map((item) => item.id) });
  };
  const save = (values: {
    name: string;
    trackingLevel: Team["trackingLevel"];
  }) => {
    if (editing === "new")
      create.mutate(values, { onSuccess: () => setEditing(null) });
    else if (editing)
      update.mutate(
        { id: editing.id, ...values },
        { onSuccess: () => setEditing(null) },
      );
  };
  const activeTeams = (query.data ?? []).filter(
    (item) => item.status !== "ARCHIVED",
  );
  return (
    <ResourceFrame
      action={
        <Button onClick={() => setEditing("new")}>
          <Plus data-icon="inline-start" />
          Nova selekcija
        </Button>
      }
    >
      {query.isLoading ? (
        <LoadingState />
      ) : query.isError ? (
        <ErrorState
          description="Selekcije nije moguće učitati."
          action={
            <Button onClick={() => void query.refetch()}>Pokušaj ponovo</Button>
          }
        />
      ) : query.data?.length === 0 ? (
        <EmptyState
          title="Nema selekcija"
          description="Kreirajte prvu selekciju za korištenje kroz sistem."
        />
      ) : (
        <div className="border-border overflow-x-auto rounded-xl border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Redoslijed</TableHead>
                <TableHead>Naziv</TableHead>
                <TableHead>Nivo praćenja</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>
                  <span className="sr-only">Radnje</span>
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {query.data?.map((item) => {
                const index = activeTeams.findIndex(
                  (entry) => entry.id === item.id,
                );
                return (
                  <TableRow key={item.id}>
                    <TableCell>
                      {item.status === "ARCHIVED" ? "—" : item.displayOrder + 1}
                    </TableCell>
                    <TableCell className="font-medium">{item.name}</TableCell>
                    <TableCell>
                      {trackingLevelLabels[item.trackingLevel]}
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant={
                          item.status === "ARCHIVED" ? "secondary" : "outline"
                        }
                      >
                        {teamStatusLabels[item.status]}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <LifecycleButtons
                        archived={item.status === "ARCHIVED"}
                        status={item.status}
                        ordering={item.status !== "ARCHIVED"}
                        moveUp={index > 0 ? () => move(item, -1) : undefined}
                        moveDown={
                          index >= 0 && index < activeTeams.length - 1
                            ? () => move(item, 1)
                            : undefined
                        }
                        reorderPending={reorder.isPending}
                        onEdit={() => setEditing(item)}
                        onAction={(action) => setConfirmation({ item, action })}
                      />
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </div>
      )}
      <MutationMessage error={lifecycle.error ?? reorder.error} />
      {editing ? (
        <SettingFormDialog
          open
          onClose={() => setEditing(null)}
          kind="team"
          title={editing === "new" ? "Nova selekcija" : "Uredi selekciju"}
          initial={editing === "new" ? undefined : editing}
          pending={create.isPending || update.isPending}
          error={create.error ?? update.error}
          onSubmit={(values) =>
            save(
              values as { name: string; trackingLevel: Team["trackingLevel"] },
            )
          }
        />
      ) : null}
      {confirmation ? (
        <ConfirmDialog
          target={confirmation.item.name}
          action={confirmation.action}
          pending={lifecycle.isPending}
          error={lifecycle.error}
          onClose={() => setConfirmation(null)}
          onConfirm={() =>
            lifecycle.mutate(
              { id: confirmation.item.id, action: confirmation.action },
              { onSuccess: () => setConfirmation(null) },
            )
          }
        />
      ) : null}
    </ResourceFrame>
  );
}
