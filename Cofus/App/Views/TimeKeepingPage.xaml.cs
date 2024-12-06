using App.ViewModels;

using CommunityToolkit.WinUI.UI.Controls;

using Microsoft.UI.Xaml.Controls;

namespace App.Views;

public sealed partial class TimeKeepingPage : Page
{
    public TimeKeepingViewModel ViewModel
    {
        get;
    }

    public TimeKeepingPage()
    {
        ViewModel = App.GetService<TimeKeepingViewModel>();
        InitializeComponent();
    }

    private void OnViewStateChanged(object sender, ListDetailsViewState e)
    {
        if (e == ListDetailsViewState.Both)
        {
            ViewModel.EnsureItemSelected();
        }
    }
}
