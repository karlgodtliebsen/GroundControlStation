using System.Collections.ObjectModel;

using DroneGcs.Core.DomainEvents;
using DroneGcs.Core.Services;

using Microsoft.Extensions.Logging;

namespace GroundControlStationApp.Views.Vehicles.Views;

/// <summary>
/// ViewModel for managing vehicles.
/// </summary>
public class VehiclesViewModel : BindableObject
{
    private readonly ILogger<VehiclesViewModel> logger;
    private readonly IVehicleService vehicleService;
    private readonly IDomainEventHub eventHub;
    private readonly ModelMapper mapper;

    /// <summary>
    /// Gets the collection of vehicle sessions.
    /// </summary>
    public ObservableCollection<VehicleSessionViewModel> VehicleSessions { get; } = [];

    /// <summary>
    /// Gets the collection of vehicle states.
    /// </summary>
    public ObservableCollection<VehicleStateViewModel> VehicleStates { get; } = [];


    /// <summary>
    /// Gets or sets the collection of selected vehicle states.
    /// </summary>
    public ObservableCollection<VehicleStateViewModel> SelectedItems { get; set; } = [];


    /// <summary>
    /// Initializes a new instance of the <see cref="VehiclesViewModel"/> class.
    /// </summary>
    /// <param name="vehicleService">The vehicle service.</param>
    /// <param name="eventHub">The event hub.</param>
    /// <param name="mapper"></param>
    /// <param name="logger">The logger.</param>
    public VehiclesViewModel(IVehicleService vehicleService, IDomainEventHub eventHub, ModelMapper mapper, ILogger<VehiclesViewModel> logger)
    {
        this.vehicleService = vehicleService;
        this.eventHub = eventHub;
        this.mapper = mapper;
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
            VehicleSessions.Clear();
            var vehicles = vehicleService.GetVehicles();
            foreach (var vehicle in vehicles)
            {
                if (VehicleStates.Any(v => v.VehicleId == vehicle.VehicleId.ToString()))
                {
                    var target = VehicleStates.First(v => v.VehicleId == vehicle.VehicleId.ToString());
                    mapper.Update(vehicle, target);
                }
                else
                {
                    VehicleStates.Add(mapper.MapToViewModel(vehicle));
                }

                var session = vehicleService.GetVehicle(vehicle.VehicleId);
                if (session is not null)
                {
                    VehicleSessions.Add(mapper.MapToViewModel(session));
                }
            }

            var allStates = VehicleStates.ToList();
            foreach (var vehicle in allStates)
            {
                if (!vehicles.Any(v => v.VehicleId.ToString() == vehicle.VehicleId))
                {
                    VehicleStates.Remove(vehicle);
                }
            }
        });
    }

    private void OnVehicleStateUpdated(VehicleStateUpdated m)
    {
        Dispatcher.Dispatch(() =>
        {
            var target = VehicleStates.FirstOrDefault(v => v.VehicleId == m.VehicleId.ToString());
            if (target is not null)
            {
                mapper.Update(m.VehicleState, target);
            }
        });
    }

    private void OnVehicleStatusMessageReceived(VehicleStatusMessageReceived m)
    {
    }
}
