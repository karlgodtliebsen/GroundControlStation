namespace DroneGs.MavLink.Encoding;

/// <summary>
/// 
/// </summary>
public interface IMavLinkCommandEncoder
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetSystemId"></param>
    /// <param name="targetComponentId"></param>
    /// <param name="arm"></param>
    /// <returns></returns>
    byte[] EncodeArmDisarm(byte targetSystemId, byte targetComponentId, bool arm);
}
