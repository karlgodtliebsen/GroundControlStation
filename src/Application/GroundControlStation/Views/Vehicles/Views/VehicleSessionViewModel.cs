using System.Net;

using CommunityToolkit.Mvvm.ComponentModel;

using DroneGcs.Core.Models;

namespace GroundControlStationApp.Views.Vehicles.Views;

/// <summary>
/// Represents a session for a vehicle, managing its state and handling updates.
/// </summary>
public partial class VehicleSessionViewModel : ObservableObject
{
    /// <summary>
    /// Gets the unique identifier of the vehicle.
    /// </summary>
    public VehicleId Id { get; set; }


    [ObservableProperty] public partial IPEndPoint IpEndPoint { get; set; } = new(IPAddress.Any, 0);


    /// <summary>
    /// Gets the notifications for the vehicle.
    /// </summary>
    [ObservableProperty]
    public partial string Notifications { get; set; } = string.Empty;
}
