using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motix.Application.DTOs;
using Motix.Domain.Entities;
using Motix.Extensions;
using Motix.Infrastructure.Persistence;
using Motix.Models;
using Motix.Services;
using Asp.Versioning;


namespace Motix.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MovementsController : ControllerBase
{
    private readonly MotixDbContext _ctx;
    public MovementsController(MotixDbContext ctx) => _ctx = ctx;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var paging = new PagingParameters(page, pageSize);
        var paged = await _ctx.Movements.AsNoTracking()
            .OrderByDescending(mv => mv.OccurredAt)
            .ToPagedAsync(paging, ct);

        var items = paged.Items.Cast<Movement>().Select(mv => new
        {
            data = new MovementDto(mv.Id, mv.MotorcycleId, mv.SectorId, mv.OccurredAt),
            _links = LinkFactory.MovementLinks(HttpContext, mv.Id)
        });

        return Ok(new PagedResult<object>(items, paged.Page, paged.PageSize, paged.TotalCount));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MovementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var mv = await _ctx.Movements.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (mv is null) return NotFound();

        return Ok(new
        {
            data = new MovementDto(mv.Id, mv.MotorcycleId, mv.SectorId, mv.OccurredAt),
            _links = LinkFactory.MovementLinks(HttpContext, mv.Id)
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(MovementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CreateMovementDto input, CancellationToken ct = default)
    {
        if (input.MotorcycleId == Guid.Empty || input.SectorId == Guid.Empty)
            return BadRequest(new { error = "MotorcycleId e SectorId são obrigatórios" });

        var mv = new Movement
        {
            Id = Guid.NewGuid(),
            MotorcycleId = input.MotorcycleId,
            SectorId = input.SectorId,
            OccurredAt = DateTimeOffset.UtcNow
        };

        try
        {
            _ctx.Movements.Add(mv);
            await _ctx.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("ORA-02291") == true)
        {
            return BadRequest(new { error = "MotorcycleId ou SectorId inexistente (violação de FK)" });
        }

        var dto = new MovementDto(mv.Id, mv.MotorcycleId, mv.SectorId, mv.OccurredAt);
        return CreatedAtAction(nameof(GetById),
            new { version = "1.0", id = mv.Id },
            new { data = dto, _links = LinkFactory.MovementLinks(HttpContext, mv.Id) });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var mv = await _ctx.Movements.FindAsync(new object?[] { id }, ct);
        if (mv is null) return NotFound();

        _ctx.Movements.Remove(mv);
        await _ctx.SaveChangesAsync(ct);
        return NoContent();
    }
}
