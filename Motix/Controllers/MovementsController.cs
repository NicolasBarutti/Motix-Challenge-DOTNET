using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motix.Application.DTOs;
using Motix.Domain.Entities;
using Motix.Extensions;
using Motix.Infrastructure.Persistence;
using Motix.Models;
using Motix.Services;

namespace Motix.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MovementsController : ControllerBase
{
    private readonly MotixDbContext _ctx;
    public MovementsController(MotixDbContext ctx) => _ctx = ctx;

    /// <summary>Lista movimentos com paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var paging = new PagingParameters(page, pageSize);

        var paged = await _ctx.Movements
            .AsNoTracking()
            .OrderByDescending(mv => mv.OccurredAt)
            .ToPagedAsync(paging, ct);

        var items = paged.Items.Cast<Movement>().Select(mv => new
        {
            data = new MovementDto(mv.Id, mv.MotorcycleId, mv.SectorId, mv.OccurredAt),
            _links = LinkFactory.MovementLinks(HttpContext, mv.Id)
        });

        return Ok(new PagedResult<object>(items, paged.Page, paged.PageSize, paged.TotalCount));
    }

    /// <summary>Obtém um movimento por ID.</summary>
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

    /// <summary>Cria um movimento (moto foi para setor).</summary>
    /// <remarks>
    /// Exemplo:
    /// 
    /// POST /api/v1/movements
    /// { "motorcycleId": "GUID_MOTO", "sectorId": "GUID_SETOR" }
    /// 
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(MovementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CreateMovementDto input, CancellationToken ct = default)
    {
        if (input.MotorcycleId == Guid.Empty || input.SectorId == Guid.Empty)
            return BadRequest(new { error = "MotorcycleId e SectorId são obrigatórios" });

        // ⚠️ Verificações explícitas de FK (necessárias para testes com InMemory e comportamento REST correto)
        var motoExists = await _ctx.Motorcycles.AsNoTracking().AnyAsync(m => m.Id == input.MotorcycleId, ct);
        if (!motoExists)
            return BadRequest(new { error = "MotorcycleId inexistente (violação de FK)" });

        var sectorExists = await _ctx.Sectors.AsNoTracking().AnyAsync(s => s.Id == input.SectorId, ct);
        if (!sectorExists)
            return BadRequest(new { error = "SectorId inexistente (violação de FK)" });

        var mv = new Movement
        {
            Id = Guid.NewGuid(),
            MotorcycleId = input.MotorcycleId,
            SectorId = input.SectorId,
            OccurredAt = DateTimeOffset.UtcNow
        };

        _ctx.Movements.Add(mv);
        await _ctx.SaveChangesAsync(ct);

        var dto = new MovementDto(mv.Id, mv.MotorcycleId, mv.SectorId, mv.OccurredAt);
        return CreatedAtAction(nameof(GetById), new { id = mv.Id },
            new { data = dto, _links = LinkFactory.MovementLinks(HttpContext, mv.Id) });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        // Torna o DELETE idempotente (sempre retorna 204)
        var exists = await _ctx.Movements.AsNoTracking().AnyAsync(m => m.Id == id, ct);

        if (exists)
        {
            // Remove sem precisar carregar a entidade completa
            _ctx.Movements.Remove(new Movement { Id = id });
            await _ctx.SaveChangesAsync(ct);
        }

        // Mesmo se já não existir, retorna 204 (NoContent)
        return NoContent();
    }

}
