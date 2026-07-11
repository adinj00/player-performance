namespace PlayerPerformance.Infrastructure.Staff;

public sealed class StaffTeamScope
{
    public Guid UserId { get; set; }
    public Guid TeamId { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}
