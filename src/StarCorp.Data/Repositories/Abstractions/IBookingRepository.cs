using StarCorp.Data.Entities;
using StarCorp.Data.Queries;

namespace StarCorp.Data.Repositories.Abstractions;

public interface IBookingRepository
{
    /// Cria a reserva e os passageiros numa transacao, baixando os assentos de forma atomica.
    /// Retorna o id da reserva, ou null quando nao ha assentos disponiveis na classe escolhida.
    Task<int?> CreateAsync(NewBooking booking, IReadOnlyList<NewPassenger> passengers, CancellationToken ct);

    Task<BookingAggregate?> GetAggregateAsync(int id, CancellationToken ct);

    /// Registra o pagamento e marca a reserva como paga. Retorna false quando a reserva ja saiu do estado pendente.
    Task<bool> RegisterPaymentAsync(int bookingId, NewPayment payment, CancellationToken ct);

    /// Cancela a reserva, registra o reembolso e devolve os assentos. Retorna false quando ja nao esta cancelavel.
    Task<bool> CancelAsync(Booking booking, decimal refundPercentage, decimal refundAmount, DateTime cancelledAtUtc, CancellationToken ct);
}
