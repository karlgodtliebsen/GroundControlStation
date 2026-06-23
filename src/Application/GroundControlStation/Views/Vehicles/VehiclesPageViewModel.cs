using DroneGcs.Core.DomainEvents;
using DroneGcs.Core.Services;

using Microsoft.Extensions.Logging;

namespace GroundControlStationApp.Views.Vehicles;

/// <summary>
/// ViewModel for the VehiclesPage.
/// </summary>
public class VehiclesPageViewModel : BindableObject // UraniumBindableObject //ObservableObject //BindableObject
{
    private readonly IVehicleService vehicleService;
    private readonly IDomainEventHub eventHub;
    private readonly ILogger<VehiclesPageViewModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehiclesPageViewModel"/> class.
    /// </summary>
    /// <param name="vehicleService">The vehicle service.</param>
    /// <param name="eventHub">The event hub.</param>
    /// <param name="logger">The logger.</param>
    public VehiclesPageViewModel(IVehicleService vehicleService, IDomainEventHub eventHub, ILogger<VehiclesPageViewModel> logger)
    {
        this.vehicleService = vehicleService;
        this.eventHub = eventHub;
        this.logger = logger;
        SetupSubscriptions();
    }

    /// <summary>
    /// Sets up the subscriptions for domain events.
    /// </summary>
    public void SetupSubscriptions()
    {
        //eventHub.SubscribeDomainEvent<VehicleArmed>((m) => OnVehicleArmed((VehicleArmed)m));
        //eventHub.SubscribeDomainEvent<VehicleDisarmed>((m) => OnVehicleDisarmed((VehicleDisarmed)m));
        //eventHub.SubscribeDomainEvent<VehicleConnectionStateChanged>((m) => OnVehicleConnectionStateChanged((VehicleConnectionStateChanged)m));
        //eventHub.SubscribeDomainEvent<VehicleModeChanged>((m) => OnVehicleModeChanged((VehicleModeChanged)m));
        eventHub.SubscribeDomainEvent<VehicleRegistered>(OnVehicleRegistered);
        eventHub.SubscribeDomainEvent<VehicleStateUpdated>(OnVehicleStateUpdated);
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
        Dispatcher.Dispatch(() =>
        {
            //VehicleSessions.Clear();
            //VehicleStates.Clear();
            //foreach (var vehicle in vehicleService.GetVehicles())
            //{
            //    VehicleStates.Add(vehicle);
            //    var session = vehicleService.GetVehicle(vehicle.VehicleId);
            //    if (session is not null)
            //    {
            //        VehicleSessions.Add(session);
            //    }
            //}
        });
    }

    private void OnVehicleStateUpdated(VehicleStateUpdated m)
    {
    }

    private void OnVehicleStatusMessageReceived(VehicleStatusMessageReceived m)
    {
    }
}
