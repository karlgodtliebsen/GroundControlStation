namespace DroneGs.MavLink.Messages;

/// <summary>
/// Contains the IDs of MAVLink messages.
/// </summary>
public static class MessageIds
{
    public const uint Heartbeat = 0;
    public const uint SysStatus = 1;
    public const uint Attitude = 30;
    public const uint GlobalPositionInt = 33;
    public const uint CommandLong = 76;
    public const uint CommandAck = 77;
}
