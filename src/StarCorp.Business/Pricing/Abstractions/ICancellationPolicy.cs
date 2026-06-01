using StarCorp.Data.Enums;

namespace StarCorp.Business.Pricing.Abstractions;

public interface ICancellationPolicy
{
    RefundResult CalculateRefund(
        FareClass fareClass,
        double daysUntilDeparture,
        bool paid,
        decimal amountPaid,
        double? hoursSincePayment);
}
