namespace DroneGcs.Simulator.SmokeTests;

public static class TransportProbePayloads
{
    public static byte[] CreateAsciiProbe()
    {
        return "DRONEGCS-PROBE"u8.ToArray();
    }
}
