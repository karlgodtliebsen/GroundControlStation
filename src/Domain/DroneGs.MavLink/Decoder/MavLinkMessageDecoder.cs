using DroneGs.MavLink.Messages;

namespace DroneGs.MavLink.Decoder;

/// <summary>
/// Decodes MAVLink messages using a collection of specific message decoders.
/// </summary>
public sealed class MavLinkMessageDecoder(IEnumerable<IMavLinkMessageDecoder> decoders) : IMavLinkMessageDecoder
{
    private readonly IReadOnlyList<IMavLinkMessageDecoder> decoders = decoders.ToArray();


    /// <inheritdoc />
    public bool TryDecode(MavLinkFrame frame, out MavLinkMessage? message)
    {
        foreach (var decoder in decoders)
        {
            if (decoder.TryDecode(frame, out message))
            {
                return true;
            }
        }

        message = null;
        return false;
    }
}
