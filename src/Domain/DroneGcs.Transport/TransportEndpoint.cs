namespace DroneGcs.Transport;

/// <summary>
/// Represents an endpoint for a transport connection, including protocol, address, and port information.
/// </summary>
public class TransportEndpoint
{
    /// <summary>
    /// The port number of the remote endpoint.
    /// </summary>
    public int RemotePort { get; } = 14551;

    /// <summary>
    /// The protocol used by the transport endpoint (e.g., "udp", "tcp").
    /// </summary>
    public string Protocol { get; } = "udp";


    /// <summary>
    /// The host address of the remote endpoint.
    /// </summary>
    public string RemoteHost { get; } = "127.0.0.1";

    /// <summary>
    /// The port number of the local endpoint.
    /// </summary>
    public int LocalPort { get; } = 14550;

    /// <summary>
    /// The host address of the local endpoint.
    /// </summary>
    public string LocalHost { get; } = "127.0.0.1"; // or "0.0.0.0"

    /// <summary>
    /// 
    /// </summary>
    public int ReceiveBufferSize { get; } = 512;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransportEndpoint"/> class with the specified remote port, remote host, and local port.  
    /// </summary>
    /// <param name="protocol"></param>
    /// <param name="remotePort"></param>
    /// <param name="remoteHost"></param>
    /// <param name="localPort"></param>
    /// <param name="receiveBufferSize"></param>
    public TransportEndpoint(string protocol, int remotePort, string remoteHost, int localPort, int receiveBufferSize = 512)
    {
        Protocol = protocol;
        RemotePort = remotePort;
        RemoteHost = remoteHost;
        LocalPort = localPort;
        ReceiveBufferSize = receiveBufferSize;
    }
}
