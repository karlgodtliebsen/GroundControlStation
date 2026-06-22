using System.Collections.ObjectModel;

using DroneGcs.Core.DomainEvents;
using DroneGcs.Core.Models;
using DroneGcs.Core.Services;

using Microsoft.Extensions.Logging;

namespace GroundControlStationApp.Views.Vehicles.Views;

/// <summary>
/// ViewModel for managing vehicles.
/// </summary>
public class VehiclesViewModel : BindableObject
{
    private readonly IVehicleService vehicleService;
    private readonly IDomainEventHub eventHub;
    private readonly ILogger<VehiclesViewModel> logger;
    private readonly IDispatcher dispatcher;

    /// <summary>
    /// Gets the collection of vehicle sessions.
    /// </summary>
    public ObservableCollection<VehicleSession> VehicleSessions { get; } = [];

    /// <summary>
    /// Gets the collection of vehicle states.
    /// </summary>
    public ObservableCollection<VehicleState> VehicleStates { get; } = [];


    //VehicleSession
    //[ObservableProperty] public partial VehicleSession SelectedVehicleSession { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VehiclesViewModel"/> class.
    /// </summary>
    /// <param name="vehicleService">The vehicle service.</param>
    /// <param name="eventHub">The event hub.</param>
    /// <param name="dispatcher">The dispatcher.</param>
    /// <param name="logger">The logger.</param>
    public VehiclesViewModel(IVehicleService vehicleService, IDomainEventHub eventHub, IDispatcher dispatcher, ILogger<VehiclesViewModel> logger)
    {
        this.vehicleService = vehicleService;
        this.eventHub = eventHub;
        this.dispatcher = dispatcher;
        this.logger = logger;
        SetupSubscriptions();
    }

    /// <summary>
    /// Sets up the subscriptions for domain events.
    /// </summary>
    private void SetupSubscriptions()
    {
        //eventHub.SubscribeDomainEvent<VehicleArmed>((m) => OnVehicleArmed((VehicleArmed)m));
        //eventHub.SubscribeDomainEvent<VehicleDisarmed>((m) => OnVehicleDisarmed((VehicleDisarmed)m));
        //eventHub.SubscribeDomainEvent<VehicleConnectionStateChanged>((m) => OnVehicleConnectionStateChanged((VehicleConnectionStateChanged)m));
        //eventHub.SubscribeDomainEvent<VehicleModeChanged>((m) => OnVehicleModeChanged((VehicleModeChanged)m));
        eventHub.SubscribeDomainEvent<VehicleRegistered>((m) => OnVehicleRegistered((VehicleRegistered)m));
        //eventHub.SubscribeDomainEvent<VehicleStateUpdated>((m) => OnVehicleStateUpdated((VehicleStateUpdated)m));
        //eventHub.SubscribeDomainEvent<VehicleStatusMessageReceived>((m) => OnVehicleStatusMessageReceived((VehicleStatusMessageReceived)m));
    }

    private void OnVehicleArmed(VehicleArmed m)
    {
    }

    private void OnVehicleDisarmed(VehicleDisarmed m)
    {
    }

    private void OnVehicleConnectionStateChanged(VehicleConnectionStateChanged m)
    {
    }

    private void OnVehicleModeChanged(VehicleModeChanged m)
    {
    }

    private void OnVehicleRegistered(VehicleRegistered m)
    {
        dispatcher.Dispatch(() =>
        {
            VehicleSessions.Clear();
            VehicleStates.Clear();
            foreach (var vehicle in vehicleService.GetVehicles())
            {
                VehicleStates.Add(vehicle);
                var session = vehicleService.GetVehicle(vehicle.VehicleId);
                if (session is not null)
                {
                    VehicleSessions.Add(session);
                }
            }
        });
    }

    private void OnVehicleStateUpdated(VehicleStateUpdated m)
    {
    }

    private void OnVehicleStatusMessageReceived(VehicleStatusMessageReceived m)
    {
    }
}
