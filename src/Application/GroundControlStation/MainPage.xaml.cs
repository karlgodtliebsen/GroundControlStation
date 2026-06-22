using InputKit.Shared.Controls;

using UraniumUI.Pages;

namespace GroundControlStationApp;

public partial class MainPage : UraniumContentPage
{
    public MainPage()
    {
        SelectionView.GlobalSetting.CornerRadius = 0;
        InitializeComponent();
    }

    public MainPage(MainPageViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}
