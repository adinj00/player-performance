import {
  flexRender,
  getCoreRowModel,
  type ColumnDef,
  useReactTable,
} from "@tanstack/react-table";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  parseAsBoolean,
  parseAsInteger,
  parseAsString,
  useQueryStates,
} from "nuqs";
import { MoreHorizontal, Plus } from "lucide-react";
import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { routePaths } from "@/app/route-paths";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
} from "@/components/ui/empty";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { DatePicker } from "@/components/common/date-picker";
import { ErrorState } from "@/components/common/error-state";
import { FilterSelect } from "@/components/common/filter-select";
import { LoadingState } from "@/components/common/loading-state";
import { PageHeader } from "@/components/common/page-header";
import { useSession } from "@/features/auth/hooks/use-session";
import { settingsApi } from "@/features/settings/api";
import { matchesApi } from "@/features/matches/api/matches-api";
import type {
  MatchResponse,
  MatchStatus,
} from "@/features/matches/types/match";
import {
  locationLabels,
  matchStatusLabels,
} from "@/features/matches/utils/display";
import { formatUtcDateTime } from "@/lib/date-format";

const statuses: MatchStatus[] = [
  "SCHEDULED",
  "PLAYED",
  "POSTPONED",
  "CANCELLED",
];

function canCreate(role: string | null | undefined) {
  return role === "ADMIN" || role === "DATA_OPERATOR";
}
function errorMessage(error: unknown) {
  return error instanceof Error
    ? error.message
    : "Učitavanje nije uspjelo. Pokušajte ponovo.";
}

