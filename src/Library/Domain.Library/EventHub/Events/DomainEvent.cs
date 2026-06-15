namespace Domain.Library.EventHub.Events;

public interface IDomainEvent
{
    MetaData GetMetaData();
    object? Payload { get; }
    string Name { get; }

    T? TryGetMetaData<T>() where T : MetaData;

    T? TryGetData<T>() where T : class;
}

/// <summary>
/// 
/// </summary>
public class DomainEvent : Event, IDomainEvent
{
    public DomainEvent(string name) : base(name, null, new MetaData())
    {
    }

    // ReSharper disable once MemberCanBeProtected.Global
    public DomainEvent(string name, object? data, MetaData metadata) : base(name, data, metadata)
    {
    }

    public DomainEvent(string name, object? data) : base(name, data, new MetaData())
    {
    }

    public DomainEvent(string name, MetaData md) : base(name, null, md)
    {
    }

    public MetaData GetMetaData()
    {
        return MetaData;
    }
}

public class DomainEvent<T> : DomainEvent
{
    public DomainEvent(string name, T data) : base(name, data, new MetaData())
    {
    }

    public DomainEvent(string name, T data, MetaData md) : base(name, data, md)
    {
    }

    public T GetData()
    {
        return (T)Payload!;
    }
}

public class DomainEvent<T, TM>(string name, T data, TM metadata) : DomainEvent(name, data, metadata) where TM : MetaData
{
    public T GetData()
    {
        return (T)Payload!;
    }
}
