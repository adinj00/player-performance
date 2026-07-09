export interface FrontendEnv {
  apiBaseUrl: string | null;
}

function normalizeApiBaseUrl(value: string | undefined): string | null {
  const trimmedValue = value?.trim();

  if (!trimmedValue) {
    return null;
  }

  return trimmedValue.replace(/\/+$/, "");
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
