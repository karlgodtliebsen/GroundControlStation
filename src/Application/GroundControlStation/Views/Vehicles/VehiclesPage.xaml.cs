using GroundControlStationApp.Views.Vehicles.Views;

using UraniumUI.Pages;

namespace GroundControlStationApp.Views.Vehicles;

/// <summary>
/// Represents the page for managing and displaying vehicles in the ground control station application.
/// </summary>
public partial class VehiclesPage : UraniumContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VehiclesPage"/> class.
    /// </summary>
    public VehiclesPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VehiclesPage"/> class with the specified view model.
    /// </summary>
    /// <param name="vehiclesView">The view to be used in the page.</param>
    /// <param name="viewModel">The view model to be used as the binding context.</param>
    public VehiclesPage(VehiclesView vehiclesView, VehiclesPageViewModel viewModel) : this()
    {
        BindingContext = viewModel;
        var view = FindByName("VehiclesView") as StackLayout;
        view!.Children.Add(vehiclesView);
    }
}
