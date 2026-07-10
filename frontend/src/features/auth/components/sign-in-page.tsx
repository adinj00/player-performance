import { zodResolver } from "@hookform/resolvers/zod";
import { Eye, EyeOff } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { z } from "zod";

import { routePaths } from "@/app/route-paths";
import { FormErrorSummary } from "@/components/common/form-error-summary";
import { FormFieldMessage } from "@/components/common/form-field-message";
import { Button } from "@/components/ui/button";
import { getSignInErrorMessage } from "@/features/auth/auth-error-messages";
import { AuthPageShell } from "@/features/auth/components/auth-page-shell";
import { useLogin } from "@/features/auth/hooks/use-auth-mutations";
import { resolveSafeReturnPath } from "@/features/auth/route-decisions";

const signInSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, "Unesite e-mail adresu.")
    .email("Unesite ispravnu e-mail adresu."),
  password: z.string().min(1, "Unesite lozinku."),
});

type SignInValues = z.infer<typeof signInSchema>;

export function SignInPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const loginMutation = useLogin();
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isPasswordVisible, setIsPasswordVisible] = useState(false);
  const form = useForm<SignInValues>({
    resolver: zodResolver(signInSchema),
    defaultValues: { email: "", password: "" },
  });

  async function onSubmit(values: SignInValues) {
    setSubmitError(null);

    try {
      const session = await loginMutation.mutateAsync({
        email: values.email.trim(),
        password: values.password,
      });

      form.reset({ email: values.email.trim(), password: "" });
      navigate(
        session.user?.mustChangePassword
          ? routePaths.changePassword
          : resolveSafeReturnPath(location.state?.from),
        { replace: true },
      );
    } catch (error) {
      form.setValue("password", "");
      setSubmitError(getSignInErrorMessage(error));
    }
  }

  return (
    <AuthPageShell
      title="Prijava za osoblje"
      description="Prijavite se svojim službenim korisničkim podacima."
    >
      <form
        className="flex flex-col gap-5"
        onSubmit={form.handleSubmit(onSubmit)}
        noValidate
      >
        <FormErrorSummary errors={submitError} />

        <div className="flex flex-col gap-2">
          <label
            className="text-foreground text-sm font-medium"
            htmlFor="sign-in-email"
          >
            E-mail adresa
          </label>
          <input
            id="sign-in-email"
            type="email"
            autoComplete="username"
            className="border-input bg-background text-foreground focus-visible:border-ring focus-visible:ring-ring/50 h-11 rounded-lg border px-3 text-sm outline-none focus-visible:ring-2"
            aria-invalid={Boolean(form.formState.errors.email)}
            aria-describedby={
              form.formState.errors.email ? "sign-in-email-error" : undefined
            }
            disabled={loginMutation.isPending}
            {...form.register("email")}
          />
          <div id="sign-in-email-error">
            <FormFieldMessage error={form.formState.errors.email} />
          </div>
        </div>

        <div className="flex flex-col gap-2">
          <label
            className="text-foreground text-sm font-medium"
            htmlFor="sign-in-password"
          >
            Lozinka
          </label>
          <div className="relative">
            <input
              id="sign-in-password"
              type={isPasswordVisible ? "text" : "password"}
              autoComplete="current-password"
              className="border-input bg-background text-foreground focus-visible:border-ring focus-visible:ring-ring/50 h-11 w-full rounded-lg border px-3 pr-11 text-sm outline-none focus-visible:ring-2"
              aria-invalid={Boolean(form.formState.errors.password)}
              aria-describedby={
                form.formState.errors.password
                  ? "sign-in-password-error"
                  : undefined
              }
              disabled={loginMutation.isPending}
              {...form.register("password")}
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
              disabled={loginMutation.isPending}
              onClick={() => setIsPasswordVisible((visible) => !visible)}
            >
              {isPasswordVisible ? (
                <EyeOff aria-hidden="true" />
              ) : (
                <Eye aria-hidden="true" />
              )}
            </Button>
          </div>
          <div id="sign-in-password-error">
            <FormFieldMessage error={form.formState.errors.password} />
          </div>
        </div>

        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <Button type="submit" disabled={loginMutation.isPending}>
            {loginMutation.isPending ? "Prijava..." : "Prijavi se"}
          </Button>
          <Link
            to={routePaths.forgotPassword}
            className="text-primary text-sm underline-offset-4 hover:underline"
          >
            Zaboravljena lozinka
          </Link>
        </div>
      </form>
    </AuthPageShell>
  );
}
