namespace Motix.Application.DTOs;

public record SectorDto(Guid Id, string Code);
public record CreateSectorDto(string Code);
public record UpdateSectorDto(string Code);