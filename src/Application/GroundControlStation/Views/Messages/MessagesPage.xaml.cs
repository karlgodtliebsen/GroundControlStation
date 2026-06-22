using UraniumUI.Pages;

namespace GroundControlStationApp.Views.Messages;

/// <summary>
/// Represents the messages page in the application.
/// </summary>
public partial class MessagesPage : UraniumContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessagesPage"/> class.
    /// </summary>
    public MessagesPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagesPage"/> class with the specified view model.
    /// </summary>
    /// <param name="viewModel">The view model to bind to the page.</param>
    public MessagesPage(MessagesPageViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}
