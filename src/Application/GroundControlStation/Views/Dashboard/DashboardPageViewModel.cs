using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DroneGcs.Core.Services;

using DroneGs.MavLink.Services;

using GroundControlStationApp.Configuration;

namespace GroundControlStationApp.Views.Dashboard;

/// <summary>
/// Represents the view model for the dashboard page.
/// </summary>
public partial class DashboardPageViewModel : ObservableObject
{
    private readonly IMavLinkConnection connection = null!;
    private readonly IVehicleMessagePump messagePump = null!;
    private readonly ICommandService commandService = null!;
    private readonly InitializeSitl initializeSitl = null!;

    [ObservableProperty] public partial string SelectedCommand { get; set; } = null!;
    [ObservableProperty] public partial string CommandName { get; set; } = null!;

    /// <summary>
    /// Gets the list of available commands.
    /// </summary>
    public IList<string> Commands { get; } =
    [
        "help", "vehicles", "state", "arm", "disarm", "mode"
    ];


    //partial void OnSelectedCommandChanged(string value)
    //{
    //    var result = commandService.Run(value, CancellationToken.None);
    //}


    [RelayCommand]
    private void Select()
    {
        SelectedCommand = Commands[1];
    }

    private async Task ExecuteCommandAsync(string line, CancellationToken cancellationToken)
    {
        // var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var result = await commandService.Run(line, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardPageViewModel"/> class with the specified SITL initializer.
    /// </summary>
    /// <param name="initializeSitl">The SITL initializer to be used by the view model.</param>
    /// <param name="connection">The MAVLink connection to be used by the view model.</param>
    /// <param name="messagePump">The vehicle message pump to be used by the view model.</param>
    /// <param name="commandService"></param>
    public DashboardPageViewModel(IMavLinkConnection connection, IVehicleMessagePump messagePump, ICommandService commandService, InitializeSitl initializeSitl)
    {
        this.connection = connection;
        this.messagePump = messagePump;
        this.commandService = commandService;
        this.initializeSitl = initializeSitl;
        SetupSubscriptions();
    }


    /// <summary>
    /// Sets up the subscriptions for domain events.
    /// </summary>
    private void SetupSubscriptions()
    {
        var conn = connection;
        var mp = messagePump;
        _ = Task.Run(() => conn.StartAsync(CancellationToken.None), CancellationToken.None);
        _ = Task.Run(() => mp.StartAsync(CancellationToken.None), CancellationToken.None);

        initializeSitl.Initialize(CancellationToken.None).WaitAsync(CancellationToken.None);

        //eventHub.SubscribeDomainEvent<VehicleArmed>((m) => OnVehicleArmed((VehicleArmed)m));
        //eventHub.SubscribeDomainEvent<VehicleDisarmed>((m) => OnVehicleDisarmed((VehicleDisarmed)m));
        //eventHub.SubscribeDomainEvent<VehicleConnectionStateChanged>((m) => OnVehicleConnectionStateChanged((VehicleConnectionStateChanged)m));
        //eventHub.SubscribeDomainEvent<VehicleModeChanged>((m) => OnVehicleModeChanged((VehicleModeChanged)m));
        //eventHub.SubscribeDomainEvent<VehicleRegistered>((m) => OnVehicleRegistered((VehicleRegistered)m));
        //eventHub.SubscribeDomainEvent<VehicleStateUpdated>((m) => OnVehicleStateUpdated((VehicleStateUpdated)m));
        //eventHub.SubscribeDomainEvent<VehicleStatusMessageReceived>((m) => OnVehicleStatusMessageReceived((VehicleStatusMessageReceived)m));
    }
}
