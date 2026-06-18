namespace DroneGcs.ConsoleHost.Configuration;

public class ConsoleLineBuffer
{
    private readonly object syncRoot = new();
    private readonly Queue<string> lines = new();

    public ConsoleLineBuffer(int capacity = 100)
    {
        Capacity = capacity;
    }

    public int Capacity { get; }

    public void Add(string line)
    {
        lock (syncRoot)
        {
            lines.Enqueue(line);

            while (lines.Count > Capacity) lines.Dequeue();
        }
    }

    public IReadOnlyList<string> GetLines()
    {
        lock (syncRoot)
        {
            return lines.ToArray();
        }
    }

    public void Clear()
    {
        lock (syncRoot)
        {
            lines.Clear();
        }
    }
}
