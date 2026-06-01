namespace StarCorp.Business.Pricing;

public record PriceBreakdown(
    decimal FarePrice,
    int Passengers,
    decimal Subtotal,
    decimal Taxes,
    decimal ServiceFee,
    decimal AmountDue);
