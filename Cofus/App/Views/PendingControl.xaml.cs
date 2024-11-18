using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using App.ViewModels;
using App.Model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App.Views;
public sealed partial class PendingControl : UserControl
{
    public PendingControlViewModel ViewModel { get; set; }

    public PendingControl()
    {
        this.InitializeComponent();
        ViewModel = new PendingControlViewModel();
    }

    private void CompleteOrderButton_Click(object sender, RoutedEventArgs e)
    {
        // Handle the completion of the order
        var button = sender as Button;
        var order = button.DataContext as Invoice;
        ViewModel.CompleteOrder(order);
    }
}
