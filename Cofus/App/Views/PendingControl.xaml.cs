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
        this.DataContext = ViewModel;

        // Initialize and start a timer for updating RemainingTimes
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void CompleteOrderButton_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var order = button.DataContext as Invoice;
        ViewModel.CompleteOrder(order);
    }

    private DispatcherTimer _timer;

    private void Timer_Tick(object sender, object e)
    {
        foreach (var invoice in ViewModel.PendingInvoices)
        {
            var container = PendingOrdersListView.ContainerFromItem(invoice) as ListViewItem;
            if (container != null)
            {
                var textBlock = FindTextBlockByTag(container, invoice.InvoiceNumber);
                if (textBlock != null)
                {
                    textBlock.Text = invoice.RemainingTime;
                }
            }
        }
    }

    private TextBlock FindTextBlockByTag(DependencyObject parent, int tag)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is TextBlock textBlock && textBlock.Tag is int textBlockTag && textBlockTag == tag)
            {
                return textBlock;
            }

            var foundChild = FindTextBlockByTag(child, tag);
            if (foundChild != null)
            {
                return foundChild;
            }
        }

        return null;
    }

    // Stop the timer when the control is unloaded
    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }
    }
}
