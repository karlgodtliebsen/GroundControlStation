namespace GroundControlStationApp.Views.Vehicles.Views;

/// <summary>
/// Represents the view for displaying and interacting with vehicles.
/// </summary>
public partial class VehiclesView : ContentView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VehiclesView"/> class.
    /// </summary>
    public VehiclesView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VehiclesView"/> class with the specified view model.
    /// </summary>
    /// <param name="viewModel">The view model to bind to the view.</param>
    public VehiclesView(VehiclesViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}
