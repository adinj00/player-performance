import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { useState } from "react";
import { toast } from "sonner";

import { routePaths } from "@/app/route-paths";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
} from "@/components/ui/empty";
import { Skeleton } from "@/components/ui/skeleton";
import { mediaApi } from "./api";
import { useSession } from "@/features/auth/hooks/use-session";
import type { MediaLinkTargetType } from "./types";
import { categoryLabels, sourceLabels } from "./utils";

export function MediaLinksSection({
  targetId,
  targetType,
  title = "Mediji",
}: {
  targetId: string;
  targetType: MediaLinkTargetType;
  title?: string;
}) {
  const { user } = useSession();
  const queryClient = useQueryClient();
  const [mediaIdPendingUnlink, setMediaIdPendingUnlink] = useState<
    string | null
  >(null);
  const queryKey = [
    targetType === "MATCH"
      ? "matchLinkedMedia"
      : targetType === "MATCH_REPORT"
        ? "matchReportLinkedMedia"
        : "playerLinkedMedia",
    targetId,
  ];
  const media = useQuery({
    queryKey,
    queryFn: () => mediaApi.linked(targetType, targetId),
    retry: false,
  });
  const unlink = useMutation({
    mutationFn: (mediaId: string) =>
      mediaApi.unlink(mediaId, targetType, targetId),
    onSuccess: () => {
      setMediaIdPendingUnlink(null);
      void queryClient.invalidateQueries({ queryKey });
      toast.success("Veza je uklonjena. Medij ostaje sačuvan.");
    },
  });
  return (
    <section className="flex flex-col gap-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="font-heading text-xl">{title}</h2>
        <Button
          nativeButton={false}
          variant="outline"
          render={
            <Link
              to={`${routePaths.media}?attachTargetType=${targetType}&attachTargetId=${targetId}`}
            />
          }
        >
          Otvori medijateku
        </Button>
      </div>
      {media.isLoading ? (
        <Skeleton className="h-20 w-full" />
      ) : media.isError ? (
        <p className="text-muted-foreground text-sm">
          Povezane medije nije moguće učitati.
        </p>
      ) : media.data?.items.length ? (
        <ul className="grid gap-2 sm:grid-cols-2">
          {media.data.items.map((item) => (
            <li className="rounded-xl border p-3" key={item.id}>
              <Link
                className="font-medium underline-offset-4 hover:underline"
                to={`${routePaths.media}?media=${item.id}`}
              >
                {item.title}
              </Link>
              <p className="text-muted-foreground text-sm">
                {categoryLabels[item.category]} ·{" "}
                {sourceLabels[item.sourceType]}
              </p>
              {(user?.primaryRole === "ADMIN" ||
                user?.primaryRole === "DATA_OPERATOR") && (
                <Button
                  className="mt-2"
                  disabled={unlink.isPending}
                  onClick={() => setMediaIdPendingUnlink(item.id)}
                  size="sm"
                  variant="outline"
                >
                  Ukloni vezu
                </Button>
              )}
            </li>
          ))}
        </ul>
      ) : (
        <Empty>
          <EmptyHeader>
            <EmptyTitle>Nema povezanih medija</EmptyTitle>
            <EmptyDescription>
              Medije možete povezati iz medijateke.
            </EmptyDescription>
          </EmptyHeader>
        </Empty>
      )}
      <Dialog
        open={mediaIdPendingUnlink !== null}
        onOpenChange={(open) => {
          if (!open && !unlink.isPending) setMediaIdPendingUnlink(null);
        }}
      >
        <DialogContent showCloseButton={!unlink.isPending}>
          <DialogHeader>
            <DialogTitle>Ukloni vezu?</DialogTitle>
            <DialogDescription>
              Medij i fajl ostaju sačuvani. Možete ih ponovo povezati kasnije.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              disabled={unlink.isPending}
              variant="outline"
              onClick={() => setMediaIdPendingUnlink(null)}
            >
              Odustani
            </Button>
            <Button
              disabled={unlink.isPending || mediaIdPendingUnlink === null}
              variant="destructive"
              onClick={() => {
                if (mediaIdPendingUnlink) unlink.mutate(mediaIdPendingUnlink);
              }}
            >
              Ukloni vezu
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </section>
  );
}
