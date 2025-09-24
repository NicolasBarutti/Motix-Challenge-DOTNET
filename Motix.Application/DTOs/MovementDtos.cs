namespace Motix.Application.DTOs;

public record MovementDto(Guid Id, Guid MotorcycleId, Guid SectorId, DateTimeOffset OccurredAt);
public record CreateMovementDto(Guid MotorcycleId, Guid SectorId);