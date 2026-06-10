namespace RuDotaOnlineAPI.Gateway.Configuration;

public sealed class SlidingWindowPolicy
{
    public int PermitLimit { get; init; } = 60;
    public int WindowSeconds { get; init; } = 60;
    public int SegmentsPerWindow { get; init; } = 6;
    public int QueueLimit { get; init; } = 0;
}