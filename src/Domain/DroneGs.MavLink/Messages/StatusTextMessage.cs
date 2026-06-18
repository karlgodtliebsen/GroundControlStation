namespace DroneGs.MavLink.Messages;

/// <summary>
/// Represents a MAVLink StatusTextMessage, which contains status information from the drone.
/// </summary>
/// <param name="SystemId"></param>
/// <param name="ComponentId"></param>
/// <param name="Severity"></param>
/// <param name="Text"></param>
/// <param name="Id"></param>
/// <param name="ChunkSequence"></param>
/// <param name="ReceivedAt"></param>
public sealed record StatusTextMessage(byte SystemId, byte ComponentId, MavSeverity Severity, string Text, ushort? Id, byte? ChunkSequence, DateTimeOffset ReceivedAt)
    : MavLinkMessage(SystemId, ComponentId, MessageIds.StatusText, ReceivedAt);
