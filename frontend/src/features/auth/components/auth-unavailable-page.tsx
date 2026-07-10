import { Link } from "react-router-dom";

import { routePaths } from "@/app/route-paths";
import { EmptyState } from "@/components/common/empty-state";
import { buttonVariants } from "@/components/ui/button";

import { AuthPageShell } from "./auth-page-shell";

interface AuthUnavailablePageProps {
  title: string;
  description: string;
}

export function AuthUnavailablePage({
  title,
  description,
}: AuthUnavailablePageProps) {
  return (
    <AuthPageShell title={title} description={description}>
      <EmptyState
        title="Ovaj tok još nije dostupan"
        description="Reset lozinke će biti omogućen tek nakon implementacije odgovarajućih backend endpointa i sigurnosnih pravila."
        action={
          <Link to={routePaths.signIn} className={buttonVariants()}>
            Nazad na prijavu
          </Link>
        }
      />
    </AuthPageShell>
  );
}
