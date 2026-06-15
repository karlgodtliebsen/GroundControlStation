using DroneGcs.Core;

namespace DroneGcs.Test;

/// <summary>
/// 
/// </summary>
public static class SimulatedVehicleStateExtensions
{
    public static VehicleSession ApplyTo(
        this SimulatedVehicleState simulated,
        IVehicleRegistry registry)
    {
        var vehicle = registry.RegisterOrUpdateHeartbeat(
            simulated.VehicleId,
            simulated.CustomMode,
            simulated.VehicleType,
            simulated.Autopilot,
            simulated.BaseMode,
            simulated.SystemStatus,
            simulated.MavLinkVersion,
            simulated.Timestamp);

        if (simulated.Latitude is not null &&
            simulated.Longitude is not null &&
            simulated.Altitude is not null)
        {
            vehicle.ApplyPosition(
                simulated.Latitude.Value,
                simulated.Longitude.Value,
                simulated.Altitude.Value);
        }

        if (simulated.Roll is not null &&
            simulated.Pitch is not null &&
            simulated.Yaw is not null)
        {
            vehicle.ApplyAttitude(
                simulated.Roll.Value,
                simulated.Pitch.Value,
                simulated.Yaw.Value);
        }

        vehicle.ApplyBattery(
            simulated.BatteryRemaining,
            simulated.BatteryVoltage);

        return vehicle;
    }
}
