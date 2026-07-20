export interface FrontendEnv {
  apiBaseUrl: string | null;
}

function normalizeApiBaseUrl(value: string | undefined): string | null {
  const trimmedValue = value?.trim();

  if (!trimmedValue) {
    return null;
  }

  const normalizedValue = trimmedValue.replace(/\/+$/, "");

  if (normalizedValue.startsWith("/")) {
    if (normalizedValue.startsWith("//")) {
      throw new Error(
        "VITE_API_BASE_URL ne smije koristiti protocol-relative URL.",
      );
    }

    return normalizedValue;
  }

  let apiUrl: URL;
  try {
    apiUrl = new URL(normalizedValue);
  } catch {
    throw new Error(
      "VITE_API_BASE_URL mora biti relativna API putanja ili apsolutni URL.",
    );
  }

  const isProduction = import.meta.env.PROD;
  if (
    (isProduction && apiUrl.protocol !== "https:") ||
    apiUrl.username ||
    apiUrl.password ||
    apiUrl.search ||
    apiUrl.hash
  ) {
    throw new Error(
      "VITE_API_BASE_URL u produkciji mora biti HTTPS URL bez vjerodajnica, upita i fragmenta.",
    );
  }

  return normalizedValue;
}

const frontendEnv: FrontendEnv = {
  apiBaseUrl: normalizeApiBaseUrl(import.meta.env.VITE_API_BASE_URL),
};

export function getFrontendEnv(): FrontendEnv {
  return frontendEnv;
}

export function getRequiredApiBaseUrl(): string {
  if (!frontendEnv.apiBaseUrl) {
    throw new Error(
      "Nedostaje VITE_API_BASE_URL. Dodaj URL backend API-ja u frontend/.env.local prije poziva prema serveru.",
    );
  }

  return frontendEnv.apiBaseUrl;
}
