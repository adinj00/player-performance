import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
} from "@/components/ui/empty";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { matchesApi } from "@/features/matches/api/matches-api";
import {
  MatchArchiveDialog,
  MatchEditDialog,
} from "@/features/matches/components/match-dialogs";
import { useSession } from "@/features/auth/hooks/use-session";
import { LineupTab } from "@/features/matches/components/lineup-tab";
import { StatisticsTab } from "@/features/matches/components/statistics-tab";
import {
  locationLabels,
  matchStatusLabels,
} from "@/features/matches/utils/display";
import { formatUtcDateTime } from "@/lib/date-format";

const futureTabs = ["Statistika", "GPS / Fizički podaci", "Video", "Revizija"];

export function MatchDetailPage() {
  const { matchId } = useParams();
  const { user } = useSession();
  const [editing, setEditing] = useState(false);
  const [archiveOpen, setArchiveOpen] = useState(false);
  const match = useQuery({
    queryKey: ["match", matchId],
    queryFn: () => matchesApi.get(matchId!),
    enabled: !!matchId,
    retry: false,
  });
  if (match.isLoading) return <Skeleton className="h-96 w-full" />;
  if (match.isError || !match.data)
    return (
      <Alert variant="destructive">
        <AlertTitle>Utakmica nije dostupna</AlertTitle>
        <AlertDescription>
          Nemate pristup ovoj utakmici ili zapis više ne postoji.
        </AlertDescription>
      </Alert>
    );
  const data = match.data;
  const canEdit =
    user?.primaryRole === "ADMIN" ||
    (user?.primaryRole === "DATA_OPERATOR" &&
      (user.teamScope.type === "ALL" ||
        user.teamScope.selectedTeamIds.includes(data.team.id)));
  return (
    <div className="flex flex-col gap-6">
      <nav className="text-muted-foreground text-sm">
        <Link to={routePaths.matches}>Utakmice</Link> / {data.team.name} –{" "}
        {data.opponent.name}
      </nav>
      <header className="flex flex-col gap-3">
        <div className="flex flex-wrap items-center gap-2">
          <h1 className="font-heading text-3xl">
            {data.team.name} – {data.opponent.name}
          </h1>
          <Badge variant="secondary">{matchStatusLabels[data.status]}</Badge>
          {data.isArchived ? <Badge variant="outline">Arhivirana</Badge> : null}
        </div>
        <p className="text-muted-foreground">
          {formatUtcDateTime(data.kickoffAtUtc)} · {data.competition.name}
          {data.round ? ` · ${data.round}` : ""}
        </p>
        {data.status === "PLAYED" ? (
          <p className="font-mono text-xl">
            {data.teamScore} : {data.opponentScore}
          </p>
        ) : null}
        {canEdit || user?.primaryRole === "ADMIN" ? (
          <div className="flex flex-wrap gap-2">
            {canEdit ? (
              <Button onClick={() => setEditing(true)} variant="outline">
                Uredi utakmicu
              </Button>
            ) : null}
            {user?.primaryRole === "ADMIN" ? (
              <Button
                onClick={() => setArchiveOpen(true)}
                variant={data.isArchived ? "outline" : "destructive"}
              >
                {data.isArchived ? "Vrati iz arhive" : "Arhiviraj"}
              </Button>
            ) : null}
          </div>
        ) : null}
      </header>
      <Tabs defaultValue="overview">
        <TabsList>
          <TabsTrigger value="overview">Pregled</TabsTrigger>
          <TabsTrigger value="lineup">Sastav</TabsTrigger>
          <TabsTrigger value="statistics">Statistika</TabsTrigger>
          {futureTabs
            .filter((tab) => tab !== "Statistika")
            .map((tab) => (
              <TabsTrigger key={tab} value={tab}>
                {tab}
              </TabsTrigger>
            ))}
        </TabsList>
        <TabsContent value="overview">
          <dl className="grid gap-4 rounded-xl border p-5 sm:grid-cols-2">
            <div>
              <dt className="text-muted-foreground text-sm">Sezona</dt>
              <dd>{data.season.name}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground text-sm">Takmičenje</dt>
              <dd>{data.competition.name}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground text-sm">Selekcija</dt>
              <dd>{data.team.name}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground text-sm">Protivnik</dt>
              <dd>{data.opponent.name}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground text-sm">Lokacija</dt>
              <dd>{locationLabels[data.locationType]}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground text-sm">Mjesto</dt>
              <dd>{data.venue?.name ?? "Nije određeno"}</dd>
            </div>
          </dl>
        </TabsContent>
        <TabsContent value="lineup">
          <LineupTab match={data} />
        </TabsContent>
        <TabsContent value="statistics">
          <StatisticsTab match={data} />
        </TabsContent>
        {futureTabs
          .filter((tab) => tab !== "Statistika")
          .map((tab) => (
            <TabsContent key={tab} value={tab}>
              <Empty>
                <EmptyHeader>
                  <EmptyTitle>{tab}</EmptyTitle>
                  <EmptyDescription>
                    Ovaj dio još nije dostupan.
                  </EmptyDescription>
                </EmptyHeader>
              </Empty>
            </TabsContent>
          ))}
      </Tabs>
      {editing ? (
        <MatchEditDialog match={data} onClose={() => setEditing(false)} />
      ) : null}
      {archiveOpen ? (
        <MatchArchiveDialog
          match={data}
          onClose={() => setArchiveOpen(false)}
        />
      ) : null}
    </div>
  );
}
