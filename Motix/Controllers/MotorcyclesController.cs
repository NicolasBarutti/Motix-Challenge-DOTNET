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
public class MotorcyclesController : ControllerBase
{
    private readonly MotixDbContext _ctx;
    public MotorcyclesController(MotixDbContext ctx) => _ctx = ctx;

    /// <summary>Lista motos com paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var paging = new PagingParameters(page, pageSize);
        var paged = await _ctx.Motorcycles.AsNoTracking()
            .OrderBy(m => m.Plate)
            .ToPagedAsync(paging, ct);

        var items = paged.Items.Cast<Motorcycle>().Select(m => new
        {
            data = new MotorcycleDto(m.Id, m.Plate, m.SectorId),
            _links = LinkFactory.MotorcycleLinks(HttpContext, m.Id)
        });

        return Ok(new PagedResult<object>(items, paged.Page, paged.PageSize, paged.TotalCount));
    }

    /// <summary>Obtém uma moto por ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MotorcycleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var m = await _ctx.Motorcycles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return NotFound();
        return Ok(new { data = new MotorcycleDto(m.Id, m.Plate, m.SectorId), _links = LinkFactory.MotorcycleLinks(HttpContext, m.Id) });
    }

    /// <summary>Cria uma moto.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MotorcycleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CreateMotorcycleDto input, CancellationToken ct = default)
    {
        if (input.SectorId == Guid.Empty)
            return BadRequest(new { error = "SectorId é obrigatório" });

        // ✅ valida FK explicitamente (necessário para EF InMemory e comportamento REST)
        var sectorExists = await _ctx.Sectors.AsNoTracking().AnyAsync(s => s.Id == input.SectorId, ct);
        if (!sectorExists)
            return BadRequest(new { error = "SectorId inexistente (violação de FK)" });

        if (string.IsNullOrWhiteSpace(input.Plate))
            return BadRequest(new { error = "Plate é obrigatório" });

        var m = new Motorcycle
        {
            Id = Guid.NewGuid(),
            Plate = input.Plate.Trim(),
            SectorId = input.SectorId
        };

        _ctx.Motorcycles.Add(m);
        await _ctx.SaveChangesAsync(ct);

        var dto = new MotorcycleDto(m.Id, m.Plate, m.SectorId);
        return CreatedAtAction(nameof(GetById), new { id = m.Id }, new { data = dto, _links = LinkFactory.MotorcycleLinks(HttpContext, m.Id) });
    }

    /// <summary>Atualiza uma moto.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateMotorcycleDto input, CancellationToken ct = default)
    {
        var m = await _ctx.Motorcycles.FindAsync(new object?[] { id }, ct);
        if (m is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(input.Plate))
            m.Plate = input.Plate.Trim();

        if (input.SectorId != Guid.Empty)
        {
            var sectorExists = await _ctx.Sectors.AsNoTracking().AnyAsync(s => s.Id == input.SectorId, ct);
            if (!sectorExists)
                return BadRequest(new { error = "SectorId inexistente (violação de FK)" });

            m.SectorId = input.SectorId;
        }

        await _ctx.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Remove uma moto.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var exists = await _ctx.Motorcycles.AsNoTracking().AnyAsync(x => x.Id == id, ct);
        if (!exists) return NotFound();

        _ctx.Motorcycles.Remove(new Motorcycle { Id = id });
        await _ctx.SaveChangesAsync(ct);
        return NoContent();
    }
}
