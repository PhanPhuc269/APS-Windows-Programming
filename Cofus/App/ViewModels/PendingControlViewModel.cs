using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Model;
using Microsoft.UI.Xaml.Automation.Provider;

namespace App.ViewModels;
public class PendingControlViewModel
{
    public FullObservableCollection<Invoice> PendingInvoices { get; set; }
    public PendingControlViewModel()
    {
        IDao dao = App.GetService<IDao>();
        PendingInvoices = dao.GetPendingOrders();
    }
    public void CompleteOrder(Invoice order)
    {
        PendingInvoices.Remove(order);
        order.CompleteTime = DateTime.Now;
        App.GetService<IDao>().CompletePendingOrder(order);

    }
}
