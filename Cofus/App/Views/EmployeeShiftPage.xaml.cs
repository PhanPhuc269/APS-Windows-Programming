using App.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace App.Views;

public sealed partial class EmployeeShiftPage : Page
{
    public EmployeeShiftViewModel ViewModel
    {
        get;
    }

    public EmployeeShiftPage()
    {
        ViewModel = App.GetService<EmployeeShiftViewModel>();
        InitializeComponent();
    }
}
