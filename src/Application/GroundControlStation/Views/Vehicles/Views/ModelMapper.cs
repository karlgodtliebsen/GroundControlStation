using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using Riok.Mapperly.Abstractions;

#pragma warning disable CA1822

namespace GroundControlStationApp.Views.Vehicles.Views;

/// <summary>
/// Mapper class for converting between domain models and view models.
/// </summary>
[Mapper]
public partial class ModelMapper
{
    /// <summary>
    /// Maps a <see cref="VehicleSession"/> to a <see cref="VehicleSessionViewModel"/>.
    /// </summary>
    /// <param name="item">The vehicle session to map.</param>
    /// <returns>The mapped vehicle session view model.</returns>
    public partial VehicleSessionViewModel Map(VehicleSession item);

    /// <summary>
    /// Maps a <see cref="VehicleState"/> to a <see cref="VehicleStateViewModel"/>.
    /// </summary>
    /// <param name="item">The vehicle state to map.</param>
    /// <returns>The mapped vehicle state view model.</returns>
    public partial VehicleStateViewModel Map(VehicleState item);

    /// <summary>
    /// Maps a <see cref="VehicleSession"/> to a <see cref="VehicleSessionViewModel"/>.
    /// </summary>
    /// <param name="vehicle">The vehicle session to map.</param>
    /// <returns>The mapped vehicle session view model.</returns>
    public VehicleSessionViewModel MapToViewModel(VehicleSession vehicle)
    {
        var m = Map(vehicle);
        m.Notifications = "None";
        if (vehicle.Notifications.Any())
        {
            m.Notifications = string.Join(", ", vehicle.Notifications.Select(t => t.Text));
        }

        return m;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="vehicle"></param>
    public void Update(VehicleState vehicle, VehicleStateViewModel target)
    {
        target.VehicleId = $"{vehicle.VehicleId.SystemId}:{vehicle.VehicleId.ComponentId}";

        target.CustomMode = ((VehicleMode)vehicle.CustomMode).ToString();
        target.VehicleType = vehicle.VehicleType == 2 ? "Quadrotor" : vehicle.VehicleType.ToString();

        target.Autopilot = vehicle.Autopilot == 3 ? "ArduPilot Mega" : vehicle.Autopilot.ToString();
        target.BaseMode = GetBaseMode(vehicle.BaseMode);
        target.SystemStatus = GetSystemStatus(vehicle.SystemStatus);

        target.ConnectionState = vehicle.ConnectionState.ToString();
        target.LastHeartbeatAt = vehicle.LastHeartbeatAt.ToLocalTime().ToString("HH:mm:ss");
        target.Mode = vehicle.Mode.ToString();
        target.IsArmed = vehicle.IsArmed ? "Armed" : "Disarmed";

        target.Latitude = vehicle.Latitude?.ToString("F6") ?? string.Empty;
        target.Longitude = vehicle.Longitude?.ToString("F6") ?? string.Empty;
        target.Altitude = vehicle.Altitude?.ToString("F1") ?? string.Empty;
        target.Altitude += " m";

        target.Roll = RadiansToDegrees(vehicle.Roll ?? 0).ToString("F1");
        target.Roll += " °";
        target.Pitch = RadiansToDegrees(vehicle.Pitch ?? 0).ToString("F1");
        target.Pitch += " °";
        target.Yaw = RadiansToDegrees(vehicle.Yaw ?? 0).ToString("F1");
        target.Yaw += " °";

        target.BatteryRemaining = vehicle.BatteryRemaining?.ToString() ?? string.Empty;
        target.BatteryRemaining += " %";
        target.BatteryVoltage = vehicle.BatteryVoltage?.ToString("F2") ?? string.Empty;
        target.BatteryVoltage += " V";
    }


    /// <summary>
    /// Maps a <see cref="VehicleState"/> to a <see cref="VehicleStateViewModel"/>.
    /// </summary>
    /// <param name="vehicle"></param>
    /// <returns></returns>
    public VehicleStateViewModel MapToViewModel(VehicleState vehicle)
    {
        var target = Map(vehicle);
        Update(vehicle, target);
        return target;
    }

    private double RadiansToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }

    private static string GetBaseMode(byte baseMode)
    {
        var flags = new List<string>();

        if ((baseMode & 0x80) != 0)
        {
            flags.Add("Armed");
        }

        if ((baseMode & 0x40) != 0)
        {
            flags.Add("Manual");
        }

        if ((baseMode & 0x10) != 0)
        {
            flags.Add("Stabilize");
        }

        if ((baseMode & 0x08) != 0)
        {
            flags.Add("Guided");
        }

        if ((baseMode & 0x04) != 0)
        {
            flags.Add("Auto");
        }

        return string.Join(", ", flags);
    }


#pragma warning disable RMG060
    private static string GetSystemStatus(byte state)
    {
        return state switch
        {
            0 => "Uninitialized",
            1 => "Booting",
            2 => "Calibrating",
            3 => "Standby",
            4 => "Active",
            5 => "Critical",
            6 => "Emergency",
            7 => "Poweroff",
            8 => "Flight Termination",
            var _ => state.ToString()
        };
        return state.ToString();
    }
#pragma warning restore RMG060
}
