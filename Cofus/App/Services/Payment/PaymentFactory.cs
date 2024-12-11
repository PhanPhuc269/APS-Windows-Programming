using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Services.Payment;

namespace App;
public class PaymentFactory
{
    public static IPaymentMethod CreatePaymentMethod(string paymentMethod)
    {
        return paymentMethod switch
        {
            "MoMo" => new MoMoPayment(),
            "VNPay" => new VNPayPayment(),
            "VietQR" => new VietQRPayment(),
            "Cash" => new CashPayment(),
            _ => throw new ArgumentException("Invalid payment method")
        };
    }
}
