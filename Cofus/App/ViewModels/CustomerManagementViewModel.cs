using CommunityToolkit.Mvvm.ComponentModel;
using App.Model;
using System.Collections.ObjectModel;

namespace App.ViewModels;

public partial class CustomerManagementViewModel : ObservableRecipient
{
    public FullObservableCollection<Customer> Customers { get; set; }

    public CustomerManagementViewModel()
    {
        // Initialize the Customers collection
        Customers = new FullObservableCollection<Customer>();

        // Initialize sample data
        Customers.Add(new Customer
        {
            CustomerId = 1,
            CustomerName = "John Doe",
            PhoneNumber = "1234567890",
            Email = "john.doe@example.com",
            Points = 100
        });

        Customers.Add(new Customer
        {
            CustomerId = 2,
            CustomerName = "Jane Smith",
            PhoneNumber = "0987654321",
            Email = "jane.smith@example.com",
            Points = 200
        });

        Customers.Add(new Customer
        {
            CustomerId = 3,
            CustomerName = "Alice Johnson",
            PhoneNumber = "5555555555",
            Email = "alice.johnson@example.com",
            Points = 150
        });

        FilteredCustomers = new FullObservableCollection<Customer>();
        currentPage = 0;
        UpdateCurrentPage();
    }

    public FullObservableCollection<Customer> FilteredCustomers { get; set; }

    private int currentPage;
    private const int itemsPerPage = 10;

    public int CurrentPage
    {
        get => currentPage;
        set
        {
            if (currentPage != value)
            {
                currentPage = value;
                UpdateCurrentPage();
            }
        }
    }

    public void UpdateCurrentPage()
    {
        FilteredCustomers.Clear();
        var items = Customers.Skip(CurrentPage * itemsPerPage).Take(itemsPerPage).ToList();
        foreach (var item in items)
        {
            FilteredCustomers.Add(item);
        }
    }

    public int TotalPages()
    {
        return (int)Math.Ceiling((double)Customers.Count / itemsPerPage);
    }

    public void SearchCustomersByName(string searchText, int? minPoints, int? maxPoints)
    {
        var filtered = Customers.Where(c => c.CustomerName.ToLower().Contains(searchText.ToLower()));
        if (minPoints.HasValue)
        {
            filtered = filtered.Where(c => c.Points >= minPoints.Value);
        }
        if (maxPoints.HasValue)
        {
            filtered = filtered.Where(c => c.Points <= maxPoints.Value);
        }
        FilteredCustomers = new FullObservableCollection<Customer>(filtered);
        currentPage = 0;
        UpdateCurrentPage();
    }

    public void SearchCustomersByPhoneNumber(string searchText, int? minPoints, int? maxPoints)
    {
        var filtered = Customers.Where(c => c.PhoneNumber.ToLower().Contains(searchText.ToLower()));
        if (minPoints.HasValue)
        {
            filtered = filtered.Where(c => c.Points >= minPoints.Value);
        }
        if (maxPoints.HasValue)
        {
            filtered = filtered.Where(c => c.Points <= maxPoints.Value);
        }
        FilteredCustomers = new FullObservableCollection<Customer>(filtered);
        currentPage = 0;
        UpdateCurrentPage();
    }
}
