import { isApiError } from "@/lib/api/api-client";

export function getSignInErrorMessage(error: unknown): string {
  if (isApiError(error)) {
    if (error.code === "invalid_credentials") {
      return "E-mail adresa ili lozinka nisu ispravni.";
    }

    if (error.code === "account_unavailable") {
      return "Ovaj korisnički račun trenutno nije dostupan.";
    }

    if (error.status === 400) {
      return "Sigurnosna provjera nije uspjela. Osvježite stranicu i pokušajte ponovo.";
    }
  }

  return "Prijava trenutno nije uspjela. Pokušajte ponovo.";
}

export function getChangePasswordErrorMessage(error: unknown): string {
  if (isApiError(error)) {
    if (error.code === "password_validation_failed") {
      return "Nova lozinka ne ispunjava sigurnosna pravila sistema.";
    }

    if (error.code === "account_unavailable") {
      return "Ovaj korisnički račun trenutno nije dostupan.";
    }
  }

  return "Promjena lozinke trenutno nije uspjela. Pokušajte ponovo.";
}
