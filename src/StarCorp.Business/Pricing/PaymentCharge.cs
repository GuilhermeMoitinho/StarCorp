using StarCorp.Data.Enums;

namespace StarCorp.Business.Pricing;

public record PaymentCharge(PaymentMethod Method, decimal Adjustment, decimal Total);
