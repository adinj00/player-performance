using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Media;

public sealed class MediaItem : Entity
{
    public const int TitleMaxLength = 200, DescriptionMaxLength = 2000;
    private MediaItem() : base(Guid.Empty)
    {
        Title = string.Empty;
    }
    private MediaItem(Guid id, Guid teamId, MediaSourceType sourceType, MediaCategory category, string title, string? description, Guid createdByUserId, DateTime now) : base(id)
    {
        TeamId = teamId;
        SourceType = sourceType;
        Category = category;
        Title = title;
        Description = description;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = UpdatedAtUtc = now;
    }
    public Guid TeamId { get; private set; }
    public MediaSourceType SourceType { get; private set; }
    public MediaCategory Category { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAtUtc { get; private set; }
    public Guid? ArchivedByUserId { get; private set; }
    public static MediaItem Create(Guid id, Guid teamId, MediaSourceType sourceType, MediaCategory category, string title, string? description, Guid actor, DateTime now)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(teamId, nameof(teamId));
        Guard.AgainstDefault(actor, nameof(actor));
        if (!Enum.IsDefined(sourceType) || !Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(nameof(category));
        return new(id, teamId, sourceType, category, Required(title, TitleMaxLength, nameof(title)), Optional(description, DescriptionMaxLength, nameof(description)), actor, now);
    }
    public bool Update(MediaCategory category, string title, string? description, DateTime now)
    {
        if (IsArchived)
            throw new InvalidOperationException("Archived media cannot be changed.");
        var t = Required(title, TitleMaxLength, nameof(title));
        var d = Optional(description, DescriptionMaxLength, nameof(description));
        if (!Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(nameof(category));
        if (Category == category && Title == t && Description == d)
            return false;
        Category = category;
        Title = t;
        Description = d;
        UpdatedAtUtc = now;
        return true;
    }
    public void Archive(Guid actor, DateTime now)
    {
        Guard.AgainstDefault(actor, nameof(actor));
        if (IsArchived)
            throw new InvalidOperationException("Media is already archived.");
        IsArchived = true;
        ArchivedByUserId = actor;
        ArchivedAtUtc = now;
        UpdatedAtUtc = now;
    }
    public void Restore(Guid actor, DateTime now)
    {
        Guard.AgainstDefault(actor, nameof(actor));
        if (!IsArchived)
            throw new InvalidOperationException("Media is already active.");
        IsArchived = false;
        ArchivedByUserId = null;
        ArchivedAtUtc = null;
        UpdatedAtUtc = now;
    }
    private static string Required(string? v, int max, string name) => Optional(v, max, name) ?? throw new ArgumentException(name);
    private static string? Optional(string? v, int max, string name)
    {
        if (string.IsNullOrWhiteSpace(v))
            return null;
        var x = v.Trim();
        if (x.Length > max || x.Any(char.IsControl))
            throw new ArgumentOutOfRangeException(name);
        return x;
    }
}
