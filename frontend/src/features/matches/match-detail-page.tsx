import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { Link, useSearchParams, useParams } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { Button } from "@/components/ui/button";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
} from "@/components/ui/empty";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { matchesApi } from "@/features/matches/api/matches-api";
import {
  MatchArchiveDialog,
  MatchEditDialog,
} from "@/features/matches/components/match-dialogs";
import { useSession } from "@/features/auth/hooks/use-session";
import { LineupTab } from "@/features/matches/components/lineup-tab";
import { StatisticsTab } from "@/features/matches/components/statistics-tab";
import { ReportReviewTab } from "@/features/matches/components/report-review-tab";
import { MediaLinksSection } from "@/features/media";
import {
  locationLabels,
  matchStatusLabels,
} from "@/features/matches/utils/display";
import { formatUtcDateTime } from "@/lib/date-format";

const matchTabs = [
  { value: "overview", label: "Pregled" },
  { value: "lineup", label: "Sastav" },
  { value: "statistics", label: "Statistika" },
  { value: "physical", label: "GPS / Fizički podaci" },
  { value: "video", label: "Mediji" },
  { value: "audit", label: "Revizija" },
] as const;

const futureTabs = matchTabs.filter(
  (tab) => !["overview", "lineup", "statistics"].includes(tab.value),
);

export function MatchDetailPage() {
  const { matchId } = useParams();
  const [searchParams, setSearchParams] = useSearchParams();
  const { user } = useSession();
  const [editing, setEditing] = useState(false);
  const [archiveOpen, setArchiveOpen] = useState(false);
  const activeTab =
    matchTabs.find((tab) => tab.value === searchParams.get("tab"))?.value ??
    "overview";
  const selectTab = (tab: (typeof matchTabs)[number]["value"]) => {
    const next = new URLSearchParams(searchParams);
    if (tab === "overview") next.delete("tab");
    else next.set("tab", tab);
    setSearchParams(next);
  };
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
      <Breadcrumb>
        <BreadcrumbList>
          <BreadcrumbItem>
            <BreadcrumbLink render={<Link to={routePaths.matches} />}>
              Utakmice
            </BreadcrumbLink>
          </BreadcrumbItem>
          <BreadcrumbSeparator />
          <BreadcrumbItem>
            <BreadcrumbPage>
              {data.team.name} – {data.opponent.name}
            </BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>
      <nav className="sr-only">
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
      <Tabs
        value={activeTab}
        onValueChange={(tab) => selectTab(tab as typeof activeTab)}
      >
        <div className="md:hidden">
          <Select
            value={activeTab}
            onValueChange={(tab) => {
              if (tab) selectTab(tab as typeof activeTab);
            }}
          >
            <SelectTrigger
              aria-label="Odaberite sadržaj utakmice"
              className="w-full"
            >
              <SelectValue>
                {matchTabs.find((tab) => tab.value === activeTab)?.label}
              </SelectValue>
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                {matchTabs.map((tab) => (
                  <SelectItem key={tab.value} value={tab.value}>
                    {tab.label}
                  </SelectItem>
                ))}
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>
        <TabsList aria-label="Sadržaj utakmice" className="hidden md:flex">
          {matchTabs.map((tab) => (
            <TabsTrigger key={tab.value} value={tab.value}>
              {tab.label}
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
        <TabsContent value="video">
          <MatchMediaTab matchId={data.id} />
        </TabsContent>
        <TabsContent value="audit">
          <ReportReviewTab
            match={data}
            onNavigate={(tab) => {
              selectTab(tab);
            }}
          />
        </TabsContent>
        {futureTabs
          .filter((tab) => tab.value !== "audit" && tab.value !== "video")
          .map((tab) => (
            <TabsContent key={tab.value} value={tab.value}>
              <Empty>
                <EmptyHeader>
                  <EmptyTitle>{tab.label}</EmptyTitle>
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

function MatchMediaTab({ matchId }: { matchId: string }) {
  const report = useQuery({
    queryKey: ["match", matchId, "report"],
    queryFn: () => matchesApi.getReport(matchId),
    retry: false,
  });
  return (
    <div className="flex flex-col gap-6">
      <MediaLinksSection
        targetId={matchId}
        targetType="MATCH"
        title="Mediji utakmice"
      />
      {report.data ? (
        <MediaLinksSection
          targetId={report.data.id}
          targetType="MATCH_REPORT"
          title="Mediji izvještaja"
        />
      ) : null}
    </div>
  );
}
