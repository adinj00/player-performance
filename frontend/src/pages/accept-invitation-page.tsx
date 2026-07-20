import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useSearchParams } from "react-router-dom";
import { z } from "zod";
import { routePaths } from "@/app/route-paths";
import { Button } from "@/components/ui/button";
import { AuthPageShell } from "@/features/auth/components/auth-page-shell";
import { useSession } from "@/features/auth/hooks/use-session";
import { acceptInvitation } from "@/features/users/api";

const schema = z
  .object({
    password: z.string().min(8, "Lozinka mora imati najmanje 8 znakova."),
    confirmPassword: z.string(),
  })
  .refine((v) => v.password === v.confirmPassword, {
    path: ["confirmPassword"],
    message: "Potvrda lozinke se ne podudara.",
  });
type Values = z.infer<typeof schema>;
export function AcceptInvitationPage() {
  const [params, setParams] = useSearchParams();
  const { isAuthenticated, isLoading } = useSession();
  const [credentials, setCredentials] = useState<{
    email: string;
    token: string;
  } | null>(() => {
    const email = params.get("email");
    const token = params.get("token");
    return email && token ? { email, token } : null;
  });
  const [done, setDone] = useState(false);
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: { password: "", confirmPassword: "" },
  });
  useEffect(() => {
    if (credentials) setParams({}, { replace: true });
  }, [credentials, setParams]);
  const mutation = useMutation({
    mutationFn: (v: Values) =>
      credentials
        ? acceptInvitation({ ...credentials, ...v })
        : Promise.reject(new Error("missing")),
    retry: false,
    onSuccess: () => {
      setCredentials(null);
      form.reset();
      setDone(true);
    },
  });
  if (isLoading) return null;
  return (
    <AuthPageShell
      title="Postavljanje lozinke"
      description="Dovršite aktivaciju pozvanog korisničkog računa."
    >
      {isAuthenticated ? (
        <p className="text-muted-foreground">
          Za postavljanje poziva prvo se odjavite iz trenutnog računa.
        </p>
      ) : done ? (
        <div className="flex flex-col gap-4">
          <p>Lozinka je postavljena. Sada se možete prijaviti.</p>
          <Button nativeButton={false} render={<Link to={routePaths.signIn} />}>
            Nastavi na prijavu
          </Button>
        </div>
      ) : !credentials ? (
        <p className="text-muted-foreground">
          Ovaj link nije važeći. Zatražite novi poziv od administratora.
        </p>
      ) : (
        <form
          className="flex flex-col gap-4"
          onSubmit={form.handleSubmit((v) => mutation.mutate(v))}
        >
          <label className="flex flex-col gap-1 text-sm">
            Nova lozinka
            <input
              type="password"
              autoComplete="new-password"
              className="border-input bg-background h-10 rounded-md border px-3"
              {...form.register("password")}
            />
            {form.formState.errors.password ? (
              <span className="text-destructive">
                {form.formState.errors.password.message}
              </span>
            ) : null}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Potvrdite lozinku
            <input
              type="password"
              autoComplete="new-password"
              className="border-input bg-background h-10 rounded-md border px-3"
              {...form.register("confirmPassword")}
            />
            {form.formState.errors.confirmPassword ? (
              <span className="text-destructive">
                {form.formState.errors.confirmPassword.message}
              </span>
            ) : null}
          </label>
          {mutation.isError ? (
            <p className="text-destructive text-sm">
              Link nije važeći ili je istekao. Zatražite novi poziv od
              administratora.
            </p>
          ) : null}
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Postavljanje..." : "Postavi lozinku"}
          </Button>
        </form>
      )}
    </AuthPageShell>
  );
}
