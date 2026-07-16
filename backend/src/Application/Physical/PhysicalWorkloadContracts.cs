using PlayerPerformance.Domain.Physical;
namespace PlayerPerformance.Application.Physical;
public enum PhysicalWorkloadDuplicatePolicy { REJECT_IF_WORKLOAD_EXISTS, APPEND_REVISION_IF_CURRENT_MATCHES_EXPECTATION }
public sealed record CanonicalPhysicalMetricWrite(string MetricCode, decimal Value, PhysicalMetricUnit UnitCode, decimal? ThresholdValue, PhysicalMetricUnit? ThresholdUnitCode, ThresholdDirection? ThresholdDirection, ThresholdScope? ThresholdScope, string? MethodKey, string? MethodVersion);
public sealed record PhysicalWorkloadWriteCommand(Guid? TrainingSessionParticipantId, Guid? PlayerMatchAppearanceId, PhysicalWorkloadDuplicatePolicy DuplicatePolicy, Guid? ExpectedCurrentRevisionId, Guid ImportJobId, string SourceSystem, string ProcessorKey, string ProcessorVersion, Guid ActorUserId, IReadOnlyList<CanonicalPhysicalMetricWrite> Metrics);
public interface IPhysicalWorkloadWriter { Task<Guid> WriteAsync(PhysicalWorkloadWriteCommand command, CancellationToken ct); }
