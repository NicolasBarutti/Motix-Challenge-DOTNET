namespace Motix.Application.DTOs;

public record MotorcycleDto(Guid Id, string? Plate, Guid SectorId);
public record CreateMotorcycleDto(string? Plate, Guid SectorId);
public record UpdateMotorcycleDto(string? Plate, Guid SectorId);