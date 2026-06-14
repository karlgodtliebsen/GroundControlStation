namespace DroneGs.MavLink;

/// <summary>
/// Provides known MAVLink frames for testing and simulation purposes.
/// </summary>
public static class MavLinkKnownFrames
{
    // MAVLink v2 HEARTBEAT
    //
    // SystemId:    1
    // ComponentId: 1
    // MessageId:   0 HEARTBEAT
    //
    // Payload:
    // custom_mode:   0
    // type:          2 MAV_TYPE_QUADROTOR
    // autopilot:     3 MAV_AUTOPILOT_ARDUPILOTMEGA
    // base_mode:     0
    // system_status: 4 MAV_STATE_ACTIVE
    // mavlink_ver:   3
    //
    // Frame length:
    // 10 byte MAVLink v2 header
    // 9 byte payload
    // 2 byte checksum
    //
    // No signature.
    /// <summary>
    /// Creates a MAVLink v2 HEARTBEAT frame.
    /// </summary>
    /// <returns>A byte array representing the HEARTBEAT frame.</returns>
    public static byte[] CreateHeartbeatV2()
    {
        return
        [
            0xFD, // MAVLink v2 magic
            0x09, // payload length
            0x00, // incompat flags
            0x00, // compat flags
            0x00, // sequence
            0x01, // system id
            0x01, // component id
            0x00, // message id byte 0
            0x00, // message id byte 1
            0x00, // message id byte 2

            // HEARTBEAT payload
            0x00, 0x00, 0x00, 0x00, // custom_mode
            0x02, // type
            0x03, // autopilot
            0x00, // base_mode
            0x04, // system_status
            0x03, // mavlink_version

            // CRC, little-endian
            0x4A, 0xD7
        ];
    }
}
