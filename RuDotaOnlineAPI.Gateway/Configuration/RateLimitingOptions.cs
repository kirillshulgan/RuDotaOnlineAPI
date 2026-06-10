namespace RuDotaOnlineAPI.Gateway.Configuration;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public SlidingWindowPolicy AnonymousSlidingWindow { get; init; } = new();
    public SlidingWindowPolicy AuthenticatedSlidingWindow { get; init; } = new();
}