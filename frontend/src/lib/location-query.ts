type LocationParts = Pick<Location, "hash" | "pathname" | "search">;

export function withLocationQuery(
  location: LocationParts,
  updates: Record<string, string | null>,
): string {
  const params = new URLSearchParams(location.search);

  for (const [key, value] of Object.entries(updates)) {
    if (value === null) params.delete(key);
    else params.set(key, value);
  }

  const query = params.toString();
  return `${location.pathname}${query ? `?${query}` : ""}${location.hash}`;
}
