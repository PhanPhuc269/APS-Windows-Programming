using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Model;
public class MoMoResponse
{
    public string qrCodeUrl
    {
        get; set;
    }
    public int resultCode
    {
        get; set;
    }
    public string message
    {
        get; set;
    }
    // Add other properties as needed based on the MoMo API response
}