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
        currentPage = 1;
        UpdateCurrentPage();
    }

    private int currentPage;
    private const int itemsPerPage = 10;
    private string? searchName;
    private string? searchPhoneNumber;
    private int? MinPoints;
    private int? MaxPoints;

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
    public int TotalPages
    {
        get; set;
    }


    public string? SearchName
    {
        get => searchName;
        set
        {
            if (searchName != value)
            {
                searchName = value;
                UpdateCurrentPage();
            }
        }
    }

    public string? SearchPhoneNumber
    {
        get => searchPhoneNumber;
        set
        {
            if (searchPhoneNumber != value)
            {
                searchPhoneNumber = value;
                UpdateCurrentPage();
            }
        }
    }

    public void UpdateCurrentPage()
    {
        var (customers, count) = App.GetService<IDao>().GetCustomers(currentPage, itemsPerPage, searchName, searchPhoneNumber, MinPoints, MaxPoints);
        Customers = new FullObservableCollection<Customer>(customers);
        TotalPages = (int)Math.Ceiling((double)count / itemsPerPage);
    }

    public void SearchCustomersByName(string searchText, int? minPoints, int? maxPoints)
    {
        SearchName = searchText;
        if(minPoints.HasValue)
            MinPoints = minPoints;
        if (maxPoints.HasValue)
            MaxPoints = maxPoints;
        UpdateCurrentPage();
    }

    public void SearchCustomersByPhoneNumber(string searchText, int? minPoints, int? maxPoints)
    {
        SearchPhoneNumber = searchText;
        if (minPoints.HasValue)
            MinPoints = minPoints;
        if (maxPoints.HasValue)
            MaxPoints = maxPoints;
        // Implement filtering logic based on minPoints and maxPoints if needed
        UpdateCurrentPage();
    }
}
