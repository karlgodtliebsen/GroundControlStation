using CommunityToolkit.Mvvm.ComponentModel;

namespace GroundControlStationApp.Views.Vehicles.Views;

/// <summary>
/// Represents a display-friendly vehicle state for UI binding.
/// </summary>
public sealed partial class VehicleStateViewModel : ObservableObject
{
    //[DisplayName("Selected")]
    ///// <summary>
    ///// Gets or sets a value indicating whether the vehicle is selected in the UI.
    ///// </summary>
    //public bool IsSelected { get; set; } = true;
    //[DataGridIgnore]


    /// <summary>
    /// Gets or sets the vehicle identifier, formatted as system id and component id.
    /// Example: 1:1.
    /// </summary>
    [ObservableProperty]
    public partial string VehicleId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw MAVLink custom mode value.
    /// For ArduCopter this can be mapped to a friendly mode name.
    /// </summary>
    [ObservableProperty]
    public partial string CustomMode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vehicle type as a display-friendly text.
    /// Example: Quadrotor.
    /// </summary>
    [ObservableProperty]
    public partial string VehicleType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the autopilot type as a display-friendly text.
    /// Example: ArduPilot Mega.
    /// </summary>
    [ObservableProperty]
    public partial string Autopilot { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw MAVLink base mode flags as display text.
    /// This may include armed state, custom mode enabled, guided enabled, and similar flags.
    /// </summary>
    [ObservableProperty]
    public partial string BaseMode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MAVLink system status as display-friendly text.
    /// Example: Active, Standby, Critical, Emergency.
    /// </summary>
    [ObservableProperty]
    public partial string SystemStatus { get; set; } = string.Empty;

    ///// <summary>
    ///// Gets or sets the MAVLink protocol version reported by the vehicle.
    ///// </summary>
    //public string MavLinkVersion { get; set; }

    /// <summary>
    /// Gets or sets the current connection state as display-friendly text.
    /// Example: Online, Degraded, Offline.
    /// </summary>
    [ObservableProperty]
    public partial string ConnectionState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp for the last received heartbeat, formatted for display.
    /// </summary>
    [ObservableProperty]
    public partial string LastHeartbeatAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current vehicle mode as UI-friendly text.
    /// Example: Stabilize, Guided, Loiter, RTL, Land.
    /// </summary>
    [ObservableProperty]
    public partial string Mode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the armed state as UI-friendly text.
    /// Example: Armed or Disarmed.
    /// </summary>
    [ObservableProperty]
    public partial string IsArmed { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current latitude, formatted for display.
    /// </summary>
    [ObservableProperty]
    public partial string Latitude { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current longitude, formatted for display.
    /// </summary>
    [ObservableProperty]
    public partial string Longitude { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current altitude, formatted for display.
    /// </summary>
    [ObservableProperty]
    public partial string Altitude { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current roll angle, formatted for display.
    /// </summary>
    [ObservableProperty]
    public partial string Roll { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current pitch angle, formatted for display.
    /// </summary>
    [ObservableProperty]
    public partial string Pitch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current yaw angle, formatted for display.
    /// </summary>
    [ObservableProperty]
    public partial string Yaw { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the remaining battery percentage, formatted for display.
    /// </summary>
    [ObservableProperty]
    public partial string BatteryRemaining { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current battery voltage, formatted for display.
    /// </summary>
    [ObservableProperty]
    public partial string BatteryVoltage { get; set; } = string.Empty;
}
