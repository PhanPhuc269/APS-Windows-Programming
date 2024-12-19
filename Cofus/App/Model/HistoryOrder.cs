using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Model;

public class HistoryOrder
{

    public int Id
    {
        get; set;
    }
    public string Name
    {
        get; set;
    }
    public string Image
    {
        get; set;
    }
    public int TypeBeverageId
    {
        get; set;
    }
    public int Quantity
    {
        get; set;
    }
    public DateTime OrderTime
    {
        get; set;
    }

}
