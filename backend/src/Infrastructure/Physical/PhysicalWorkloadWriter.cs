using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Physical;
using PlayerPerformance.Domain.Physical;
using PlayerPerformance.Domain.Training;
using PlayerPerformance.Infrastructure.Persistence;
namespace PlayerPerformance.Infrastructure.Physical;

internal sealed class PhysicalWorkloadWriter(AppDbContext db, IPhysicalMetricCatalog catalog) : IPhysicalWorkloadWriter
{
    public async Task<Guid> WriteAsync(PhysicalWorkloadWriteCommand c, CancellationToken ct)
    {
        if (c.TrainingSessionParticipantId.HasValue == c.PlayerMatchAppearanceId.HasValue || c.Metrics.Count == 0 || c.Metrics.Select(x => x.MetricCode).Distinct(StringComparer.Ordinal).Count() != c.Metrics.Count)
            throw new InvalidOperationException("A complete explicit workload context and unique metrics are required.");
        PlayerPhysicalWorkload? workload;
        Guid teamId;
        Guid playerId;
        DateOnly date;
        if (c.TrainingSessionParticipantId is { } participantId)
        {
            var row = await (from p in db.TrainingSessionParticipants join s in db.TrainingSessions on p.TrainingSessionId equals s.Id where p.Id == participantId select new { p, s }).SingleOrDefaultAsync(ct) ?? throw new InvalidOperationException("Training participant was not found.");
            if (row.s.Status != TrainingSessionStatus.COMPLETED)
                throw new InvalidOperationException("Training session must be completed.");
            teamId = row.s.TeamId;
            playerId = row.p.PlayerId;
            date = row.s.SessionDate;
            workload = await db.PlayerPhysicalWorkloads.SingleOrDefaultAsync(x => x.TrainingSessionParticipantId == participantId, ct);
        }
        else
        {
            var row = await (from a in db.PlayerMatchAppearances join m in db.Matches on a.MatchId equals m.Id where a.Id == c.PlayerMatchAppearanceId select new { a, m }).SingleOrDefaultAsync(ct) ?? throw new InvalidOperationException("Match appearance was not found.");
            if (row.m.Status != Domain.Matches.MatchStatus.PLAYED || row.m.IsArchived)
                throw new InvalidOperationException("Match workload target is not eligible.");
            teamId = row.m.TeamId;
            playerId = row.a.PlayerId;
            date = DateOnly.FromDateTime(row.m.KickoffAtUtc);
            workload = await db.PlayerPhysicalWorkloads.SingleOrDefaultAsync(x => x.PlayerMatchAppearanceId == row.a.Id, ct);
        }
        foreach (var m in c.Metrics)
            Validate(m);
        if (workload is not null && (c.DuplicatePolicy == PhysicalWorkloadDuplicatePolicy.REJECT_IF_WORKLOAD_EXISTS || workload.CurrentRevisionId != c.ExpectedCurrentRevisionId))
            throw new InvalidOperationException("Workload revision conflict.");
        if (workload is null)
        { workload = PlayerPhysicalWorkload.Create(Guid.NewGuid(), teamId, playerId, date, c.TrainingSessionParticipantId, c.PlayerMatchAppearanceId, DateTime.UtcNow); db.PlayerPhysicalWorkloads.Add(workload); }
        var revision = PhysicalWorkloadRevision.CreateImport(Guid.NewGuid(), workload.Id, (await db.PhysicalWorkloadRevisions.CountAsync(x => x.PlayerPhysicalWorkloadId == workload.Id, ct)) + 1, c.ImportJobId, c.SourceSystem, c.ProcessorKey, c.ProcessorVersion, c.ActorUserId, DateTime.UtcNow);
        db.PhysicalWorkloadRevisions.Add(revision);
        foreach (var m in c.Metrics)
            db.PhysicalMetricValues.Add(PhysicalMetricValue.Create(Guid.NewGuid(), revision.Id, m.MetricCode, m.Value, m.UnitCode, m.ThresholdValue, m.ThresholdUnitCode, m.ThresholdDirection, m.ThresholdScope, m.MethodKey, m.MethodVersion));
        workload.SetCurrentRevision(revision.Id);
        // The approved import-confirmation orchestrator owns SaveChanges/transaction commit.
        // Keeping this writer unit-of-work-neutral prevents a workload revision, import status,
        // and their audit records from being committed independently.
        return workload.Id;
    }
    private void Validate(CanonicalPhysicalMetricWrite m)
    {
        var d = catalog.Find(m.MetricCode) ?? throw new InvalidOperationException("Unknown canonical metric.");
        if (m.Value < 0 || d.CanonicalUnit != m.UnitCode || d.RequiresThresholdContext != m.ThresholdValue.HasValue || d.RequiresMethodContext != !string.IsNullOrWhiteSpace(m.MethodKey) || (d.ValueKind is PhysicalMetricValueKind.INTEGER or PhysicalMetricValueKind.DURATION && decimal.Truncate(m.Value) != m.Value))
            throw new InvalidOperationException("Metric value is invalid.");
    }
}
