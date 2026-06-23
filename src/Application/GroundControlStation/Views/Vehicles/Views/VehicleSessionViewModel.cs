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

    /// <summary>
    /// Gets the notifications for the vehicle.
    /// </summary>
    //public ObservableCollection<string> Notifications { get; set; } = [];

    [ObservableProperty]
    public partial string Notifications { get; set; } = string.Empty;
}
