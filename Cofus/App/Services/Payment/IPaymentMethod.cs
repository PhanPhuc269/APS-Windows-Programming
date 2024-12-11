using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Model;

namespace App;
public interface IPaymentMethod
{
    Task<bool> ProcessPayment(Invoice invoice);
}
