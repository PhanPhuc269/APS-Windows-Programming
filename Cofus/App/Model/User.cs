using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace App.Model;

public class User : INotifyPropertyChanged
{
    private string id;
    private string name;
    private string role;
    private int accessLevel;
    private string username;
    private string password;
    private int salary;
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public string Id
    {
        get => id;
        set
        {
            if (id != value)
            {
                id = value;
                OnPropertyChanged();
            }
        }
    }
    public string Name
    {
        get => name;
        set
        {
            if (name != value)
            {
                name = value;
                OnPropertyChanged();
            }
        }
    }
    public string Role
    {
        get => role;
        set
        {
            if (role != value)
            {
                role = value;
                OnPropertyChanged();
            }
        }
    }

    public string Username
    {
        get => username;
        set
        {
            if (username != value)
            {
                username = value;
                OnPropertyChanged();
            }
        }
    }

    public string Password
    {
        get => password;
        set
        {
            if (password != value)
            {
                password = value;
                OnPropertyChanged();
            }
        }
    }

    public int AccessLevel
    {
        get => accessLevel;
        set
        {
            if (accessLevel != value)
            {
                accessLevel = value;
                OnPropertyChanged();
            }
        }
    }

    public int Salary
    {
        get => salary;
        set
        {
            if (salary != value)
            {
                salary = value;
                OnPropertyChanged();
            }
        }
    }
    public string Email
    {
        get; set;
    }
}
