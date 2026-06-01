using StarCorp.Business.Enums;

namespace StarCorp.Business.Pricing.Abstractions;

public interface IPricingCalculator
{
    PriceBreakdown Calculate(decimal basePrice, decimal classMultiplier, int passengers);
    PaymentCharge ApplyPayment(decimal amountDue, PaymentMethod method);
}
