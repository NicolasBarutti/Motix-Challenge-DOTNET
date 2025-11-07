using Microsoft.ML;
using Microsoft.ML.Data;
using Motix.Application.DTOs;

namespace Motix.Services;

public interface IMlPredictionService
{
    MovementPredictionDto Predict(MovementFeaturesDto input);
}

public class MlPredictionService : IMlPredictionService
{
    private readonly PredictionEngine<ModelInput, ModelOutput> _engine;

    public MlPredictionService()
    {
        var ml = new MLContext(seed: 7);

        // Dados de treino simples (sintéticos)
        var trainData = new List<ModelInput>
        {
            new(0,  0, 200, false),
            new(2,  0,  80, false),
            new(5,  1,  24, true ),
            new(10, 3,   5, true ),
            new(8,  2,  10, true ),
            new(1,  0, 120, false),
        };

        var data = ml.Data.LoadFromEnumerable(trainData);

        var pipeline =
            ml.Transforms.Concatenate("Features",
                    nameof(ModelInput.MovementsCount),
                    nameof(ModelInput.SectorChanges7d),
                    nameof(ModelInput.HoursSinceLast))
              .Append(ml.Transforms.NormalizeMinMax("Features"))
              .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression());

        var model = pipeline.Fit(data);
        _engine = ml.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
    }

    public MovementPredictionDto Predict(MovementFeaturesDto input)
    {
        var raw = _engine.Predict(new ModelInput(
            input.MovementsCount,
            input.SectorChangesLast7Days,
            input.HoursSinceLastMove,
            false));

        return new MovementPredictionDto(
            ShouldMove: raw.Score > 0,
            Score: raw.Score,
            Probability: raw.Probability
        );
    }

    private sealed record ModelInput(float MovementsCount, float SectorChanges7d, float HoursSinceLast, bool Label);

    private sealed class ModelOutput
    {
        public bool PredictedLabel { get; set; }
        public float Score { get; set; }
        public float Probability { get; set; }
    }
}
