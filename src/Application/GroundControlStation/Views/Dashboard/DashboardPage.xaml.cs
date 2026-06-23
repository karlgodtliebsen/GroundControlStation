using GroundControlStationApp.Views.Vehicles.Views;

using UraniumUI.Pages;

namespace GroundControlStationApp.Views.Dashboard;

/// <summary>
/// Represents the dashboard page of the application.
/// </summary>
public partial class DashboardPage : UraniumContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardPage"/> class.
    /// </summary>
    public DashboardPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardPage"/> class with the specified view model.
    /// </summary>
    /// <param name="vehiclesView"></param>
    /// <param name="viewModel">The view model to be used by the page.</param>
    public DashboardPage(VehiclesView vehiclesView, DashboardPageViewModel viewModel) : this()
    {
        BindingContext = viewModel;
        var view = FindByName("VehiclesView") as StackLayout;
        view!.Children.Add(vehiclesView);
        // PickerField.SelectedValueChangedCommand = viewModel.ExecuteCommand;
    }
}
