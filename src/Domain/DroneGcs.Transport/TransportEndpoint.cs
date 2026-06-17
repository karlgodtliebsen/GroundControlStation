namespace DroneGcs.Transport;

/// <summary>
/// Represents an endpoint for a transport connection, including protocol, address, and port information.
/// </summary>
public class TransportEndpoint
{
    /// <summary>
    /// The port number of the remote endpoint.
    /// </summary>
    public int RemotePort { get; set; }

    /// <summary>
    /// The protocol used by the transport endpoint (e.g., "udp", "tcp").
    /// </summary>
    public string Protocol { get; set; }


    /// <summary>
    /// The host address of the remote endpoint.
    /// </summary>
    public string RemoteHost { get; set; }

    /// <summary>
    /// The port number of the local endpoint.
    /// </summary>
    public int LocalPort { get; set; }

    /// <summary>
    /// The host address of the local endpoint.
    /// </summary>
    public string LocalHost { get; set; } = "127.0.0.1"; // or "0.0.0.0"

    /// <summary>
    /// 
    /// </summary>
    public int ReceiveBufferSize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransportEndpoint"/> class with the specified remote port, remote host, and local port.  
    /// </summary>
    /// <param name="protocol"></param>
    /// <param name="remotePort"></param>
    /// <param name="remoteHost"></param>
    /// <param name="localPort"></param>
    /// <param name="receiveBufferSize"></param>
    public TransportEndpoint(string protocol = "udp", int remotePort = 14551, string remoteHost = "127.0.0.1", int localPort = 14550, int receiveBufferSize = 512)
    {
        Protocol = protocol;
        RemotePort = remotePort;
        RemoteHost = remoteHost;
        LocalPort = localPort;
        ReceiveBufferSize = receiveBufferSize;
    }
}
