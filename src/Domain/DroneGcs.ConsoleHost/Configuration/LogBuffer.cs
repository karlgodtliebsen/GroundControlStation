namespace DroneGcs.ConsoleHost.Configuration;

/// <summary>
/// 
/// </summary>
public sealed class LogBuffer
{
    private readonly object syncRoot = new();

    private readonly Queue<string> entries = new();

    public void Add(string message)
    {
        lock (syncRoot)
        {
            entries.Enqueue(message);

            while (entries.Count > 20)
            {
                entries.Dequeue();
            }
        }
    }

    public IReadOnlyList<string> GetEntries()
    {
        lock (syncRoot)
        {
            return entries.ToArray();
        }
    }
}
