namespace Motix.Domain.Entities;

public class Movement
{
    public Guid Id { get; set; }
    public Guid MotorcycleId { get; set; }  // moto movimentada
    public Guid SectorId { get; set; }      // setor para onde foi
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}