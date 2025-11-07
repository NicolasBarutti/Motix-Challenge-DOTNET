using FluentAssertions;
using Motix.Application.DTOs;   // ✅ DTOs (com O maiúsculo)
using Motix.Services;
using Xunit;

namespace Motix.Tests;

public class MlPredictionServiceTests
{
    [Fact]
    public void Predict_ShouldMove_WhenActivityIsHigh()
    {
        var svc = new MlPredictionService();
        var input = new MovementFeaturesDto(8, 3, 5);

        var result = svc.Predict(input);

        result.ShouldMove.Should().BeTrue();
        result.Score.Should().BeGreaterThan(0);
        result.Probability.Should().BeInRange(0f, 1f);
    }

    [Fact]
    public void Predict_ShouldNotMove_WhenActivityIsLow()
    {
        var svc = new MlPredictionService();
        var input = new MovementFeaturesDto(0, 0, 200);

        var result = svc.Predict(input);

        result.ShouldMove.Should().BeFalse();
        result.Score.Should().BeLessOrEqualTo(0);
        result.Probability.Should().BeInRange(0f, 1f);
    }
}