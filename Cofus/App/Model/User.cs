using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Model;

public class User
{
    public string Username
    {
        get; set;
    }
    public string Password
    {
        get; set;
    }
    public string Email
    {
        get; set;
    }
    public int AccessLevel
    {
        get; set;
    }
}
