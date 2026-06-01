using StarCorp.Business.Pricing;
using StarCorp.Business.Enums;

namespace StarCorp.UnitTests.Pricing;

public class PricingCalculatorTests
{
    private readonly PricingCalculator _sut = new();

    [Fact]
    public void Calculate_Should_Compose_Price_For_Economica_Single_Passenger()
    {
        var breakdown = _sut.Calculate(basePrice: 500m, classMultiplier: 1.0m, passengers: 1);

        Assert.Equal(500.00m, breakdown.FarePrice);
        Assert.Equal(500.00m, breakdown.Subtotal);
        Assert.Equal(85.00m, breakdown.Taxes);
        Assert.Equal(29.25m, breakdown.ServiceFee);
        Assert.Equal(614.25m, breakdown.AmountDue);
    }

    [Fact]
    public void Calculate_Should_Multiply_Subtotal_By_Passenger_Count()
    {
        var breakdown = _sut.Calculate(basePrice: 1000m, classMultiplier: 1.0m, passengers: 2);

        Assert.Equal(2000.00m, breakdown.Subtotal);
        Assert.Equal(250.00m, breakdown.Taxes);
        Assert.Equal(112.50m, breakdown.ServiceFee);
        Assert.Equal(2362.50m, breakdown.AmountDue);
    }

    [Fact]
    public void Calculate_Should_Apply_Executiva_Multiplier_On_Base_Price()
    {
        var breakdown = _sut.Calculate(basePrice: 1000m, classMultiplier: 2.5m, passengers: 1);

        Assert.Equal(2500.00m, breakdown.FarePrice);
        Assert.Equal(245.00m, breakdown.Taxes);
        Assert.Equal(137.25m, breakdown.ServiceFee);
        Assert.Equal(2882.25m, breakdown.AmountDue);
    }

    [Fact]
    public void ApplyPayment_CreditCard_Should_Add_3_Percent()
    {
        var charge = _sut.ApplyPayment(2362.50m, PaymentMethod.CreditCard);

        Assert.Equal(70.88m, charge.Adjustment);
        Assert.Equal(2433.38m, charge.Total);
    }

    [Fact]
    public void ApplyPayment_Pix_Should_Discount_5_Percent()
    {
        var charge = _sut.ApplyPayment(2362.50m, PaymentMethod.Pix);

        Assert.Equal(-118.13m, charge.Adjustment);
        Assert.Equal(2244.37m, charge.Total);
    }

    [Fact]
    public void ApplyPayment_Boleto_Should_Add_1_Percent()
    {
        var charge = _sut.ApplyPayment(2362.50m, PaymentMethod.Boleto);

        Assert.Equal(23.63m, charge.Adjustment);
        Assert.Equal(2386.13m, charge.Total);
    }

    [Fact]
    public void ApplyPayment_Should_Throw_For_Unknown_Method()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.ApplyPayment(100m, (PaymentMethod)99));
    }
}
