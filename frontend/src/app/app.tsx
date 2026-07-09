import { AppEmptyState } from "@/components/common/app-empty-state";
import { AppShell } from "@/components/layout/app-shell";

export function App() {
  return (
    <AppShell>
      <AppEmptyState />
    </AppShell>
  );
}
