import { zodResolver } from "@hookform/resolvers/zod";
import { Eye, EyeOff } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { z } from "zod";

import { routePaths } from "@/app/route-paths";
import { FormErrorSummary } from "@/components/common/form-error-summary";
import { FormFieldMessage } from "@/components/common/form-field-message";
import { Button } from "@/components/ui/button";
import { getChangePasswordErrorMessage } from "@/features/auth/auth-error-messages";
import { AuthPageShell } from "@/features/auth/components/auth-page-shell";
import { useChangePassword } from "@/features/auth/hooks/use-auth-mutations";
import { useLogout } from "@/features/auth/hooks/use-logout";
import { isApiError } from "@/lib/api/api-client";

const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Unesite trenutnu lozinku."),
    newPassword: z.string().min(1, "Unesite novu lozinku."),
    confirmPassword: z.string().min(1, "Potvrdite novu lozinku."),
  })
  .refine((values) => values.newPassword === values.confirmPassword, {
    message: "Potvrda nove lozinke se ne podudara.",
    path: ["confirmPassword"],
  });

type ChangePasswordValues = z.infer<typeof changePasswordSchema>;

export function ChangePasswordPage() {
  const navigate = useNavigate();
  const changePasswordMutation = useChangePassword();
  const logoutMutation = useLogout();
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [visiblePasswords, setVisiblePasswords] = useState<
    Partial<Record<keyof ChangePasswordValues, boolean>>
  >({});
  const form = useForm<ChangePasswordValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: "",
      newPassword: "",
      confirmPassword: "",
    },
  });

  async function onSubmit(values: ChangePasswordValues) {
    setSubmitError(null);

    try {
      const session = await changePasswordMutation.mutateAsync(values);

      if (session.user?.mustChangePassword !== false) {
        setSubmitError("Promjena lozinke nije potvrđena. Pokušajte ponovo.");
        return;
      }

      form.reset();
      navigate(routePaths.dashboard, { replace: true });
    } catch (error) {
      if (isApiError(error) && error.code === "current_password_invalid") {
        form.setError(
          "currentPassword",
          { message: "Trenutna lozinka nije ispravna." },
          { shouldFocus: true },
        );
      }

      setSubmitError(getChangePasswordErrorMessage(error));
    }
  }

  async function onLogout() {
    setSubmitError(null);
    try {
      await logoutMutation.mutateAsync();
      navigate(routePaths.signIn, { replace: true });
    } catch {
      setSubmitError("Odjava trenutno nije uspjela. Pokušajte ponovo.");
    }
  }

  return (
    <AuthPageShell
      title="Promjena privremene lozinke"
      description="Prije nastavka morate promijeniti privremenu lozinku."
    >
      <form
        className="flex flex-col gap-5"
        onSubmit={form.handleSubmit(onSubmit)}
        noValidate
      >
        <p className="text-muted-foreground text-sm leading-6">
          Nova lozinka mora ispunjavati sigurnosna pravila sistema.
        </p>
        <FormErrorSummary errors={submitError} />
        {(
          [
            ["currentPassword", "Trenutna lozinka", "current-password"],
            ["newPassword", "Nova lozinka", "new-password"],
            ["confirmPassword", "Potvrdi novu lozinku", "new-password"],
          ] as const
        ).map(([name, label, autoComplete]) => {
          const error = form.formState.errors[name];
          const id = `change-password-${name}`;
          const isPasswordVisible = visiblePasswords[name] === true;
          const isPending =
            changePasswordMutation.isPending || logoutMutation.isPending;
          return (
            <div className="flex flex-col gap-2" key={name}>
              <label
                className="text-foreground text-sm font-medium"
                htmlFor={id}
              >
                {label}
              </label>
              <div className="relative">
                <input
                  id={id}
                  type={isPasswordVisible ? "text" : "password"}
                  autoComplete={autoComplete}
                  className="border-input bg-background text-foreground focus-visible:border-ring focus-visible:ring-ring/50 h-11 w-full rounded-lg border px-3 pr-11 text-sm outline-none focus-visible:ring-2"
                  aria-invalid={Boolean(error)}
                  aria-describedby={error ? `${id}-error` : undefined}
                  disabled={isPending}
                  {...form.register(name)}
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  pressMotion={false}
                  className="absolute top-1/2 right-1 -translate-y-1/2"
                  aria-label={
                    isPasswordVisible ? "Sakrij lozinku" : "Prikaži lozinku"
                  }
                  aria-pressed={isPasswordVisible}
                  disabled={isPending}
                  onClick={() =>
                    setVisiblePasswords((visible) => ({
                      ...visible,
                      [name]: !isPasswordVisible,
                    }))
                  }
                >
                  {isPasswordVisible ? (
                    <EyeOff aria-hidden="true" />
                  ) : (
                    <Eye aria-hidden="true" />
                  )}
                </Button>
              </div>
              <div id={`${id}-error`}>
                <FormFieldMessage error={error} />
              </div>
            </div>
          );
        })}
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <Button
            type="submit"
            disabled={
              changePasswordMutation.isPending || logoutMutation.isPending
            }
          >
            {changePasswordMutation.isPending
              ? "Čuvanje..."
              : "Sačuvaj novu lozinku"}
          </Button>
          <Button
            type="button"
            variant="outline"
            disabled={
              changePasswordMutation.isPending || logoutMutation.isPending
            }
            onClick={() => void onLogout()}
          >
            {logoutMutation.isPending ? "Odjava..." : "Odjava"}
          </Button>
        </div>
      </form>
    </AuthPageShell>
  );
}
