using DroneGcs.Transport;

namespace DroneGcs.Simulator.SmokeTests;

public sealed record TransportSmokeTestResult(
    int BytesReceived,
    byte[] Data,
    MavLinkEndpoint? RemoteEndpoint,
    DateTimeOffset ReceivedAt);
