export interface DashboardTeamOption {
  id: string;
  name: string;
  status: string;
  displayOrder: number;
}
export interface DashboardSeasonOption {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isArchived: boolean;
}
export interface DashboardContextOptions {
  teams: DashboardTeamOption[];
  seasons: DashboardSeasonOption[];
}
export interface DashboardStatusCount {
  status: string;
  count: number;
}
export interface DashboardOverview {
  context: {
    team: DashboardTeamOption;
    season: DashboardSeasonOption;
    generatedAtUtc: string;
  };
  recentMatches: {
    id: string;
    kickoffAtUtc: string;
    competitionName: string;
    opponentName: string;
    locationType: string;
    teamScore: number;
    opponentScore: number;
    result: string;
    reportStatus: string | null;
  }[];
  teamForm: {
    consideredMatchCount: number;
    wins: number;
    draws: number;
    losses: number;
    goalsFor: number;
    goalsAgainst: number;
    form: { matchId: string; result: string }[];
  };
  reportWorkflow: {
    visibilityMode: string;
    statusCounts: DashboardStatusCount[];
    playedMatchCount?: number;
    missingReportCount?: number;
  };
  availability: {
    scope: string;
    asOfDate: string;
    totalPlayers: number;
    availableCount: number;
    limitedCount: number;
    unavailableCount: number;
    rehabCount: number;
    unknownCount: number;
  };
  statisticsLeaders: {
    groups: {
      metricCode: string;
      valueKind: string;
      eligibleReportCount: number;
      leaders: {
        rank: number;
        player: { id: string; displayName: string };
        value: number;
        appearanceCount: number;
        matchCount: number;
      }[];
      hasAdditionalTies: boolean;
    }[];
    emptyReason: string | null;
  };
  physicalWorkload: {
    groups: DashboardWorkloadGroup[];
    truncated: boolean;
    availableGroupCount: number;
    returnedGroupCount: number;
    hasMultipleComparabilityContexts: boolean;
    splitMetricContextCount: number;
    emptyReason: string | null;
  };
  qualityAlerts: {
    code: string;
    severity: string;
    count: number;
    destination: string;
    isSeasonScoped: boolean;
  }[];
  generatedAtUtc: string;
}
export interface DashboardWorkloadGroup {
  contextType: string;
  metricCode: string;
  comparabilityKey: string;
  unitCode: string;
  thresholdContext: {
    value: number | null;
    unitCode: string | null;
    direction: string | null;
    scope: string | null;
  };
  methodContext: { key: string | null; version: string | null };
  aggregationKind: string;
  aggregateValue: number;
  workloadCount: number;
  playerCount: number;
  contextCount: number;
  firstOccurredOn: string;
  lastOccurredOn: string;
}
