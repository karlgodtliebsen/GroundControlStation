using DroneGs.MavLink.Messages;
using DroneGs.MavLink.Services;

namespace DroneGs.MavLink.Decoding;

/// <summary>
/// Decodes MAVLink messages using a collection of specific message decoders.
/// </summary>
public sealed class MavLinkMessageDecoder : IMavLinkMessageDecoder
{
    private readonly IReadOnlyList<IMavLinkMessageDecoder> decoders;

    /// <summary>
    /// Initializes a new instance of the <see cref="MavLinkMessageDecoder"/> class.
    /// </summary>
    /// <param name="options">The options containing the list of message decoders.</param>
    public MavLinkMessageDecoder(MavLinkMessageDecoders options)
    {
        decoders = options.Decoders;
    }


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
