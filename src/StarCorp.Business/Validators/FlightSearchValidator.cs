using FluentValidation;
using StarCorp.Business.Dtos;

namespace StarCorp.Business.Validators;

public sealed class FlightSearchValidator : AbstractValidator<FlightSearchRequest>
{
    public FlightSearchValidator()
    {
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue)
            .WithMessage("Preco minimo nao pode ser negativo.");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue)
            .WithMessage("Preco maximo nao pode ser negativo.");

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithName("priceRange")
            .WithMessage("Preco minimo nao pode ser maior que o preco maximo.");

        RuleFor(x => x.FareClass)
            .IsInEnum().When(x => x.FareClass.HasValue)
            .WithMessage("Classe tarifaria invalida.");
    }
}
