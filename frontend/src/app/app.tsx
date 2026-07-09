import { AppProviders } from "@/app/providers";
import { AppRoutes } from "@/app/app-routes";

export function App() {
  return (
    <AppProviders>
      <AppRoutes />
    </AppProviders>
  );
}
