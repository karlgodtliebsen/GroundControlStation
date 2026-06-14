namespace DroneGs.MavLink;

/// <summary>
/// 
/// </summary>
public interface IMavLinkFrameParser
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="receivedAt"></param>
    /// <returns></returns>
    IReadOnlyList<MavLinkFrame> Parse(ReadOnlySpan<byte> data, DateTimeOffset receivedAt);
}
