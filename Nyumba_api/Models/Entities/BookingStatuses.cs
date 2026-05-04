namespace Nyumba_api.Models.Entities;

public static class BookingStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
    public const string Rejected = "Rejected";
    public const string Completed = "Completed";

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        Confirmed,
        Cancelled,
        Rejected,
        Completed
    };

    public static bool IsValid(string status) => ValidStatuses.Contains(status);

    public static string Normalize(string status)
    {
        if (status.Equals(Pending, StringComparison.OrdinalIgnoreCase)) return Pending;
        if (status.Equals(Confirmed, StringComparison.OrdinalIgnoreCase)) return Confirmed;
        if (status.Equals(Cancelled, StringComparison.OrdinalIgnoreCase)) return Cancelled;
        if (status.Equals(Rejected, StringComparison.OrdinalIgnoreCase)) return Rejected;
        if (status.Equals(Completed, StringComparison.OrdinalIgnoreCase)) return Completed;

        return status;
    }
}
