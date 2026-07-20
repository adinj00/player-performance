import {
  flexRender,
  getCoreRowModel,
  type ColumnDef,
  useReactTable,
} from "@tanstack/react-table";
import { useQuery } from "@tanstack/react-query";
import { parseAsInteger, parseAsString, useQueryStates } from "nuqs";
import { useMemo } from "react";
import { Link, useNavigate } from "react-router-dom";
import { MoreHorizontal } from "lucide-react";

import { routePaths } from "@/app/route-paths";
import { DatePicker } from "@/components/common/date-picker";
import { ErrorState } from "@/components/common/error-state";
import { FilterSelect } from "@/components/common/filter-select";
import { PageHeader } from "@/components/common/page-header";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
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
import { useSession } from "@/features/auth/hooks/use-session";
import { matchesApi } from "@/features/matches/api/matches-api";
import type {
  MatchReportListItem,
  MatchReportStatus,
} from "@/features/matches/types/match";
import { reportStatusLabels } from "@/features/matches/utils/reports";
import { settingsApi } from "@/features/settings/api";
import { formatUtcDateTime } from "@/lib/date-format";

export function MatchReportsPage() {
  const navigate = useNavigate();
  const { user } = useSession();
  const [filters, setFilters] = useQueryStates({
    seasonId: parseAsString,
    teamId: parseAsString,
    competitionId: parseAsString,
    status: parseAsString,
    dateFrom: parseAsString,
    dateTo: parseAsString,
    page: parseAsInteger.withDefault(1),
  });
  const invalidRange =
    !!filters.dateFrom && !!filters.dateTo && filters.dateFrom > filters.dateTo;
  const reports = useQuery({
    queryKey: ["match-reports", filters],
    queryFn: () =>
      matchesApi.listReports({
        ...filters,
        status: filters.status as MatchReportStatus | null,
        dateFrom: invalidRange ? null : filters.dateFrom,
        dateTo: invalidRange ? null : filters.dateTo,
      }),
  });
  const options = useQuery({
    queryKey: ["match-reports", "filter-options"],
    queryFn: async () => {
      const [seasons, teams, competitions] = await Promise.all([
        settingsApi.listSeasons(false),
        settingsApi.listTeams(false),
        settingsApi.listNamed("competitions", false),
      ]);
      return { seasons, teams, competitions };
    },
    retry: false,
  });
  const change = (value: Partial<typeof filters>) =>
    void setFilters({ ...value, page: 1 });
  const clear = () =>
    void setFilters({
      seasonId: null,
      teamId: null,
      competitionId: null,
      status: null,
      dateFrom: null,
      dateTo: null,
      page: 1,
    });
  const hasFilters = !!(
    filters.seasonId ||
    filters.teamId ||
    filters.competitionId ||
    filters.status ||
    filters.dateFrom ||
    filters.dateTo
  );
  const teamOptions = (options.data?.teams ?? []).filter(
    (team) =>
      team.status === "ACTIVE" &&
      (user?.teamScope.type !== "SELECTED_TEAMS" ||
        user.teamScope.selectedTeamIds.includes(team.id)),
  );
  const columns = useMemo(
    () => [
      {
        header: "Utakmica",
        cell: (item: MatchReportListItem) => (
          <div className="flex flex-col gap-1">
            <Link
              className="font-medium hover:underline"
              to={`${routePaths.matchDetail(item.matchId)}?tab=audit`}
            >
              {item.team.name} – {item.opponent.name}
            </Link>
            <span className="text-muted-foreground text-sm">
              {formatUtcDateTime(item.kickoffAtUtc)}
            </span>
          </div>
        ),
      },
      {
        header: "Takmičenje",
        cell: (item: MatchReportListItem) => item.competition.name,
      },
      {
        header: "Status",
        cell: (item: MatchReportListItem) => (
          <Badge variant="secondary">{reportStatusLabels[item.status]}</Badge>
        ),
      },
      {
        header: "Poslano",
        cell: (item: MatchReportListItem) =>
          item.submitted ? formatUtcDateTime(item.submitted.atUtc) : "—",
      },
      {
        header: "Verificirano",
        cell: (item: MatchReportListItem) =>
          item.verified ? formatUtcDateTime(item.verified.atUtc) : "—",
      },
      {
        header: "Korekcija",
        cell: (item: MatchReportListItem) =>
          item.lastCorrection ? item.lastCorrection.reason : "—",
      },
      {
        id: "actions",
        header: "",
        cell: (item: MatchReportListItem) => (
          <DropdownMenu>
            <DropdownMenuTrigger
              render={
                <Button
                  aria-label="Radnje za izvještaj"
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
                  onClick={() =>
                    navigate(
                      `${routePaths.matchDetail(item.matchId)}?tab=audit`,
                    )
                  }
                >
                  Otvori reviziju
                </DropdownMenuItem>
              </DropdownMenuGroup>
            </DropdownMenuContent>
          </DropdownMenu>
        ),
      },
    ],
    [navigate],
  );
  // TanStack Table creates a mutable table instance; React Compiler must not memoize it.
  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({
    data: reports.data?.items ?? [],
    columns: columns.map(({ cell, ...column }) => ({
      ...column,
      accessorFn: (row: MatchReportListItem) => row.id,
      cell: (context: { row: { original: MatchReportListItem } }) =>
        cell(context.row.original),
    })) as unknown as ColumnDef<MatchReportListItem>[],
    getCoreRowModel: getCoreRowModel(),
  });
  const page = reports.data;
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Izvještaji utakmica"
        description="Red za pregled i tok verifikacije izvještaja utakmica."
      />
      <section className="border-border bg-card grid gap-3 rounded-xl border p-4 sm:grid-cols-2 md:flex md:flex-wrap">
        <FilterSelect
          label="Sezona"
          emptyLabel="Sve sezone"
          value={filters.seasonId}
          options={Object.fromEntries(
            (options.data?.seasons ?? []).map((item) => [item.id, item.name]),
          )}
          className="w-full md:w-44"
          onChange={(seasonId) => change({ seasonId })}
        />
        <FilterSelect
          label="Selekcija"
          emptyLabel="Sve selekcije"
          value={filters.teamId}
          options={Object.fromEntries(
            teamOptions.map((item) => [item.id, item.name]),
          )}
          className="w-full md:w-48"
          onChange={(teamId) => change({ teamId })}
        />
        <FilterSelect
          label="Takmičenje"
          emptyLabel="Sva takmičenja"
          value={filters.competitionId}
          options={Object.fromEntries(
            (options.data?.competitions ?? []).map((item) => [
              item.id,
              item.name,
            ]),
          )}
          className="w-full md:w-48"
          onChange={(competitionId) => change({ competitionId })}
        />
        <FilterSelect
          label="Status izvještaja"
          emptyLabel="Svi izvještaji"
          value={filters.status}
          options={reportStatusLabels}
          className="w-full md:w-52"
          onChange={(status) => change({ status })}
        />
        <DatePicker
          id="reports-date-from"
          value={filters.dateFrom ?? undefined}
          placeholder="Datum od"
          className="w-full md:w-44"
          onChange={(dateFrom) =>
            change({
              dateFrom,
              dateTo:
                filters.dateTo && dateFrom > filters.dateTo
                  ? null
                  : filters.dateTo,
            })
          }
        />
        <DatePicker
          id="reports-date-to"
          value={filters.dateTo ?? undefined}
          placeholder="Datum do"
          className="w-full md:w-44"
          onChange={(dateTo) =>
            change({
              dateTo,
              dateFrom:
                filters.dateFrom && dateTo < filters.dateFrom
                  ? null
                  : filters.dateFrom,
            })
          }
        />
        {hasFilters ? (
          <Button variant="outline" onClick={clear}>
            Poništi filtere
          </Button>
        ) : null}
      </section>
      {reports.isLoading || options.isLoading ? (
        <Skeleton className="h-96 w-full" />
      ) : reports.isError ? (
        <ErrorState
          description="Izvještaje nije moguće učitati."
          action={
            <Button onClick={() => void reports.refetch()}>
              Pokušaj ponovo
            </Button>
          }
        />
      ) : page?.items.length === 0 ? (
        <Empty>
          <EmptyHeader>
            <EmptyTitle>
              {filters.status === "READY_FOR_REVIEW"
                ? "Trenutno nema izvještaja spremnih za pregled"
                : hasFilters
                  ? "Nema rezultata za odabrane filtere"
                  : "Nema izvještaja utakmica"}
            </EmptyTitle>
            <EmptyDescription>
              {hasFilters
                ? "Promijenite ili poništite filtere."
                : "Izvještaji će se prikazati kada budu kreirani."}
            </EmptyDescription>
          </EmptyHeader>
          <EmptyContent>
            {hasFilters ? (
              <Button variant="outline" onClick={clear}>
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
                  <TableRow key={row.id}>
                    {row.getVisibleCells().map((cell) => (
                      <TableCell key={cell.id}>
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
                variant="outline"
                onClick={() => change({ page: page!.page - 1 })}
              >
                Prethodna
              </Button>
              <Button
                disabled={!page || page.page >= page.totalPages}
                variant="outline"
                onClick={() => change({ page: page!.page + 1 })}
              >
                Sljedeća
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
