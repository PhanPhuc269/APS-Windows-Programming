using App.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.ViewModels;

public partial class EmployeeManagementViewModel : ObservableRecipient
{
    private readonly IDao _dao;
    private int _currentPage;
    private int _totalPages;

    public FullObservableCollection<User> Employee
    {
        get; set;
    }
    public FullObservableCollection<User> FilteredUser
    {
        get; set;
    }
    public User SelectedEmployee
    {
        get; set;
    }

    public EmployeeManagementViewModel(IDao dao)
    {
        _dao = dao;
        Employee = new FullObservableCollection<User>(_dao.GetAllUsers());
        FilteredUser = new FullObservableCollection<User>(Employee.Take(10).ToList());
        CurrentPage = 1;
        TotalPages = (int)Math.Ceiling((double)Employee.Count / 10); // Assuming 10 items per page
    }
    public void UpdateCurrentPage()
    {
        FilteredUser.Clear();
        var users = Employee.Skip((CurrentPage - 1) * 10).Take(10).ToList();
        foreach (var user in users)
        {
            FilteredUser.Add(user);
        }
    }
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (SetProperty(ref _currentPage, value))
            {
                UpdateCurrentPage();
            }
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        set => SetProperty(ref _totalPages, value);
    }

    public void SearchEmployees(string keyword)
    {
        FilteredUser.Clear();
        var result = _dao.SearchEmployees(keyword);
        foreach (var user in result)
        {
            FilteredUser.Add(user);
        }
    }

    public void DeleteEmployee(string userId)
    {
        if (_dao.DeleteEmployee(userId))
        {
            var user = Employee.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                Employee.Remove(user);
                FilteredUser.Remove(user);
            }
        }
    }

    public void UpdateEmployee(User user)
    {
        if (_dao.UpdateEmployee(user))
        {
            var existingUser = Employee.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                existingUser.Name = user.Name;
                existingUser.Role = user.Role;
                existingUser.AccessLevel = user.AccessLevel;
                existingUser.Username = user.Username;
                existingUser.Password = user.Password;
                existingUser.Salary = user.Salary;
            }
        }
    }

    public async Task LoadEmployees()
    {
        var users = await Task.Run(() => _dao.GetAllUsers());
        Employee.Clear();
        foreach (var user in users)
        {
            Employee.Add(user);
        }
        TotalPages = (int)Math.Ceiling((double)Employee.Count / 10); // Assuming 10 items per page
        UpdateCurrentPage();
    }

    
}
