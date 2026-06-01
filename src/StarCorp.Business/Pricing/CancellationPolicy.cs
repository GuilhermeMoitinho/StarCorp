using StarCorp.Business.Pricing.Abstractions;
using StarCorp.Business.Enums;

namespace StarCorp.Business.Pricing;

public sealed class CancellationPolicy : ICancellationPolicy
{
    private const double FullRefundWindowHours = 24d;

    public RefundResult CalculateRefund(
        FareClass fareClass,
        double daysUntilDeparture,
        bool paid,
        decimal amountPaid,
        double? hoursSincePayment)
    {
        if (!paid)
            return new RefundResult(0m, 0m);

        if (hoursSincePayment is <= FullRefundWindowHours)
            return new RefundResult(100m, amountPaid);

        var percentage = PercentageFor(fareClass, daysUntilDeparture);
        var refund = Math.Round(amountPaid * percentage / 100m, 2, MidpointRounding.AwayFromZero);
        return new RefundResult(percentage, refund);
    }

    private static decimal PercentageFor(FareClass fareClass, double days) => fareClass switch
    {
        FareClass.Economica => days > 7 ? 100m : days >= 2 ? 50m : 0m,
        FareClass.Executiva => days > 7 ? 100m : days >= 2 ? 75m : 25m,
        _ => throw new ArgumentOutOfRangeException(nameof(fareClass), fareClass, "Classe tarifaria desconhecida.")
    };
}
