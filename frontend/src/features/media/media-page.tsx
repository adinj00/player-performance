import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  parseAsBoolean,
  parseAsInteger,
  parseAsString,
  useQueryStates,
} from "nuqs";
import {
  Archive,
  ArchiveRestore,
  CalendarPlus,
  FilePenLine,
  FilePlus,
  Link2,
  MoreHorizontal,
  Upload,
  UserRoundPlus,
} from "lucide-react";
import { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { toast } from "sonner";
import {
  useReactTable,
  getCoreRowModel,
  flexRender,
  type ColumnDef,
} from "@tanstack/react-table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
} from "@/components/ui/empty";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { Separator } from "@/components/ui/separator";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";
import { Progress } from "@/components/ui/progress";
import { PageHeader } from "@/components/common/page-header";
import { FilterSelect } from "@/components/common/filter-select";
import { ErrorState } from "@/components/common/error-state";
import { LoadingState } from "@/components/common/loading-state";
import { useSession } from "@/features/auth/hooks/use-session";
import { isApiError } from "@/lib/api/api-client";
import { withLocationQuery } from "@/lib/location-query";
import { settingsApi } from "@/features/settings/api";
import { mediaApi } from "./api";
import type { MediaCategory, MediaItem, MediaLinkTargetType } from "./types";
import {
  categoryLabels,
  contentUrl,
  formatBytes,
  safeHost,
  sourceLabels,
} from "./utils";

const categories: MediaCategory[] = ["VIDEO", "IMAGE", "DOCUMENT", "OTHER"];
function canMutate(role: string | null | undefined) {
  return role === "ADMIN" || role === "DATA_OPERATOR";
}

