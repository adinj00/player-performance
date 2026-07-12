import type {
  GoalkeeperStatisticFieldCode,
  MatchReportStatisticsResponse,
  PlayerStatisticFieldCode,
  SaveMatchReportStatisticsRequest,
} from "@/features/matches/types/match";

export type StatisticFieldCode =
  PlayerStatisticFieldCode | GoalkeeperStatisticFieldCode;

export interface StatisticFieldPresentation {
  label: string;
  shortLabel: string;
  group: string;
  description: string;
  input: "integer" | "boolean";
  order: number;
}

export const statisticFieldRegistry: Record<
  StatisticFieldCode,
  StatisticFieldPresentation
> = {
  goals: {
    label: "Golovi",
    shortLabel: "GOL",
    group: "Napad",
    description: "Postignuti golovi",
    input: "integer",
    order: 1,
  },
  assists: {
    label: "Asistencije",
    shortLabel: "AST",
    group: "Napad",
    description: "Asistencije",
    input: "integer",
    order: 2,
  },
  yellowCards: {
    label: "Žuti kartoni",
    shortLabel: "ŽK",
    group: "Disciplina",
    description: "Žuti kartoni",
    input: "integer",
    order: 3,
  },
  redCards: {
    label: "Crveni kartoni",
    shortLabel: "CK",
    group: "Disciplina",
    description: "Crveni kartoni",
    input: "integer",
    order: 4,
  },
  shots: {
    label: "Udarci",
    shortLabel: "UD",
    group: "Napad",
    description: "Ukupni udarci",
    input: "integer",
    order: 5,
  },
  shotsOnTarget: {
    label: "Udarci u okvir",
    shortLabel: "UO",
    group: "Napad",
    description: "Udarci u okvir gola",
    input: "integer",
    order: 6,
  },
  passesAttempted: {
    label: "Pokušaji dodavanja",
    shortLabel: "PD",
    group: "Dodavanja",
    description: "Ukupno pokušaja dodavanja",
    input: "integer",
    order: 7,
  },
  passesCompleted: {
    label: "Uspješna dodavanja",
    shortLabel: "UD",
    group: "Dodavanja",
    description: "Uspješno izvedena dodavanja",
    input: "integer",
    order: 8,
  },
  keyPasses: {
    label: "Ključna dodavanja",
    shortLabel: "KD",
    group: "Dodavanja",
    description: "Ključna dodavanja",
    input: "integer",
    order: 9,
  },
  duelsAttempted: {
    label: "Dueli",
    shortLabel: "DU",
    group: "Dueli",
    description: "Ukupno duela",
    input: "integer",
    order: 10,
  },
  duelsWon: {
    label: "Osvojeni dueli",
    shortLabel: "OD",
    group: "Dueli",
    description: "Osvojeni dueli",
    input: "integer",
    order: 11,
  },
  foulsCommitted: {
    label: "Napravljeni prekršaji",
    shortLabel: "NP",
    group: "Disciplina",
    description: "Prekršaji koje je igrač napravio",
    input: "integer",
    order: 12,
  },
  foulsWon: {
    label: "Iznuđeni prekršaji",
    shortLabel: "IP",
    group: "Disciplina",
    description: "Prekršaji nad igračem",
    input: "integer",
    order: 13,
  },
  offsides: {
    label: "Zaleđa",
    shortLabel: "ZAL",
    group: "Napad",
    description: "Ofsajd situacije",
    input: "integer",
    order: 14,
  },
  ballRecoveries: {
    label: "Osvojene lopte",
    shortLabel: "OL",
    group: "Odbrana",
    description: "Osvojene lopte",
    input: "integer",
    order: 15,
  },
  possessionLosses: {
    label: "Izgubljene lopte",
    shortLabel: "IL",
    group: "Odbrana",
    description: "Izgubljene lopte",
    input: "integer",
    order: 16,
  },
  saves: {
    label: "Odbrane",
    shortLabel: "OD",
    group: "Golman",
    description: "Odbrane golmana",
    input: "integer",
    order: 17,
  },
  goalsConceded: {
    label: "Primljeni golovi",
    shortLabel: "PG",
    group: "Golman",
    description: "Primljeni golovi",
    input: "integer",
    order: 18,
  },
  cleanSheet: {
    label: "Sačuvana mreža",
    shortLabel: "SM",
    group: "Golman",
    description: "Sačuvana mreža",
    input: "boolean",
    order: 19,
  },
  penaltySaves: {
    label: "Odbrane penala",
    shortLabel: "OP",
    group: "Golman",
    description: "Odbrane penala",
    input: "integer",
    order: 20,
  },
};