export function MatchesPage() {
  const navigate = useNavigate();
  const { user } = useSession();
  const queryClient = useQueryClient();
  const [editing, setEditing] = useState<MatchResponse | null>(null);
  const [filters, setFilters] = useQueryStates({
    seasonId: parseAsString,
    teamId: parseAsString,
    competitionId: parseAsString,
    opponentId: parseAsString,
    status: parseAsString,
    dateFrom: parseAsString,
    dateTo: parseAsString,
    archived: parseAsBoolean.withDefault(false),
    page: parseAsInteger.withDefault(1),
  });
  const hasInvalidDateRange =
    !!filters.dateFrom && !!filters.dateTo && filters.dateFrom > filters.dateTo;
  const matches = useQuery({
    queryKey: ["matches", filters],
    queryFn: () =>
      matchesApi.list({
        ...filters,
        status: filters.status as MatchStatus | null,
        dateFrom: hasInvalidDateRange ? null : filters.dateFrom,
        dateTo: hasInvalidDateRange ? null : filters.dateTo,
        includeArchived: filters.archived,
        page: filters.page,
      }),
  });
  const settings = useQuery({
    queryKey: ["matches", "filter-options"],
    queryFn: async () => {
      const [seasons, teams, competitions, opponents] = await Promise.all([
        settingsApi.listSeasons(false),
        settingsApi.listTeams(false),
        settingsApi.listNamed("competitions", false),
        settingsApi.listNamed("opponents", false),
      ]);
      return { seasons, teams, competitions, opponents };
    },
    retry: false,
  });
  const archive = useMutation({
    mutationFn: (match: MatchResponse) =>
      match.isArchived
        ? matchesApi.restore(match.id)
        : matchesApi.archive(match.id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["matches"] });
      toast.success("Stanje arhive je ažurirano.");
    },
    onError: (error) => toast.error(errorMessage(error)),
  });
  const change = (value: Partial<typeof filters>) =>
    void setFilters({ ...value, page: 1 });
  const clear = () =>
    void setFilters({
      seasonId: null,
      teamId: null,
      competitionId: null,
      opponentId: null,
      status: null,
      dateFrom: null,
      dateTo: null,
      archived: false,
      page: 1,
    });
  const hasFilters = !!(
    filters.seasonId ||
    filters.teamId ||
    filters.competitionId ||
    filters.opponentId ||
    filters.status ||
    filters.dateFrom ||
    filters.dateTo ||
    filters.archived
  );
  const columns = useMemo(
    () => [
      {
        header: "Datum i vrijeme",
        cell: (item: MatchResponse) => formatUtcDateTime(item.kickoffAtUtc),
      },
      { header: "Selekcija", cell: (item: MatchResponse) => item.team.name },
      {
        header: "Protivnik",
        cell: (item: MatchResponse) => item.opponent.name,
      },
      {
        header: "Takmičenje",
        cell: (item: MatchResponse) => item.competition.name,
      },
      { header: "Kolo", cell: (item: MatchResponse) => item.round ?? "—" },
      {
        header: "Lokacija",
        cell: (item: MatchResponse) => locationLabels[item.locationType],
      },
      {
        header: "Rezultat / status",
        cell: (item: MatchResponse) => (
          <div className="flex flex-col gap-1">
            <Badge variant="secondary">{matchStatusLabels[item.status]}</Badge>
            {item.status === "PLAYED" ? (
              <span className="font-mono">
                {item.teamScore} : {item.opponentScore}
              </span>
            ) : null}
          </div>
        ),
      },
      {
        header: "Mjesto",
        cell: (item: MatchResponse) => item.venue?.name ?? "—",
      },
      {
        id: "actions",
        header: "",
        cell: (item: MatchResponse) => (
          <DropdownMenu>
            <DropdownMenuTrigger
              render={
                <Button
                  aria-label="Radnje za utakmicu"
                  size="icon"
                  variant="ghost"
                >
                  <MoreHorizontal />
                </Button>
              }
            />
            <DropdownMenuContent align="end">
              <DropdownMenuGroup>
                <DropdownMenuItem
                  onClick={() => navigate(routePaths.matchDetail(item.id))}
                >
                  Otvori
                </DropdownMenuItem>
                {canCreate(user?.primaryRole) ? (
                  <DropdownMenuItem onClick={() => setEditing(item)}>
                    Uredi
                  </DropdownMenuItem>
                ) : null}
                {user?.primaryRole === "ADMIN" ? (
                  <DropdownMenuItem onClick={() => archive.mutate(item)}>
                    {item.isArchived ? "Vrati iz arhive" : "Arhiviraj"}
                  </DropdownMenuItem>
                ) : null}
              </DropdownMenuGroup>
            </DropdownMenuContent>
          </DropdownMenu>
        ),
      },
    ],
    [archive, navigate, user?.primaryRole],
  );
  const table = useReactTable({
    data: matches.data?.items ?? [],
    columns: columns.map(({ cell, ...column }) => ({
      ...column,
      accessorFn: (row: MatchResponse) => row.id,
      cell: (context: { row: { original: MatchResponse } }) =>
        cell(context.row.original),
    })) as unknown as ColumnDef<MatchResponse>[],
    getCoreRowModel: getCoreRowModel(),
  });
  const page = matches.data;
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Utakmice"
        description="Raspored, rezultati i osnovni podaci o utakmicama."
        actions={
          canCreate(user?.primaryRole) ? (
            <Button
              onClick={() =>
                toast.info(
                  "Forma za novu utakmicu bit će dostupna u sljedećem ažuriranju.",
                )
              }
            >
              <Plus data-icon="inline-start" />
              Nova utakmica
            </Button>
          ) : null
        }
      />
      {editing ? (
        <MatchEditNotice match={editing} onClose={() => setEditing(null)} />
      ) : null}
      <section className="border-border bg-card grid gap-3 rounded-xl border p-4 sm:grid-cols-2 md:flex md:flex-wrap">
        <FilterSelect
          label="Sezona"
          emptyLabel="Sve sezone"
          value={filters.seasonId}
          options={Object.fromEntries(
            (settings.data?.seasons ?? []).map((item) => [item.id, item.name]),
          )}
          className="w-full md:w-44"
          onChange={(seasonId) => change({ seasonId })}
        />
        <FilterSelect
          label="Selekcija"
          emptyLabel="Sve selekcije"
          value={filters.teamId}
          options={Object.fromEntries(
            (settings.data?.teams ?? [])
              .filter((item) => item.status === "ACTIVE")
              .map((item) => [item.id, item.name]),
          )}
          className="w-full md:w-48"
          onChange={(teamId) => change({ teamId })}
        />
        <FilterSelect
          label="Takmičenje"
          emptyLabel="Sva takmičenja"
          value={filters.competitionId}
          options={Object.fromEntries(
            (settings.data?.competitions ?? []).map((item) => [
              item.id,
              item.name,
            ]),
          )}
          className="w-full md:w-48"
          onChange={(competitionId) => change({ competitionId })}
        />
        <FilterSelect
          label="Protivnik"
          emptyLabel="Svi protivnici"
          value={filters.opponentId}
          options={Object.fromEntries(
            (settings.data?.opponents ?? []).map((item) => [
              item.id,
              item.name,
            ]),
          )}
          className="w-full md:w-48"
          onChange={(opponentId) => change({ opponentId })}
        />
        <FilterSelect
          label="Status"
          emptyLabel="Svi statusi"
          value={filters.status}
          options={matchStatusLabels}
          className="w-full md:w-40"
          onChange={(status) => change({ status })}
        />
        <DatePicker
          id="matches-date-from"
          value={filters.dateFrom ?? undefined}
          onChange={(dateFrom) =>
            change({
              dateFrom,
              dateTo:
                filters.dateTo && dateFrom > filters.dateTo
                  ? null
                  : filters.dateTo,
            })
          }
          placeholder="Datum od"
          className="w-full md:w-44"
          disabledDates={
            filters.dateTo
              ? { after: new Date(`${filters.dateTo}T00:00:00`) }
              : undefined
          }
        />
        <DatePicker
          id="matches-date-to"
          value={filters.dateTo ?? undefined}
          onChange={(dateTo) =>
            change({
              dateTo,
              dateFrom:
                filters.dateFrom && dateTo < filters.dateFrom
                  ? null
                  : filters.dateFrom,
            })
          }
          placeholder="Datum do"
          className="w-full md:w-44"
          disabledDates={
            filters.dateFrom
              ? { before: new Date(`${filters.dateFrom}T00:00:00`) }
              : undefined
          }
        />
        <label className="flex shrink-0 items-center gap-2 text-sm">
          <Checkbox
            id="matches-include-archived"
            checked={filters.archived}
            onCheckedChange={(checked) =>
              change({ archived: checked === true })
            }
          />
          <span>Prikaži arhivirane</span>
        </label>
      </section>
      {matches.isLoading || settings.isLoading ? (
        <LoadingState />
      ) : matches.isError ? (
        <ErrorState
          description="Utakmice nije moguće učitati."
          action={
            <Button onClick={() => void matches.refetch()}>
              Pokušaj ponovo
            </Button>
          }
        />
      ) : page?.items.length === 0 ? (
        <Empty>
          <EmptyHeader>
            <EmptyTitle>
              {hasFilters
                ? "Nema rezultata za odabrane filtere"
                : "Nema utakmica"}
            </EmptyTitle>
            <EmptyDescription>
              {hasFilters
                ? "Promijenite ili poništite filtere."
                : "Još nema unesenih utakmica."}
            </EmptyDescription>
          </EmptyHeader>
          <EmptyContent>
            {hasFilters ? (
              <Button onClick={clear} variant="outline">
                Poništi filtere
              </Button>
            ) : null}
          </EmptyContent>
        </Empty>
      ) : (
        <>
          <div className="border-border overflow-x-auto rounded-xl border">
            <Table>
              <TableHeader className="bg-muted">
                {table.getHeaderGroups().map((group) => (
                  <TableRow key={group.id}>
                    {group.headers.map((header) => (
                      <TableHead key={header.id}>
                        {flexRender(
                          header.column.columnDef.header,
                          header.getContext(),
                        )}
                      </TableHead>
                    ))}
                  </TableRow>
                ))}
              </TableHeader>
              <TableBody>
                {table.getRowModel().rows.map((row) => (
                  <TableRow
                    className="cursor-pointer"
                    key={row.id}
                    onClick={() =>
                      navigate(routePaths.matchDetail(row.original.id))
                    }
                  >
                    {row.getVisibleCells().map((cell) => (
                      <TableCell
                        key={cell.id}
                        onClick={(event) => {
                          if (cell.column.id === "actions")
                            event.stopPropagation();
                        }}
                      >
                        {flexRender(
                          cell.column.columnDef.cell,
                          cell.getContext(),
                        )}
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          <div className="flex items-center justify-between gap-3">
            <p className="text-muted-foreground text-sm">
              Ukupno: {page?.totalCount ?? 0}
            </p>
            <div className="flex gap-2">
              <Button
                disabled={!page || page.page <= 1}
                onClick={() => change({ page: page!.page - 1 })}
                variant="outline"
              >
                Prethodna
              </Button>
              <Button
                disabled={!page || page.page >= page.totalPages}
                onClick={() => change({ page: page!.page + 1 })}
                variant="outline"
              >
                Sljedeća
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
  );
  return (
    <div className="flex flex-col gap-6">
      <header className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
        <div>
          <h1 className="font-heading text-3xl">Utakmice</h1>
          <p className="text-muted-foreground mt-2">
            Raspored, rezultati i osnovni podaci o utakmicama.
          </p>
        </div>
        {canCreate(user?.primaryRole) ? (
          <Button
            onClick={() =>
              toast.info(
                "Forma za novu utakmicu bit će dostupna u sljedećem ažuriranju.",
              )
            }
          >
            <Plus data-icon="inline-start" />
            Nova utakmica
          </Button>
        ) : null}
      </header>
      <Tabs
        value={filters.status ?? "all"}
        onValueChange={(value) =>
          change({ status: value === "all" ? null : value })
        }
      >
        <TabsList>
          <TabsTrigger value="all">Sve</TabsTrigger>
          {statuses.map((status) => (
            <TabsTrigger key={status} value={status}>
              {matchStatusLabels[status]}
            </TabsTrigger>
          ))}
        </TabsList>
      </Tabs>
      <div className="flex flex-wrap gap-3">
        <Input
          aria-label="Datum od"
          className="w-40"
          type="date"
          value={filters.dateFrom ?? ""}
          onChange={(event) => change({ dateFrom: event.target.value || null })}
        />
        <Input
          aria-label="Datum do"
          className="w-40"
          type="date"
          value={filters.dateTo ?? ""}
          onChange={(event) => change({ dateTo: event.target.value || null })}
        />
        <Button
          aria-pressed={filters.archived}
          onClick={() => change({ archived: !filters.archived })}
          variant="outline"
        >
          Prikaži arhivirane
        </Button>
        {hasFilters ? (
          <Button onClick={clear} variant="ghost">
            Očisti filtere
          </Button>
        ) : null}
      </div>
      {matches.isLoading ? (
        <Skeleton className="h-96 w-full" />
      ) : matches.isError ? (
        <Alert variant="destructive">
          <AlertTitle>Učitavanje utakmica nije uspjelo</AlertTitle>
          <AlertDescription className="flex items-center justify-between gap-3">
            {errorMessage(matches.error)}
            <Button onClick={() => void matches.refetch()} variant="outline">
              Pokušaj ponovo
            </Button>
          </AlertDescription>
        </Alert>
      ) : page?.items.length === 0 ? (
        <Empty>
          <EmptyHeader>
            <EmptyTitle>
              {hasFilters ? "Nema rezultata" : "Nema utakmica"}
            </EmptyTitle>
            <EmptyDescription>
              {hasFilters
                ? "Nijedna utakmica ne odgovara trenutnim filterima."
                : "Još nema unesenih utakmica."}
            </EmptyDescription>
          </EmptyHeader>
          <EmptyContent>
            {hasFilters ? (
              <Button onClick={clear} variant="outline">
                Očisti filtere
              </Button>
            ) : null}
          </EmptyContent>
        </Empty>
      ) : (
        <>
          <div className="overflow-x-auto rounded-xl border">
            <Table>
              <TableHeader>
                {table.getHeaderGroups().map((group) => (
                  <TableRow key={group.id}>
                    {group.headers.map((header) => (
                      <TableHead key={header.id}>
                        {flexRender(
                          header.column.columnDef.header,
                          header.getContext(),
                        )}
                      </TableHead>
                    ))}
                  </TableRow>
                ))}
              </TableHeader>
              <TableBody>
                {table.getRowModel().rows.map((row) => (
                  <TableRow
                    className="cursor-pointer"
                    key={row.id}
                    onClick={() =>
                      navigate(routePaths.matchDetail(row.original.id))
                    }
                  >
                    {row.getVisibleCells().map((cell) => (
                      <TableCell
                        key={cell.id}
                        onClick={(event) => {
                          if (cell.column.id === "actions")
                            event.stopPropagation();
                        }}
                      >
                        {flexRender(
                          cell.column.columnDef.cell,
                          cell.getContext(),
                        )}
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          <div className="flex items-center justify-between gap-3">
            <p className="text-muted-foreground text-sm">
              Stranica {page?.page ?? 1} od {page?.totalPages ?? 1} ·{" "}
              {page?.totalCount ?? 0} ukupno
            </p>
            <div className="flex gap-2">
              <Button
                disabled={(page?.page ?? 1) <= 1}
                onClick={() => change({ page: (page?.page ?? 2) - 1 })}
                variant="outline"
              >
                Prethodna
              </Button>
              <Button
                disabled={(page?.page ?? 1) >= (page?.totalPages ?? 1)}
                onClick={() => change({ page: (page?.page ?? 0) + 1 })}
                variant="outline"
              >
                Sljedeća
              </Button>
            </div>
          </div>
        </>
      )}
      {editing ? (
        <MatchEditNotice match={editing!} onClose={() => setEditing(null)} />
      ) : null}
    </div>
  );
}

function MatchEditNotice({
  match,
  onClose,
}: {
  match: MatchResponse;
  onClose: () => void;
}) {
  return (
    <Alert>
      <AlertTitle>Uređivanje utakmice</AlertTitle>
      <AlertDescription className="flex items-center justify-between gap-3">
        Uređivanje za {match.team.name} protiv {match.opponent.name} uskoro će
        biti dostupno.
        <Button onClick={onClose} variant="outline">
          Zatvori
        </Button>
      </AlertDescription>
    </Alert>
  );
}
