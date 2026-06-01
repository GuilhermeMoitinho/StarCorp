using FluentValidation;
using StarCorp.Business.Dtos;

namespace StarCorp.Business.Validators;

public sealed class CreateBookingValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Cliente e obrigatorio.");
        RuleFor(x => x.FlightId).GreaterThan(0).WithMessage("Voo e obrigatorio.");
        RuleFor(x => x.FareClass).IsInEnum().WithMessage("Classe tarifaria invalida.");

        RuleFor(x => x.Passengers).NotEmpty().WithMessage("Informe ao menos um passageiro.");

        RuleForEach(x => x.Passengers).ChildRules(passenger =>
        {
            passenger.RuleFor(p => p.Name).NotEmpty().WithMessage("Nome do passageiro e obrigatorio.");
            passenger.RuleFor(p => p.Document).NotEmpty().WithMessage("Documento do passageiro e obrigatorio.");
        });
    }
}
