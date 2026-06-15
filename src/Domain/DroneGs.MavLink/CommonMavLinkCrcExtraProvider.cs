namespace DroneGs.MavLink;

/// <summary>
/// Provides CRC extra bytes for common MAVLink messages.
/// </summary>
public sealed class CommonMavLinkCrcExtraProvider : IMavLinkCrcExtraProvider
{
    /// <inheritdoc />
    public bool TryGetCrcExtra(uint messageId, out byte crcExtra)
    {
        switch (messageId)
        {
            case MessageIds.Heartbeat:
                crcExtra = 50;
                return true;

            case MessageIds.CommandLong:
                crcExtra = 152;
                return true;

            case MessageIds.CommandAck:
                crcExtra = 143;
                return true;

            default:
                crcExtra = 0;
                return false;
        }
    }
}