export function isKnownStatisticField(
  value: string,
): value is StatisticFieldCode {
  return value in statisticFieldRegistry;
}

export function parseNullableInteger(value: string): number | null | undefined {
  if (value.trim() === "") return null;
  if (!/^\d+$/.test(value)) return undefined;
  const parsed = Number(value);
  return Number.isSafeInteger(parsed) ? parsed : undefined;
}

export function formatNullableInteger(value: number | null): string {
  return value === null ? "" : String(value);
}

export function validateStatisticRelationship(
  values: Record<string, string>,
): string | null {
  const pairs: Array<[string, string, string]> = [
    [
      "shotsOnTarget",
      "shots",
      "Udarci u okvir ne mogu biti veći od ukupnih udaraca.",
    ],
    [
      "passesCompleted",
      "passesAttempted",
      "Uspješna dodavanja ne mogu biti veća od pokušaja.",
    ],
    [
      "duelsWon",
      "duelsAttempted",
      "Osvojeni dueli ne mogu biti veći od ukupnih duela.",
    ],
  ];
  for (const [part, total, message] of pairs) {
    const partValue = parseNullableInteger(values[part] ?? "");
    const totalValue = parseNullableInteger(values[total] ?? "");
    if (
      typeof partValue === "number" &&
      typeof totalValue === "number" &&
      partValue > totalValue
    )
      return message;
  }
  return null;
}

export function isComplete(values: Record<string, string>, fields: string[]) {
  return fields.every((field) => {
    if (field === "cleanSheet")
      return values[field] === "true" || values[field] === "false";
    return parseNullableInteger(values[field] ?? "") !== null;
  });
}

export function createStatisticsPayload(
  values: StatisticsFormValues,
  response: MatchReportStatisticsResponse,
): SaveMatchReportStatisticsRequest {
  const playerFields =
    response.enabledPlayerFields as PlayerStatisticFieldCode[];
  const goalkeeperFields =
    response.enabledGoalkeeperFields as GoalkeeperStatisticFieldCode[];
  return {
    playerStatistics: values.players.map((row) => ({
      playerMatchAppearanceId: row.playerMatchAppearanceId,
      ...Object.fromEntries(
        playerFields.map((field) => [
          field,
          parseNullableInteger(row.values[field] ?? "") ?? null,
        ]),
      ),
    })) as SaveMatchReportStatisticsRequest["playerStatistics"],
    goalkeeperStatistics: values.goalkeepers.map((row) => ({
      playerMatchAppearanceId: row.playerMatchAppearanceId,
      ...Object.fromEntries(
        goalkeeperFields.map((field) => [
          field,
          field === "cleanSheet"
            ? row.values[field] === ""
              ? null
              : row.values[field] === "true"
            : (parseNullableInteger(row.values[field] ?? "") ?? null),
        ]),
      ),
    })) as SaveMatchReportStatisticsRequest["goalkeeperStatistics"],
  };
}

export interface StatisticsFormRow {
  playerMatchAppearanceId: string;
  values: Record<string, string>;
}
export interface StatisticsFormValues {
  players: StatisticsFormRow[];
  goalkeepers: StatisticsFormRow[];
}

export function createStatisticsFormValues(
  response: MatchReportStatisticsResponse,
): StatisticsFormValues {
  const playerByAppearance = new Map(
    response.playerStatistics.map((row) => [row.playerMatchAppearanceId, row]),
  );
  return {
    players: response.appearances.map((appearance) => ({
      playerMatchAppearanceId: appearance.playerMatchAppearanceId,
      values: Object.fromEntries(
        response.enabledPlayerFields.map((field) => [
          field,
          formatNullableInteger(
            (playerByAppearance.get(appearance.playerMatchAppearanceId)?.[
              field as PlayerStatisticFieldCode
            ] ?? null) as number | null,
          ),
        ]),
      ),
    })),
    goalkeepers: response.goalkeeperStatistics.map((row) => ({
      playerMatchAppearanceId: row.playerMatchAppearanceId,
      values: Object.fromEntries(
        response.enabledGoalkeeperFields.map((field) => [
          field,
          field === "cleanSheet"
            ? row.cleanSheet === null
              ? ""
              : String(row.cleanSheet)
            : formatNullableInteger(
                row[
                  field as Exclude<GoalkeeperStatisticFieldCode, "cleanSheet">
                ] as number | null,
              ),
        ]),
      ),
    })),
  };
}
