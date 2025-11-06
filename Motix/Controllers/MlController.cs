using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Motix.Application.DTOs;
using Motix.Services;

namespace Motix.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ml")]
public class MlController : ControllerBase
{
    private readonly IMlPredictionService _ml;
    public MlController(IMlPredictionService ml) => _ml = ml;

    /// <summary>Prediz se uma moto deve se mover (modelo simples ML.NET).</summary>
    [HttpPost("predict")]
    [ProducesResponseType(typeof(MovementPredictionDto), StatusCodes.Status200OK)]
    public IActionResult Predict([FromBody] MovementFeaturesDto input)
        => Ok(_ml.Predict(input));
}