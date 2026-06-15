using System.Buffers.Binary;

using DroneGs.MavLink.Messages;
using DroneGs.MavLink.Services;

namespace DroneGs.MavLink.Decoding;

/// <inheritdoc />
public sealed class AttitudeMessageDecoder : IMavLinkMessageDecoder
{
    /// <inheritdoc />
    public bool TryDecode(MavLinkFrame frame, out MavLinkMessage? message)
    {
        message = null;

        if (frame.MessageId != MessageIds.Attitude)
        {
            return false;
        }

        if (frame.Payload.Length < 28)
        {
            return false;
        }

        var span = frame.Payload.Span;

        message = new AttitudeMessage(
            frame.SystemId,
            frame.ComponentId,
            ReadSingle(span[4..8]),
            ReadSingle(span[8..12]),
            ReadSingle(span[12..16]),
            frame.ReceivedAt);

        return true;
    }

    private static float ReadSingle(ReadOnlySpan<byte> value)
    {
        var raw = BinaryPrimitives.ReadInt32LittleEndian(value);
        return BitConverter.Int32BitsToSingle(raw);
    }
}
