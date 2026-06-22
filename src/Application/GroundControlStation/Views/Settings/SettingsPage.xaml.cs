using UraniumUI.Pages;

namespace GroundControlStationApp.Views.Settings;

/// <summary>
/// ViewModel for the SettingsPage.
/// </summary>
public partial class SettingsPage : UraniumContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPage"/> class.
    /// </summary>
    public SettingsPage()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPage"/> class with the specified view model.
    /// </summary>
    /// <param name="viewModel">The view model for the page.</param>
    public SettingsPage(SettingsPageViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}
