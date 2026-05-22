namespace FootballAcademy.Services;

public static class AppMonitoringState
{
    public static readonly DateTime ServerStartedAt = DateTime.UtcNow;

    private static readonly List<ClientErrorEntry> _errors = [];
    private static readonly Lock _lock = new();
    private const int MaxErrors = 500;

    public static void AddError(ClientErrorEntry entry)
    {
        lock (_lock)
        {
            _errors.Add(entry);
            if (_errors.Count > MaxErrors)
                _errors.RemoveAt(0);
        }
    }

    public static List<ClientErrorEntry> GetErrors()
    {
        lock (_lock) { return [.. _errors]; }
    }

    public static void ClearErrors()
    {
        lock (_lock) { _errors.Clear(); }
    }
}

public class ClientErrorEntry
{
    public DateTime Timestamp { get; set; }
    public int Status { get; set; }
    public string Method { get; set; } = "";
    public string Url { get; set; } = "";
    public string Message { get; set; } = "";
    public int? UserId { get; set; }
}
