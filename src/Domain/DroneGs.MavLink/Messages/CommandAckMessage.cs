namespace DroneGs.MavLink.Messages;

/// <summary>
/// 
/// </summary>
/// <param name="SystemId"></param>
/// <param name="ComponentId"></param>
/// <param name="Command"></param>
/// <param name="Result"></param>
/// <param name="ReceivedAt"></param>
public sealed record CommandAckMessage(
    byte SystemId,
    byte ComponentId,
    ushort Command,
    byte Result,
    DateTimeOffset ReceivedAt)
    : MavLinkMessage(
        SystemId,
        ComponentId,
        MessageIds.CommandAck,
        ReceivedAt);
