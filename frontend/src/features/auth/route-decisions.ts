import { routePaths } from "@/app/route-paths";

const unsafeReturnPaths = new Set<string>([
  routePaths.signIn,
  routePaths.forgotPassword,
  routePaths.resetPassword,
  routePaths.changePassword,
]);

export function resolveSafeReturnPath(
  candidate: unknown,
  fallbackPath = routePaths.dashboard,
): string {
  if (
    typeof candidate !== "string" ||
    !candidate.startsWith("/") ||
    candidate.startsWith("//") ||
    candidate.includes("\\")
  ) {
    return fallbackPath;
  }

  const pathname = candidate.split(/[?#]/, 1)[0];

  if (unsafeReturnPaths.has(pathname)) {
    return fallbackPath;
  }

  return candidate;
}
