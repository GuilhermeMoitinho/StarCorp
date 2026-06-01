using StarCorp.Business.Pricing.Abstractions;
using StarCorp.Data.Enums;

namespace StarCorp.Business.Pricing;

/// Politica de cancelamento (secao 5.4):
///                | > 7 dias | 2 a 7 dias | < 2 dias
///   Economica    |   100%   |    50%     |    0%
///   Executiva    |   100%   |    75%     |    25%
/// Regra especial: cancelamento em ate 24h apos o pagamento reembolsa 100%, independente da tabela.
/// Os dias sao medidos pela diferenca exata entre a partida e o instante do cancelamento (TimeSpan.TotalDays).
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
        // Reserva nao paga nao gera reembolso, apenas libera os assentos.
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
