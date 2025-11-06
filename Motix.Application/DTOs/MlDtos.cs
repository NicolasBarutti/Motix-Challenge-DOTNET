namespace Motix.Application.DTOs;

public record MovementFeaturesDto(float MovementsCount, float SectorChangesLast7Days, float HoursSinceLastMove);
public record MovementPredictionDto(bool ShouldMove, float Score, float Probability);