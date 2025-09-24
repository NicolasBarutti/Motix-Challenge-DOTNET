namespace Motix.Domain.Entities;

public class Motorcycle
{
    public Guid Id { get; set; }
    public string? Plate { get; set; }
    public Guid SectorId { get; set; }   // referência simples ao setor
}