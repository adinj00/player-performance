import { useEffect, useRef } from "react";
import { useLocation } from "react-router-dom";

import { getRouteDefinition } from "@/app/route-paths";

/** Moves focus only for route changes, never for query-string filter changes. */
export function RouteFocusManager() {
  const { pathname } = useLocation();
  const previousPathname = useRef<string | null>(null);

  useEffect(() => {
    const changedPathname = previousPathname.current !== null;
    previousPathname.current = pathname;
    document.title = `${getRouteDefinition(pathname)?.title ?? "Stranica nije pronađena"} | FK Velež Mostar`;

    if (!changedPathname) return;

    const frame = window.requestAnimationFrame(() => {
      const target =
        document.querySelector<HTMLElement>("[data-route-heading]") ??
        document.querySelector<HTMLElement>("main");
      target?.focus({ preventScroll: true });
    });

    return () => window.cancelAnimationFrame(frame);
  }, [pathname]);

  return null;
}
