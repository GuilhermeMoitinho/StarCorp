using StarCorp.Business.Pricing;
using StarCorp.Data.Enums;

namespace StarCorp.UnitTests.Pricing;

public class CancellationPolicyTests
{
    private readonly CancellationPolicy _sut = new();
    private const double OutsideWindow = 48d; // fora da janela de 24h apos o pagamento

    public static IEnumerable<object[]> PolicyMatrix =>
    [
        // classe, dias ate a partida, percentual esperado
        [FareClass.Economica, 10d, 100m],
        [FareClass.Economica, 8d, 100m],
        [FareClass.Economica, 7d, 50m],
        [FareClass.Economica, 5d, 50m],
        [FareClass.Economica, 2d, 50m],
        [FareClass.Economica, 1.99d, 0m],
        [FareClass.Economica, 1d, 0m],
        [FareClass.Executiva, 10d, 100m],
        [FareClass.Executiva, 7d, 75m],
        [FareClass.Executiva, 2d, 75m],
        [FareClass.Executiva, 1.99d, 25m],
        [FareClass.Executiva, 1d, 25m]
    ];

    [Theory]
    [MemberData(nameof(PolicyMatrix))]
    public void CalculateRefund_Should_Follow_Policy_Table(FareClass fareClass, double days, decimal expectedPercentage)
    {
        var result = _sut.CalculateRefund(fareClass, days, paid: true, amountPaid: 1000m, hoursSincePayment: OutsideWindow);

        Assert.Equal(expectedPercentage, result.RefundPercentage);
        Assert.Equal(Math.Round(1000m * expectedPercentage / 100m, 2), result.RefundAmount);
    }

    [Fact]
    public void CalculateRefund_Should_Refund_Full_Amount_Within_24h_Of_Payment()
    {
        // Partida amanha cairia no bucket de 0%, mas a janela de 24h apos o pagamento garante 100%.
        var result = _sut.CalculateRefund(FareClass.Economica, daysUntilDeparture: 1d, paid: true, amountPaid: 614.25m, hoursSincePayment: 10d);

        Assert.Equal(100m, result.RefundPercentage);
        Assert.Equal(614.25m, result.RefundAmount);
    }

    [Fact]
    public void CalculateRefund_Should_Treat_24h_Boundary_As_Inside_Window()
    {
        var result = _sut.CalculateRefund(FareClass.Executiva, daysUntilDeparture: 1d, paid: true, amountPaid: 1000m, hoursSincePayment: 24d);

        Assert.Equal(100m, result.RefundPercentage);
    }

    [Fact]
    public void CalculateRefund_Should_Return_Zero_When_Not_Paid()
    {
        var result = _sut.CalculateRefund(FareClass.Executiva, daysUntilDeparture: 1d, paid: false, amountPaid: 0m, hoursSincePayment: null);

        Assert.Equal(0m, result.RefundPercentage);
        Assert.Equal(0m, result.RefundAmount);
    }
}
