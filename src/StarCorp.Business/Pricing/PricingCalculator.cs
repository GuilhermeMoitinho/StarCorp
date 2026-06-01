using StarCorp.Business.Pricing.Abstractions;
using StarCorp.Data.Enums;

namespace StarCorp.Business.Pricing;

public sealed class PricingCalculator : IPricingCalculator
{
    private const decimal TaxRate = 0.08m;
    private const decimal FixedTaxPerPassenger = 45m;
    private const decimal ServiceFeeRate = 0.05m;

    public PriceBreakdown Calculate(decimal basePrice, decimal classMultiplier, int passengers)
    {
        var farePrice = Round(basePrice * classMultiplier);
        var subtotal = Round(farePrice * passengers);
        var taxes = Round(subtotal * TaxRate + FixedTaxPerPassenger * passengers);
        var serviceFee = Round((subtotal + taxes) * ServiceFeeRate);
        var amountDue = subtotal + taxes + serviceFee;

        return new PriceBreakdown(farePrice, passengers, subtotal, taxes, serviceFee, amountDue);
    }

    public PaymentCharge ApplyPayment(decimal amountDue, PaymentMethod method)
    {
        var rate = method switch
        {
            PaymentMethod.CreditCard => 0.03m,
            PaymentMethod.Pix => -0.05m,
            PaymentMethod.Boleto => 0.01m,
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Metodo de pagamento desconhecido.")
        };

        var adjustment = Round(amountDue * rate);
        var total = amountDue + adjustment;

        return new PaymentCharge(method, adjustment, total);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
