using FluentValidation;
using StarCorp.Business.Dtos;

namespace StarCorp.Business.Validators;

public sealed class PaymentValidator : AbstractValidator<PaymentRequest>
{
    public PaymentValidator()
    {
        RuleFor(x => x.Method).IsInEnum().WithMessage("Metodo de pagamento invalido.");
    }
}