export function MediaPage() {
  const location = useLocation();
  const { user } = useSession();
  const client = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);
  const [uploadOpen, setUploadOpen] = useState(false);
  const [state, setState] = useQueryStates({
    search: parseAsString,
    teamId: parseAsString,
    source: parseAsString,
    category: parseAsString,
    archived: parseAsBoolean.withDefault(false),
    page: parseAsInteger.withDefault(1),
    media: parseAsString,
    attachTargetType: parseAsString,
    attachTargetId: parseAsString,
  });
  const media = useQuery({
    queryKey: ["mediaList", state],
    queryFn: () =>
      mediaApi.list({
        search: state.search,
        teamId: state.teamId,
        sourceType: state.source,
        category: state.category,
        includeArchived: state.archived,
        page: state.page,
        pageSize: 25,
      }),
  });
  const teams = useQuery({
    queryKey: ["media", "teams"],
    queryFn: () => settingsApi.listTeams(false),
    retry: false,
  });
  const mutationTeams = (teams.data ?? []).filter(
    (team) =>
      team.status === "ACTIVE" &&
      (user?.primaryRole === "ADMIN" ||
        (user?.primaryRole === "DATA_OPERATOR" &&
          (user.teamScope.type === "ALL" ||
            user.teamScope.selectedTeamIds.includes(team.id)))),
  );
  const selected = useQuery({
    queryKey: ["mediaDetail", state.media],
    queryFn: () => mediaApi.get(state.media!),
    enabled: !!state.media,
    retry: false,
  });
  const change = (v: Partial<typeof state>) =>
    void setState({ ...v, page: v.page === undefined ? 1 : v.page });
  const mediaDetailUrl = (mediaId: string) => {
    return withLocationQuery(location, { media: mediaId });
  };
  const clearFilters = () =>
    void setState({
      search: null,
      teamId: null,
      source: null,
      category: null,
      archived: false,
      page: 1,
    });
  const hasFilters = Boolean(
    state.search ||
    state.teamId ||
    state.source ||
    state.category ||
    state.archived,
  );
  const created = async (item: MediaItem) => {
    if (state.attachTargetType && state.attachTargetId) {
      try {
        await mediaApi.link(
          item.id,
          state.attachTargetType as MediaLinkTargetType,
          state.attachTargetId,
        );
        toast.success("Medij je kreiran i povezan.");
      } catch {
        toast.error(
          "Medij je kreiran, ali povezivanje nije uspjelo. Možete ga povezati iz detalja medija.",
        );
      }
    }
    setCreateOpen(false);
    setUploadOpen(false);
    change({ media: item.id });
    void client.invalidateQueries({ queryKey: ["mediaList"] });
  };
  const columns: ColumnDef<MediaItem>[] = [
    {
      header: "Naziv",
      accessorKey: "title",
      cell: ({ row }) => (
        <Link
          className="font-medium hover:underline"
          to={mediaDetailUrl(row.original.id)}
        >
          {row.original.title}
        </Link>
      ),
    },
    { header: "Selekcija", accessorKey: "teamName" },
    {
      header: "Kategorija",
      cell: ({ row }) => (
        <Badge variant="secondary">
          {categoryLabels[row.original.category]}
        </Badge>
      ),
    },
    {
      header: "Izvor",
      cell: ({ row }) => <span>{sourceLabels[row.original.sourceType]}</span>,
    },
    {
      header: "Datoteka / domena",
      cell: ({ row }) =>
        row.original.sourceType === "UPLOADED_FILE"
          ? (row.original.source.originalFileName ?? "—")
          : safeHost(row.original.source.url),
    },
    {
      header: "Veličina",
      cell: ({ row }) => (
        <span title={row.original.source.sizeBytes?.toString()}>
          {formatBytes(row.original.source.sizeBytes)}
        </span>
      ),
    },
    {
      id: "actions",
      header: "",
      cell: ({ row }) => (
        <DropdownMenu>
          <DropdownMenuTrigger
            render={
              <Button aria-label="Radnje za medij" size="icon" variant="ghost">
                <MoreHorizontal />
              </Button>
            }
          />
          <DropdownMenuContent align="end">
            <DropdownMenuGroup>
              <DropdownMenuItem
                onClick={() => change({ media: row.original.id })}
              >
                Otvori
              </DropdownMenuItem>
            </DropdownMenuGroup>
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    },
  ];
  // TanStack Table creates mutable instances.
  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({
    data: media.data?.items ?? [],
    columns,
    getCoreRowModel: getCoreRowModel(),
  });
  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Medijateka"
        description="Učitani fajlovi i vanjske reference za selekcije."
        actions={
          canMutate(user?.primaryRole) ? (
            <div className="flex flex-wrap gap-2">
              <Button variant="outline" onClick={() => setUploadOpen(true)}>
                <Upload data-icon="inline-start" />
                Učitaj fajl
              </Button>
              <Button onClick={() => setCreateOpen(true)}>
                <FilePlus data-icon="inline-start" />
                Dodaj vanjsku referencu
              </Button>
            </div>
          ) : null
        }
      />
      <section className="border-border bg-card grid gap-3 rounded-xl border p-4 md:flex md:flex-wrap">
        <Input
          aria-label="Pretraži medije"
          placeholder="Pretraži"
          value={state.search ?? ""}
          onChange={(e) => change({ search: e.target.value || null })}
          className="w-full md:w-64"
        />
        <FilterSelect
          label="Selekcija"
          emptyLabel="Sve selekcije"
          value={state.teamId}
          options={Object.fromEntries(
            (teams.data ?? [])
              .filter((team) => team.status === "ACTIVE")
              .map((team) => [team.id, team.name]),
          )}
          className="w-full md:w-48"
          onChange={(teamId) => change({ teamId })}
        />
        <FilterSelect
          label="Kategorija"
          emptyLabel="Sve kategorije"
          value={state.category}
          options={categoryLabels}
          className="w-full md:w-40"
          onChange={(category) => change({ category })}
        />
        <FilterSelect
          label="Izvor"
          emptyLabel="Svi izvori"
          value={state.source}
          options={sourceLabels}
          className="w-full md:w-48"
          onChange={(source) => change({ source })}
        />
        {user?.primaryRole === "ADMIN" ? (
          <label className="flex shrink-0 items-center gap-2 text-sm">
            <Checkbox
              id="media-include-archived"
              checked={state.archived}
              onCheckedChange={(checked) =>
                change({ archived: checked === true })
              }
            />
            <span>Prikaži arhivirane</span>
          </label>
        ) : null}
      </section>
      {media.isLoading ? (
        <LoadingState />
      ) : media.isError ? (
        <ErrorState
          description="Medije nije moguće učitati."
          action={
            <Button onClick={() => void media.refetch()}>Pokušaj ponovo</Button>
          }
        />
      ) : media.data?.items.length === 0 ? (
        <Empty>
          <EmptyHeader>
            <EmptyTitle>
              {hasFilters
                ? "Nema rezultata za odabrane filtere"
                : "Nema medija"}
            </EmptyTitle>
            <EmptyDescription>
              {hasFilters
                ? "Promijenite ili poništite filtere."
                : "Dodajte prvi medij u medijateku."}
            </EmptyDescription>
          </EmptyHeader>
          <EmptyContent>
            {hasFilters ? (
              <Button onClick={clearFilters} variant="outline">
                Poništi filtere
              </Button>
            ) : null}
          </EmptyContent>
        </Empty>
      ) : (
        <>
          <div className="overflow-x-auto rounded-xl border">
            <Table>
              <TableHeader>
                <TableRow>
                  {table.getHeaderGroups()[0].headers.map((h) => (
                    <TableHead key={h.id}>
                      {flexRender(h.column.columnDef.header, h.getContext())}
                    </TableHead>
                  ))}
                </TableRow>
              </TableHeader>
              <TableBody>
                {table.getRowModel().rows.map((row) => (
                  <TableRow key={row.id}>
                    {row.getVisibleCells().map((cell) => (
                      <TableCell key={cell.id}>
                        {flexRender(
                          cell.column.columnDef.cell,
                          cell.getContext(),
                        )}
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-muted-foreground text-sm">
              Ukupno: {media.data?.totalCount}
            </span>
            <div className="flex gap-2">
              <Button
                variant="outline"
                disabled={state.page <= 1}
                onClick={() => change({ page: state.page - 1 })}
              >
                Prethodna
              </Button>
              <Button
                variant="outline"
                disabled={state.page >= (media.data?.totalPages ?? 1)}
                onClick={() => change({ page: state.page + 1 })}
              >
                Sljedeća
              </Button>
            </div>
          </div>
        </>
      )}
      <MediaSheet
        item={selected.data}
        open={!!state.media}
        onClose={() => change({ media: null })}
        isAdmin={user?.primaryRole === "ADMIN"}
        canManage={canMutate(user?.primaryRole)}
        onChanged={() => {
          void client.invalidateQueries({ queryKey: ["mediaList"] });
          void client.invalidateQueries({
            queryKey: ["mediaDetail", state.media],
          });
        }}
      />
      {createOpen ? (
        <ExternalCreateDialog
          teams={mutationTeams}
          onClose={() => setCreateOpen(false)}
          onCreated={created}
        />
      ) : null}
      {uploadOpen ? (
        <UploadDialog
          teams={mutationTeams}
          onClose={() => setUploadOpen(false)}
          onCreated={created}
        />
      ) : null}
    </div>
  );
}

function MediaSheet({
  item,
  open,
  onClose,
  isAdmin,
  canManage,
  onChanged,
}: {
  item?: MediaItem;
  open: boolean;
  onClose: () => void;
  isAdmin: boolean;
  canManage: boolean;
  onChanged: () => void;
}) {
  const [editing, setEditing] = useState(false);
  const [confirmLifecycle, setConfirmLifecycle] = useState(false);
  const [targetType, setTargetType] = useState<MediaLinkTargetType | null>(
    null,
  );
  const archive = useMutation({
    mutationFn: (action: "archive" | "restore") =>
      mediaApi.archive(item!.id, action),
    onSuccess: () => {
      toast.success("Status medija je ažuriran.");
      setConfirmLifecycle(false);
      onChanged();
    },
  });
  return (
    <Sheet open={open} onOpenChange={(value) => !value && onClose()}>
      <SheetContent className="w-full overflow-x-hidden overflow-y-auto data-[side=right]:sm:max-w-xl">
        <SheetHeader>
          <SheetTitle>{item?.title ?? "Medij"}</SheetTitle>
          <SheetDescription>
            {item
              ? `${sourceLabels[item.sourceType]} · ${categoryLabels[item.category]}`
              : "Učitavanje detalja"}
          </SheetDescription>
        </SheetHeader>
        {item ? (
          <div className="mt-6 flex min-w-0 flex-col gap-4 px-4 pb-6">
            <section className="flex flex-col gap-2 rounded-xl border p-4">
              <p className="text-muted-foreground">
                {item.description ?? "Nema opisa."}
              </p>
              {item.sourceType === "UPLOADED_FILE" ? (
                <>
                  <p>
                    {item.source.originalFileName} ·{" "}
                    {formatBytes(item.source.sizeBytes)}
                  </p>
                  {item.category === "IMAGE" ? (
                    <img
                      alt={item.title}
                      className="max-h-80 rounded-md object-contain"
                      src={contentUrl(item.id)}
                    />
                  ) : item.category === "VIDEO" ? (
                    <video
                      className="max-h-80 w-full"
                      controls
                      src={contentUrl(item.id)}
                    >
                      Vaš preglednik ne podržava video pregled.
                    </video>
                  ) : null}
                  <div className="flex flex-wrap gap-2">
                    <Button
                      render={
                        <a
                          href={contentUrl(item.id)}
                          target="_blank"
                          rel="noreferrer"
                        />
                      }
                    >
                      Otvori
                    </Button>
                    <Button
                      variant="outline"
                      render={
                        <a
                          href={contentUrl(item.id, true)}
                          target="_blank"
                          rel="noreferrer"
                        />
                      }
                    >
                      Preuzmi
                    </Button>
                  </div>
                </>
              ) : (
                <Button
                  render={
                    <a
                      href={item.source.url ?? "#"}
                      target="_blank"
                      rel="noreferrer noopener"
                    />
                  }
                >
                  Otvori vanjsku referencu <Link2 data-icon="inline-end" />
                </Button>
              )}
            </section>
            {canManage || isAdmin ? (
              <section className="flex flex-col gap-4 rounded-xl border p-4">
                <div className="flex flex-col gap-1">
                  <h3 className="font-medium">Upravljanje medijem</h3>
                  <p className="text-muted-foreground text-sm">
                    Uredite podatke, povežite medij ili upravljajte njegovim
                    statusom.
                  </p>
                </div>
                {canManage && !item.isArchived ? (
                  <>
                    <div className="flex flex-col gap-2">
                      <p className="text-sm font-medium">Poveži medij</p>
                      <div className="grid gap-2 sm:grid-cols-3">
                        <Button
                          className="justify-start"
                          variant="outline"
                          onClick={() => setTargetType("MATCH")}
                        >
                          <CalendarPlus data-icon="inline-start" />
                          Utakmica
                        </Button>
                        <Button
                          className="justify-start"
                          variant="outline"
                          onClick={() => setTargetType("MATCH_REPORT")}
                        >
                          <FilePenLine data-icon="inline-start" />
                          Izvještaj
                        </Button>
                        <Button
                          className="justify-start"
                          variant="outline"
                          onClick={() => setTargetType("PLAYER")}
                        >
                          <UserRoundPlus data-icon="inline-start" />
                          Igrač
                        </Button>
                      </div>
                    </div>
                    <Separator />
                    <Button
                      className="justify-start"
                      variant="outline"
                      onClick={() => setEditing(true)}
                    >
                      <FilePenLine data-icon="inline-start" />
                      Uredi podatke o mediju
                    </Button>
                  </>
                ) : null}
                {isAdmin ? (
                  <>
                    {canManage && !item.isArchived ? <Separator /> : null}
                    <div className="flex flex-col gap-2">
                      <p className="text-sm font-medium">Status medija</p>
                      <Button
                        className="justify-start"
                        disabled={archive.isPending}
                        variant="outline"
                        onClick={() => setConfirmLifecycle(true)}
                      >
                        {item.isArchived ? (
                          <ArchiveRestore data-icon="inline-start" />
                        ) : (
                          <Archive data-icon="inline-start" />
                        )}
                        {item.isArchived
                          ? "Vrati medij iz arhive"
                          : "Arhiviraj medij"}
                      </Button>
                    </div>
                  </>
                ) : null}
              </section>
            ) : null}
          </div>
        ) : null}
        {item && editing ? (
          <EditMediaDialog
            item={item}
            onClose={() => setEditing(false)}
            onSaved={() => {
              setEditing(false);
              onChanged();
            }}
          />
        ) : null}
        {item && targetType ? (
          <LinkMediaDialog
            item={item}
            targetType={targetType}
            onClose={() => setTargetType(null)}
            onSaved={() => {
              setTargetType(null);
              onChanged();
            }}
          />
        ) : null}
        {item && confirmLifecycle ? (
          <Dialog
            open
            onOpenChange={(open) => !open && setConfirmLifecycle(false)}
          >
            <DialogContent>
              <DialogHeader>
                <DialogTitle>
                  {item.isArchived ? "Vrati medij" : "Arhiviraj medij"}
                </DialogTitle>
                <DialogDescription>
                  {item.isArchived
                    ? "Medij će ponovo biti dostupan ovlaštenim korisnicima."
                    : "Arhiviranje ne briše fajl niti postojeće veze."}
                </DialogDescription>
              </DialogHeader>
              <div className="flex justify-end gap-2">
                <Button
                  variant="outline"
                  onClick={() => setConfirmLifecycle(false)}
                >
                  Odustani
                </Button>
                <Button
                  disabled={archive.isPending}
                  variant={item.isArchived ? "default" : "destructive"}
                  onClick={() =>
                    archive.mutate(item.isArchived ? "restore" : "archive")
                  }
                >
                  {item.isArchived ? "Vrati" : "Arhiviraj"}
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        ) : null}
      </SheetContent>
    </Sheet>
  );
}

function EditMediaDialog({
  item,
  onClose,
  onSaved,
}: {
  item: MediaItem;
  onClose: () => void;
  onSaved: () => void;
}) {
  const [title, setTitle] = useState(item.title);
  const [description, setDescription] = useState(item.description ?? "");
  const [category, setCategory] = useState(item.category);
  const save = useMutation({
    mutationFn: () =>
      mediaApi.update(item.id, {
        title,
        description,
        category,
        ...(item.sourceType === "EXTERNAL_REFERENCE"
          ? {
              url: item.source.url ?? "",
              providerLabel: item.source.providerLabel ?? "",
            }
          : {}),
      }),
    onSuccess: onSaved,
    onError: () => toast.error("Medij nije moguće urediti."),
  });
  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Uredi medij</DialogTitle>
          <DialogDescription>
            Izvor i selekcija se ne mogu promijeniti.
          </DialogDescription>
        </DialogHeader>
        <form
          onSubmit={(event) => {
            event.preventDefault();
            if (title.trim()) save.mutate();
          }}
        >
          <FieldGroup>
            <Field>
              <FieldLabel>Naziv</FieldLabel>
              <Input
                value={title}
                onChange={(event) => setTitle(event.target.value)}
              />
            </Field>
            <Field>
              <FieldLabel>Kategorija</FieldLabel>
              <Select
                value={category}
                onValueChange={(value) =>
                  value && setCategory(value as MediaCategory)
                }
              >
                <SelectTrigger>
                  <SelectValue>{categoryLabels[category]}</SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectGroup>
                    {categories.map((value) => (
                      <SelectItem key={value} value={value}>
                        {categoryLabels[value]}
                      </SelectItem>
                    ))}
                  </SelectGroup>
                </SelectContent>
              </Select>
            </Field>
            <Field>
              <FieldLabel>Opis</FieldLabel>
              <Textarea
                value={description}
                onChange={(event) => setDescription(event.target.value)}
              />
            </Field>
            <div className="flex justify-end gap-2">
              <Button type="button" variant="outline" onClick={onClose}>
                Odustani
              </Button>
              <Button disabled={save.isPending || !title.trim()} type="submit">
                Sačuvaj
              </Button>
            </div>
          </FieldGroup>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function LinkMediaDialog({
  item,
  targetType,
  onClose,
  onSaved,
}: {
  item: MediaItem;
  targetType: MediaLinkTargetType;
  onClose: () => void;
  onSaved: () => void;
}) {
  const [search, setSearch] = useState("");
  const targetName =
    targetType === "MATCH"
      ? "utakmicom"
      : targetType === "MATCH_REPORT"
        ? "izvještajem"
        : "igračem";
  const searchPlaceholder =
    targetType === "MATCH"
      ? "Pretraži protivnika ili kolo"
      : targetType === "MATCH_REPORT"
        ? "Pretraži izvještaj"
        : "Pretraži igrača";
  const emptyMessage =
    targetType === "MATCH"
      ? `Nema linkabilnih utakmica za selekciju ${item.teamName}. Utakmica ne smije biti otkazana ni zaključana izvještajem.`
      : targetType === "MATCH_REPORT"
        ? `Nema izvještaja u nacrtu ili sa traženom korekcijom za selekciju ${item.teamName}.`
        : `Nema igrača dodijeljenih selekciji ${item.teamName}.`;
  const candidates = useQuery({
    queryKey: ["mediaLinkCandidates", item.id, targetType, search],
    queryFn: () => mediaApi.candidates(item.id, targetType, search),
    retry: false,
  });
  const queryClient = useQueryClient();
  const link = useMutation({
    mutationFn: (id: string) => mediaApi.link(item.id, targetType, id),
    onSuccess: () => {
      toast.success(`Medij je povezan s ${targetName}.`);
      queryClient.removeQueries({
        queryKey: ["mediaLinkCandidates", item.id, targetType],
      });
      onSaved();
    },
    onError: (error) => {
      toast.error(
        isApiError(error)
          ? (error.detail ?? error.title)
          : "Povezivanje nije moguće. Osvježite podatke i pokušajte ponovo.",
      );
      void candidates.refetch();
    },
  });
  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Poveži s {targetName}</DialogTitle>
          <DialogDescription>
            {targetType === "PLAYER"
              ? `Prikazani su samo igrači dodijeljeni selekciji ${item.teamName}.`
              : "Prikazani su samo dozvoljeni nepovezani zapisi."}
          </DialogDescription>
        </DialogHeader>
        <Field>
          <FieldLabel htmlFor="media-candidate-search">Pretraga</FieldLabel>
          <Input
            id="media-candidate-search"
            placeholder={searchPlaceholder}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        </Field>
        <div className="flex max-h-72 flex-col gap-2 overflow-y-auto">
          {candidates.isLoading || candidates.isFetching ? (
            <p className="text-muted-foreground text-sm">Pretraga…</p>
          ) : candidates.isError ? (
            <p className="text-muted-foreground text-sm">
              Kandidati se ne mogu učitati.
            </p>
          ) : candidates.data?.items.length ? (
            candidates.data.items.map((candidate) => (
              <Button
                key={candidate.id}
                variant="outline"
                disabled={link.isPending}
                onClick={() => link.mutate(candidate.id)}
              >
                {candidate.primaryLabel}
                {candidate.secondaryLabel
                  ? ` · ${candidate.secondaryLabel}`
                  : ""}
              </Button>
            ))
          ) : (
            <p className="text-muted-foreground text-sm">{emptyMessage}</p>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}

function UploadDialog({
  teams,
  onClose,
  onCreated,
}: {
  teams: { id: string; name: string; status: string }[];
  onClose: () => void;
  onCreated: (item: MediaItem) => void;
}) {
  const capabilities = useQuery({
    queryKey: ["mediaCapabilities"],
    queryFn: mediaApi.capabilities,
  });
  const [teamId, setTeamId] = useState("");
  const [category, setCategory] = useState<MediaCategory>("VIDEO");
  const [title, setTitle] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [progress, setProgress] = useState<number | null>(null);
  const [abort, setAbort] = useState<AbortController | null>(null);
  const allowed = capabilities.data?.uploadedFileTypes.find(
    (item) => item.category === category,
  );
  const invalid = Boolean(
    file &&
    (!allowed?.extensions.some((extension) =>
      file.name.toLowerCase().endsWith(extension),
    ) ||
      file.size === 0 ||
      file.size > (capabilities.data?.maxUploadSizeBytes ?? 0)),
  );
  const upload = useMutation({
    mutationFn: () => {
      if (!file) throw new Error("Odaberite fajl.");
      const controller = new AbortController();
      setAbort(controller);
      setProgress(0);
      return mediaApi.upload(
        { teamId, category, title },
        file,
        setProgress,
        controller.signal,
      );
    },
    onSuccess: onCreated,
    onError: (error) =>
      toast.error(
        error instanceof Error
          ? error.message
          : "Fajl nije moguće učitati. Provjerite format i veličinu.",
      ),
    onSettled: () => {
      setAbort(null);
      setProgress(null);
    },
  });
  return (
    <Dialog
      open
      onOpenChange={(open) => {
        if (!open && !upload.isPending) onClose();
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Učitaj fajl</DialogTitle>
          <DialogDescription>
            Format i maksimalna veličina dolaze iz servera.
          </DialogDescription>
        </DialogHeader>
        <form
          onSubmit={(event) => {
            event.preventDefault();
            if (teamId && title.trim() && file && !invalid) upload.mutate();
          }}
        >
          <FieldGroup>
            <Field>
              <FieldLabel>Selekcija</FieldLabel>
              <Select
                value={teamId}
                onValueChange={(value) => setTeamId(value ?? "")}
              >
                <SelectTrigger>
                  <SelectValue>
                    {teams.find((team) => team.id === teamId)?.name ??
                      "Odaberite selekciju"}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectGroup>
                    {teams
                      .filter((item) => item.status === "ACTIVE")
                      .map((item) => (
                        <SelectItem key={item.id} value={item.id}>
                          {item.name}
                        </SelectItem>
                      ))}
                  </SelectGroup>
                </SelectContent>
              </Select>
            </Field>
            <Field>
              <FieldLabel>Kategorija</FieldLabel>
              <Select
                value={category}
                onValueChange={(value) =>
                  value && setCategory(value as MediaCategory)
                }
              >
                <SelectTrigger>
                  <SelectValue>{categoryLabels[category]}</SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectGroup>
                    {(capabilities.data?.uploadedFileTypes ?? []).map(
                      (item) => (
                        <SelectItem key={item.category} value={item.category}>
                          {categoryLabels[item.category]}
                        </SelectItem>
                      ),
                    )}
                  </SelectGroup>
                </SelectContent>
              </Select>
            </Field>
            <Field data-invalid={invalid}>
              <FieldLabel htmlFor="media-file">Fajl</FieldLabel>
              <Input
                id="media-file"
                type="file"
                accept={allowed?.extensions.join(",")}
                onChange={(event) => setFile(event.target.files?.[0] ?? null)}
                aria-invalid={invalid}
              />
              <p className="text-muted-foreground text-sm">
                Maksimalna veličina:{" "}
                {formatBytes(capabilities.data?.maxUploadSizeBytes ?? null)}
              </p>
              {invalid ? (
                <p className="text-destructive text-sm">
                  Fajl je prazan, prevelik ili nepodržan.
                </p>
              ) : null}
            </Field>
            <Field>
              <FieldLabel htmlFor="upload-title">Naziv</FieldLabel>
              <Input
                id="upload-title"
                value={title}
                onChange={(event) => setTitle(event.target.value)}
              />
            </Field>
            {progress !== null ? (
              <Field>
                <FieldLabel>Upload u toku: {Math.round(progress)}%</FieldLabel>
                <Progress value={progress} />
              </Field>
            ) : null}
            <div className="flex justify-end gap-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => (abort ? abort.abort() : onClose())}
              >
                {upload.isPending ? "Prekini" : "Odustani"}
              </Button>
              <Button
                type="submit"
                disabled={
                  upload.isPending ||
                  !teamId ||
                  !title.trim() ||
                  !file ||
                  invalid
                }
              >
                Učitaj
              </Button>
            </div>
          </FieldGroup>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function ExternalCreateDialog({
  teams,
  onClose,
  onCreated,
}: {
  teams: { id: string; name: string; status: string }[];
  onClose: () => void;
  onCreated: (item: MediaItem) => void;
}) {
  const [teamId, setTeamId] = useState("");
  const [category, setCategory] = useState<MediaCategory>("VIDEO");
  const [title, setTitle] = useState("");
  const [url, setUrl] = useState("");
  const [description, setDescription] = useState("");
  const create = useMutation({
    mutationFn: () =>
      mediaApi.external({ teamId, category, title, url, description }),
    onSuccess: onCreated,
    onError: () => toast.error("Medij nije moguće sačuvati."),
  });
  const validUrl = (() => {
    try {
      const parsed = new URL(url);
      return (
        (parsed.protocol === "http:" || parsed.protocol === "https:") &&
        !parsed.username &&
        !parsed.password
      );
    } catch {
      return false;
    }
  })();
  return (
    <Dialog open onOpenChange={(value) => !value && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Dodaj vanjsku referencu</DialogTitle>
          <DialogDescription>
            URL se ne pregledava niti dohvaća iz aplikacije.
          </DialogDescription>
        </DialogHeader>
        <form
          className="mt-4"
          onSubmit={(e) => {
            e.preventDefault();
            if (teamId && title.trim() && validUrl) create.mutate();
          }}
        >
          <FieldGroup>
            <Field>
              <FieldLabel htmlFor="media-team">Selekcija</FieldLabel>
              <Select
                value={teamId}
                onValueChange={(value) => setTeamId(value ?? "")}
              >
                <SelectTrigger id="media-team">
                  <SelectValue>
                    {teams.find((team) => team.id === teamId)?.name ??
                      "Odaberite selekciju"}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectGroup>
                    {teams
                      .filter((t) => t.status === "ACTIVE")
                      .map((t) => (
                        <SelectItem key={t.id} value={t.id}>
                          {t.name}
                        </SelectItem>
                      ))}
                  </SelectGroup>
                </SelectContent>
              </Select>
            </Field>
            <Field>
              <FieldLabel>Kategorija</FieldLabel>
              <Select
                value={category}
                onValueChange={(value) =>
                  value && setCategory(value as MediaCategory)
                }
              >
                <SelectTrigger>
                  <SelectValue>{categoryLabels[category]}</SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectGroup>
                    {categories.map((value) => (
                      <SelectItem key={value} value={value}>
                        {categoryLabels[value]}
                      </SelectItem>
                    ))}
                  </SelectGroup>
                </SelectContent>
              </Select>
            </Field>
            <Field>
              <FieldLabel htmlFor="media-title">Naziv</FieldLabel>
              <Input
                id="media-title"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
              />
            </Field>
            <Field>
              <FieldLabel htmlFor="media-url">URL</FieldLabel>
              <Input
                id="media-url"
                type="url"
                value={url}
                onChange={(e) => setUrl(e.target.value)}
              />
            </Field>
            <Field>
              <FieldLabel htmlFor="media-description">Opis</FieldLabel>
              <Textarea
                id="media-description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </Field>
            <div className="flex justify-end gap-2">
              <Button type="button" variant="outline" onClick={onClose}>
                Odustani
              </Button>
              <Button
                disabled={
                  !teamId || !title.trim() || !validUrl || create.isPending
                }
                type="submit"
              >
                Sačuvaj
              </Button>
            </div>
          </FieldGroup>
        </form>
      </DialogContent>
    </Dialog>
  );
}
