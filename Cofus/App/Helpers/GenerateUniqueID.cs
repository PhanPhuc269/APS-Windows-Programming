using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App;
public class GenerateUniqueID
{
    public static string GenerateUniqueOrderID()
    {
        return DateTime.Now.Ticks.ToString();
    }
}
