using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;
namespace PlayerPerformance.Domain.Media;

public sealed class MediaAsset : Entity
{
    private MediaAsset() : base(Guid.Empty) { }
    private MediaAsset(Guid id, Guid mediaItemId, Guid storedFileId) : base(id)
    {
        MediaItemId = mediaItemId;
        StoredFileId = storedFileId;
    }
    public Guid MediaItemId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public static MediaAsset Create(Guid id, Guid mediaItemId, Guid storedFileId)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(mediaItemId, nameof(mediaItemId));
        Guard.AgainstDefault(storedFileId, nameof(storedFileId));
        return new(id, mediaItemId, storedFileId);
    }
}
public sealed class ExternalMediaReference : Entity
{
    public const int UrlMaxLength = 2048, ProviderLabelMaxLength = 100;
    private ExternalMediaReference() : base(Guid.Empty)
    {
        Url = string.Empty;
    }
    private ExternalMediaReference(Guid id, Guid mediaItemId, string url, string? label) : base(id)
    {
        MediaItemId = mediaItemId;
        Url = NormalizeUrl(url);
        ProviderLabel = NormalizeLabel(label);
    }
    public Guid MediaItemId { get; private set; }
    public string Url { get; private set; }
    public string? ProviderLabel { get; private set; }
    public static ExternalMediaReference Create(Guid id, Guid mediaItemId, string url, string? label)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(mediaItemId, nameof(mediaItemId));
        return new(id, mediaItemId, url, label);
    }
    public bool Update(string url, string? label)
    {
        var u = NormalizeUrl(url);
        var l = NormalizeLabel(label);
        if (u == Url && l == ProviderLabel)
            return false;
        Url = u;
        ProviderLabel = l;
        return true;
    }
    public static string NormalizeUrl(string? v)
    {
        if (string.IsNullOrWhiteSpace(v) || v.Trim().Length > UrlMaxLength || !Uri.TryCreate(v.Trim(), UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) || !string.IsNullOrEmpty(uri.UserInfo))
            throw new ArgumentException("A safe absolute HTTP(S) URL is required.", nameof(v));
        return uri.AbsoluteUri;
    }
    private static string? NormalizeLabel(string? v)
    {
        if (string.IsNullOrWhiteSpace(v))
            return null;
        var x = v.Trim();
        if (x.Length > ProviderLabelMaxLength || x.Any(char.IsControl))
            throw new ArgumentOutOfRangeException(nameof(v));
        return x;
    }
}
